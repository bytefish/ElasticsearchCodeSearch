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

function Get-LatestCommitDate {
    param (
        [Parameter(Mandatory)]
        [string]$Directory,
        
        [Parameter(Mandatory)]
        [string]$File
    )
    
    $latestCommitDate = git --git-dir "$Directory/.git" log -1  --date=format-local:'%Y-%m-%d %H:%M:%S' --format="%ad" -- $File 2>&1
    
    return $latestCommitDate
}

function Send-FileIndexRequest {
    param (
        [Parameter(Mandatory)]
        [string]$Endpoint,

        [Parameter(Mandatory)]
        [string]$Repository,

        [Parameter(Mandatory)]
        [string]$File,
       
        [Parameter(Mandatory)]
        [string]$LatestCommitDate,
        
        [Parameter(Mandatory)]
        [string]$Content
    )
    
}

# The organization (or user) we are going to index all repositories 
# from. This should be passed as a parameter in a later version ...
$organization = "microsoft"

# The Url, where the Index Service is running at. This is the ASP.NET 
# Core WebAPI, which is responsible to send the indexing requests ...
$indexServiceUrl = ""

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
        
        # Repository Path.
        $repositoryDirectory = $baseDirectory + "\" + $_.name
            
        # Get all files in the repositrory:
        $files = Git-BuildFileList -Directory $repositoryDirectory

        # We want to create Bulk Requests, so we don't send a million
        # Requests to the Elasticsearch API. .NET 6 now comes with a 
        # Enumerable#Chunk to partition data, let's use it.
        $chunks =  [System.Linq.Enumerable]::Chunk($files, 100)
        
        # Process all File Chunks in Parallel. This allows us to send 
        # Bulk Requests to the Elasticsearch API, without complex code 
        # ...
        $chunks | ForEach-Object {  

            # Holds the File Index Data, that we are going to send 
            # to Elasticsearch for indexing.
            $fileIndexDataList = @()
            
            # Each Chunk contains a list of files.
            foreach($file in $_) {
                
                # Read the Content, that should be indexed, we may 
                # exceed memory limits, on very large files, but who 
                # cares...
                $content = Get-Content -Path "$repository\$file"
                
                # Get the latest Commit Date from the File, so we 
                # can later sort by commit date, which is the only 
                # reason for building this thing...
                $latestCommitDate = Get-LatestCommitDate -Directory $repositoryDirectory -File $file
                
                $fileIndexData = @{
                    Filename = $file
                    Content = $content
                    LatestCommitDate = $LatestCommitDate
                }
                
                $fileIndexDataList += $fileIndexData
            }
            
            # Build the actual HTTP Request.
            $fileIndexRequest = @{
                Method = "POST"
                Uri = $indexServiceUrl
                Body = ($fileIndexDataList | Convert-ToJson)
                ContentType = "application/json"
            }
            
            Write-Host "[REQ]"
            Write-Host "[REQ] File Index Request"
            Write-Host "[REQ]"
            Write-Host "[REQ]   URL:            $indexServiceUrl"
            Write-Host "[REQ]   File Count:     $($fileIndexDataList.Length)"
            Write-Host "[REQ]"

            # And Invoke it
            $fileIndexResponse = Invoke-RestMethod @fileIndexRequest
            
            
            
        } -ThrottleLimit 25
    }
    finally {
        Git-ForceDeleteRepository -Directory $repository
    }
    
} -ThrottleLimit 4