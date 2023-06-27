<#
.SYNOPSIS
    Example Script for Sending Indexing Requests to an ElasticSearch Service.
.DESCRIPTION
    
#>

# The organization (or user) we are going to index all repositories 
# from. This should be passed as a parameter in a later version ...
$organization = "microsoft"

# We want to index repositories with a maximum of 700 MB initially, so we 
# filter all large directories ...
$maxDiskUsageInKilobytes = 10 * 1024

# Get all GitHub Repositories for an organization or a user, using the GitHub CLI.
$repositories = gh repo list $Organization --json id,name,owner,nameWithOwner,languages,url,sshUrl,diskUsage
                     | ConvertFrom-Json                                         
                     | Where-Object {$_.diskUsage -lt $maxDiskUsageInKilobytes} 
                                        
# Index all files of the organization or user, cloning is going to take some time, 
# so we should probably be able to clone and process 4 repositories in parallel. We 
# need to check if it reduces the waiting time ...
$repositories | ForEach-Object -Parallel {

    # The Url, where the Index Service is running at. This is the ASP.NET 
    # Core WebAPI, which is responsible to send the indexing requests ...
    $codeSearchIndexUrl = "http://localhost:5000/api/index"

    # This is where we are going to clone the temporary Git repositories to, 
    # which will be created for reading the file content and sending it to 
    # the ElasticSearch instance.
    $baseDirectory = "C:\Temp"

    # Repository Name.
    $repositoryName = $_.name
    
    # Repository URL.
    $repositoryUrl = $_.url

    # Repository URL.
    $repositoryOwner = $_.owner.login
    
    # Repository Path.
    $repositoryDirectory = "$baseDirectory\$repositoryName"
                
    Write-Host "Processing '$repositoryName' ..."
    
    # Wrap the whole thing in a try / catch, so we always delete the repositories, 
    # when we have hit an error of are stuck.
    try {
        
        if(Test-Path $repositoryDirectory) {
            Write-Host "[$repositoryName] Directory '$repositoryDirectory' already exists. Not Cloning ...."
        } else {
            Write-Host "[$repositoryName]: Repository does not exist. Cloning to '$repositoryDirectory' ..."
        
            git clone $repositoryUrl $repositoryDirectory
        }
                
        # Get all files in the repositrory using the GIT CLI. This command 
        # returns relative filenames starting at the Repository Path.
        $relativeFilenames = git --git-dir "$repositoryDirectory/.git" ls-files 2>&1 
            | % {$_ -Split "`r`n"}

        Write-Host "[$repositoryName] '$($relativeFilenames.Length)' Files to Process ..."
        
        # We want to create Bulk Requests, so we don't send a million
        # Requests to the Elasticsearch API. We will use Skip and Take, 
        # probably not efficient, but who cares.
        $batches = @()
        $batchSize = 20
        
        for($batchStartIdx = 0; $batchStartIdx -lt $relativeFilenames.Length; $batchStartIdx += $batchSize) {
            
            # A Batch is going to hold all information for processing the Data in parallel, so 
            # we don't have to introduce race conditions, when sharing variables on different 
            # threads.
            $batch = @{
                RepositoryName = $repositoryName
                RepositoryOwner = $repositoryOwner
                RepositoryUrl = $repositoryUrl
                RepositoryDirectory = $repositoryDirectory
                CodeSearchIndexUrl = $codeSearchIndexUrl
                Elements = ($relativeFilenames
                    | Select-Object -Skip $batchStartIdx
                    | Select-Object -First $batchSize)
            }
            
            # Add the current batch to the list of batches 
            $batches += , $batch
        }
        
        Write-Host "[$repositoryName] '$($batches.Length)' Bulk Index Requests will be sent to Indexing Service ..."
        
        # Process all File Chunks in Parallel. This allows us to send 
        # Bulk Requests to the Elasticsearch API, without complex code 
        # ...
        $batches | ForEach-Object -Parallel {  
    
            # We are iterating on a batch
            $batch = $_
            
            # We need the variables from the outer scope...
            $codeSearchIndexUrl = $batch.CodeSearchIndexUrl
            $repositoryDirectory = $batch.RepositoryDirectory
            $repositoryOwner = $batch.RepositoryOwner
            $repositoryName = $batch.RepositoryName
            $repositoryUrl = $batch.RepositoryUrl
            
            # Holds the File Index Data, that we are going to send 
            # to Elasticsearch for indexing.
            $codeSearchDocumentList = @()
                        
            # Each batch contains a list of files.
            foreach($relativeFilename in $batch.Elements) {
                
                # We need the absolute filename for the Powershell Utility functions,
                # so we concatenate the path to the repository with the relative filename 
                # as returned by git.
                $absoluteFilename = "{0}\{1}" -f $repositoryDirectory,$relativeFilename
                
                # Read the Content, that should be indexed, we may 
                # exceed memory limits, on very large files, but who 
                # cares...
                $content = Get-Content -Path $absoluteFilename -Raw
                
                # We need a unique identifier. I failed to extract the 
                # GIT Blob Hash from the GIT CLI, so I am just calculating 
                # the MD5 Hash...
                $md5Hash = (Get-FileHash $absoluteFilename).Hash
                                    
                # Get the latest Commit Date from the File, so we 
                # can later sort by commit date, which is the only 
                # reason for building this thing...
                $latestCommitDate = git --git-dir "$repositoryDirectory\.git" log -1  --date=iso-strict --format="%ad" -- $relativeFilename 2>&1
                                
                # This is the 
                $codeSearchDocument = @{
                    id = $md5Hash
                    owner = $repositoryOwner
                    repository = $repositoryName
                    filename = $relativeFilename
                    content = $content
                    latestCommitDate = $latestCommitDate
                }
                                
                $codeSearchDocumentList += , $codeSearchDocument
            }
                        
            # Build the actual HTTP Request.
            $codeSearchIndexRequest = @{
                Method = "POST"
                Uri = $codeSearchIndexUrl
                Body = ($codeSearchDocumentList | ConvertTo-Json)
                ContentType = "application/json"
                StatusCodeVariable = 'statusCode'
            }
            
            Write-Host "[REQ]"
            Write-Host "[REQ] Code Search Index Request"
            Write-Host "[REQ]"
            Write-Host "[REQ]   URL:            $codeSearchIndexUrl"
            Write-Host "[REQ]   File Count:     $($codeSearchDocumentList.Length)"
            Write-Host "[REQ]"

            try {
                # And Invoke it
                $codeSearchIndexResponse = Invoke-RestMethod @codeSearchIndexRequest
                           
                Write-Host "[RES]    HTTP Status:    $statusCode"  -ForegroundColor Green
            } catch {
                Write-Host "[ERR] Request failed with Error Message: " $_.Exception.Message -ForegroundColor Red
            }
        
        } -ThrottleLimit 1
    }
    finally {
        Write-Host "Deleting GIT Repository: $repositoryDirectory ..."
    }
} -ThrottleLimit 1