<#
.SYNOPSIS
    Example Script for Sending Indexing Requests to an ElasticSearch Service.
.DESCRIPTION
    
#>

function Git-CloneRepository {
    param (
        [Parameter(Mandatory)]
        [string]$Url
        
        [Parameter(Mandatory)]
        [string]$Directory
    )
    
    git clone $repository.url $Directory
}

function Git-BuildFileList {
    param (
        [Parameter(Mandatory)]
        [string]$Directory
    )
    
    $files = git --git-dir "$Directory/.git" ls-files 2>&1 
        | % {$_ -Split "`r`n"}
    
    return $files
}

function Git-ForceDeleteRepository {
    param (
        [Parameter(Mandatory)]
        [string]$Directory
    )
    
    Remove-Item -LiteralPath $Directory -Force -Recurse
}


function Git-LatestCommitDate {
    param (
        [Parameter(Mandatory)]
        [string]$Directory,
        
        [Parameter(Mandatory)]
        [string]$Filename
    )
    
    $latestCommitDate = git --git-dir "$Directory/.git" log -1  --date=format-local:'%Y-%m-%d %H:%M:%S' --format="%ad" -- $File 2>&1
    
    return $latestCommitDate
}

# The organization (or user) we are going to index all repositories 
# from. This should be passed as a parameter in a later version ...
$organization = "microsoft"

# The Url, where the Index Service is running at. This is the ASP.NET 
# Core WebAPI, which is responsible to send the indexing requests ...
$codeSearchIndexUrl = "http://localhost:5000/api/index"

# This is where we are going to clone the temporary Git repositories to, 
# which will be created for reading the file content and sending it to 
# the ElasticSearch instance.
$baseDirectory = "C:\Temp"

# We want to index repositories with a maximum of 700 MB initially, so we 
# filter all large directories ...
$maxDiskUsageInKilobytes = 700 * 1024 * 1024

# Get all GitHub Repositories for an organization or a user, using the GitHub CLI.
$repositories = gh repo list $Organization --json id,name,nameWithOwner,languages,url,sshUrl,diskUsage
                     | ConvertFrom-Json                                         
                     | Where-Object {$_.diskUsage -lt $maxDiskUsageInKilobytes} 
                    
# Index all files of the organization or user, cloning is going to take some time, 
# so we should probably be able to clone and process 4 repositories in parallel. We 
# need to check if it reduces the waiting time ...
$repositories | ForEach-Object -parallel {

    # Wrap the whole thing in a try / catch, so we always delete the repositories, 
    # when we have hit an error of are stuck.
    try {
        
        $repositoryName = $_.name
        
        # Repository Path.
        $repositoryDirectory = $baseDirectory + "\" + $repositoryName
            
        # Get all files in the repositrory using the GIT CLI. This command 
        # returns relative filenames starting at the Repository Path.
        $relativeFilenames = Git-BuildFileList -Directory $repositoryDirectory

        # We want to create Bulk Requests, so we don't send a million
        # Requests to the Elasticsearch API. .NET 6 now comes with a 
        # Enumerable#Chunk to partition data, let's use it.
        $chunks =  [System.Linq.Enumerable]::Chunk($relativeFilenames, 100)
        
        # Process all File Chunks in Parallel. This allows us to send 
        # Bulk Requests to the Elasticsearch API, without complex code 
        # ...
        $chunks | ForEach-Object {  

            # Holds the File Index Data, that we are going to send 
            # to Elasticsearch for indexing.
            $codeSearchDocumentList = @()
            
            # Each Chunk contains a list of files.
            foreach($relativeFilename ile in $_) {
                
                # We need the absolute filename for the Powershell Utility functions,
                # so we concatenate the path to the repository with the relative filename 
                # as returned by git.
                $absoluteFilename = $repositoryDirectory + "\" + $relativeFilename
                
                # Read the Content, that should be indexed, we may 
                # exceed memory limits, on very large files, but who 
                # cares...
                $content = Get-Content -Path $absoluteFilename
             
                # We need a unique identifier. I failed to extract the 
                # GIT Blob Hash from the GIT CLI, so I am just calculating 
                # the MD5 Hash...
                $md5Hash = Get-FileHash $absoluteFilename
                
                # Get the latest Commit Date from the File, so we 
                # can later sort by commit date, which is the only 
                # reason for building this thing...
                $latestCommitDate = Get-LatestCommitDate -Directory $repositoryDirectory -Filename $relativeFilename
                
                # This is the 
                $codeSearchDocument = @{
                    Id = $md5Hash
                    Owner = $owner
                    Repository = $repository
                    Filename = $relativeFilename
                    Content = $content
                    LatestCommitDate = $LatestCommitDate
                }
                
                $codeSearchDocumentList += $codeSearchDocument
            }
            
            # Build the actual HTTP Request.
            $codeSearchIndexRequest = @{
                Method = "POST"
                Uri = $codeSearchIndexUrl
                Body = ($codeSearchDocumentList | Convert-ToJson)
                ContentType = "application/json"
                StatusCodeVariable = 'statusCode'
            }
            
            Write-Host "[REQ]"
            Write-Host "[REQ] Code Search Index Request"
            Write-Host "[REQ]"
            Write-Host "[REQ]   URL:            $indexServiceUrl"
            Write-Host "[REQ]   File Count:     $($codeSearchDocumentList.Length)"
            Write-Host "[REQ]"

            # And Invoke it
            $codeSearchIndexResponse = Invoke-RestMethod @codeSearchIndexRequest
                       
            Write-Host "[RES]    HTTP Status:    $statusCode"  -ForegroundColor Green
        
        } -ThrottleLimit 25
    }
    finally {
        Git-ForceDeleteRepository -Directory $repository
    }
    
} -ThrottleLimit 4