<#
.SYNOPSIS
    Example Script for Sending Indexing Requests to an ElasticSearch Service.
#>

# The organization (or user) we are going to index all repositories 
# from. This should be passed as a parameter in a later version ...
$organization = "microsoft"

# We want to index repositories with a maximum of 700 MB initially, so we 
# filter all large directories ...
$maxDiskUsageInKilobytes = 100 * 1024

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
    
    # Extensions allowed for processing.
    $allowedExtensions = (".txt",
        ".csv",
        ".tsv",
        ".ts", 
        ".tsx", 
        ".html", 
        ".htm",
        ".css",
        ".scss",
        ".sass",
        ".txt", 
        ".cs", 
        ".razor", 
        ".cshtml", 
        ".json", 
        ".csproj", 
        ".xml",
        ".sln",
        ".java",
        ".js",
        ".jsx",
        ".c",
        ".h",
        ".hpp",
        ".cpp",
        ".md",
        ".gitignore",
        ".editorconfig")
       
    Write-Host "Processing '$repositoryName' ..."
    
    # Wrap the whole thing in a try / catch, so we always delete the repositories, 
    # when we have hit an error of are stuck.
    try {
        
        # If the Repository already exists, we don't need to clone it again...
        if(Test-Path $repositoryDirectory) {
            Write-Host "[$repositoryName] Directory '$repositoryDirectory' already exists. Not Cloning ...."
        } else {
            Write-Host "[$repositoryName]: Repository does not exist. Cloning to '$repositoryDirectory' ..."
        
            git clone $repositoryUrl $repositoryDirectory
        }
                
        # Get all files in the repositrory using the GIT CLI. This command 
        # returns relative filenames starting at the Repository Path.
        $relativeFilenamesFromGit = git --git-dir "$repositoryDirectory/.git" ls-files 2>&1 
            | % {$_ -Split "`r`n"}
            
        # We get all files, but images and so on are probably too large to index. I want 
        # to start by whitelisting some extensions. If this leads to crappy results, we 
        # will try blacklisting ...
        $relativeFilenames = @()
        
        foreach($relativeFilename in $relativeFilenamesFromGit) {
            # We need to get the Extension for the given File, so we can add it.
            # This ignores all files like CHANGELOG, README, ... we may need some 
            # additional logic here.
            $extension = [System.IO.Path]::GetExtension($relativeFilename)    
            
            # If the extension is in the whitelist of extensions, we add it 
            # to the files to process. This should filter out excessively large 
            # binary blobs, that we don't want to index anyway.
            if($allowedExtensions -contains $extension) {
                $relativeFilenames += $relativeFilename
            }
        }

        Write-Host "[$repositoryName] '$($relativeFilenames.Length)' files to Process ..."
        
        # We want to create Bulk Requests, so we don't send a million
        # Requests to the Elasticsearch API. We will use Skip and Take, 
        # probably not efficient, but who cares.
        $batches = @()
        $batchSize = 30
        
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
                Elements = @($relativeFilenames
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
                
                # Apparently git sometimes returns " around the Filenames,
                # "optimistically" we trim it at the begin and end, and 
                # hope it works...
                $relativeFilename = $relativeFilename.Trim("`"")
                
                # We need the absolute filename for the Powershell Utility functions,
                # so we concatenate the path to the repository with the relative filename 
                # as returned by git.
                $absoluteFilename = "{0}\{1}" -f $repositoryDirectory,$relativeFilename
                
                # Sometimes there is an issue, that the files returned by git are empty 
                # directories on Windows. Who knows, why? We don't want to index them, 
                # because they won't have content.
                #
                # We could filter this out in the pipe, but better we print the problematic 
                # filenames for further investigation
                if(Test-Path -Path $absoluteFilename -PathType Container) {
                    Write-Host "[ERR] The given Filename is a directory: '$absoluteFilename'" -ForegroundColor Red
                    
                    continue
                }
                
                # Totally valid, that GIT returns garbage! Probably a file isn't on disk 
                # actually, who knows how all this stuff behaves anyway? What should we 
                # do here? Best we can do... print the problem and call it a day.
                if((Test-Path $absoluteFilename) -eq $false) {
                    Write-Host "[ERR] The given Filename does not exist: '$absoluteFilename'" -ForegroundColor Red
                    
                    continue
                }
                
                # Read the Content, that should be indexed, we may 
                # exceed memory limits, on very large files, but who 
                # cares...
                $content  = $null
                
                try {
                    $content = Get-Content -Path $absoluteFilename -Raw -ErrorAction Stop
                } catch {
                    Write-Host ("[ERR] Failed to read file content: " + $_.Exception.Message) -ForegroundColor Red
                    
                    continue
                }
                
                if($content) {
                    # Get the Content as UTF8 Bytes, which will be encoded as 
                    # Base64 and decoded on the other side, so we don't have to 
                    # deal with JSON issues serializing the text content.
                    $contentBytes = [System.Text.Encoding]::UTF8.GetBytes($content)
                    
                    # Why do we need Base64 at all? Because the ConvertTo-Json cmdlet 
                    # has all kinds of Bugs, when it is used with the Content returned 
                    # by Get-Content. 
                    #
                    # I was too lazy to find out ...
                    $contentBase64 = [System.Convert]::ToBase64String($contentBytes)
                    
                    # We have at most 30 Megabytes in a request, everything else is excessive. How 
                    # fast will we reach it with Base64 encoding? What do I know. Could we split 
                    # the text? Probably...                   
                    if([System.Text.Encoding]::UTF8.GetByteCount($contentBase64) -gt (1 * 1024 * 1024)) {
                        Write-Host "[ERR] The given content exceeds the max Content Size: '$absoluteFilename'" -ForegroundColor Red
                        continue
                    }
                }
                
                # We need a unique identifier. I failed to extract the 
                # GIT Blob Hash from the GIT CLI, so I am just calculating 
                # the MD5 Hash...
                $md5Hash = (Get-FileHash $absoluteFilename).Hash
                                    
                # Get the latest Commit Date from the File, so we 
                # can later sort by commit date, which is the only 
                # reason for building this thing...
                $latestCommitDate = git --git-dir "$repositoryDirectory\.git" log -1  --date=iso-strict --format="%ad" -- $relativeFilename 2>&1
                                
                # This is the Document, which will be included in the 
                # bulk request to Elasticsearch. We will append it to 
                # a list. 
                # 
                # Since the Content should be a maximum of 1 MB, we should be on 
                # the safe side to not have the memory exploding.
                $codeSearchDocument = @{
                    id = $md5Hash
                    owner = $repositoryOwner
                    repository = $repositoryName
                    filename = $relativeFilename
                    content = $contentBase64 
                    latestCommitDate = $latestCommitDate
                }
                        
                # Holds all documents to be included in the Bulk Request.
                $codeSearchDocumentList += , $codeSearchDocument
            }

            # Build the actual HTTP Request.
            $codeSearchIndexRequest = @{
                Method = "POST"
                Uri = $codeSearchIndexUrl
                Body = ConvertTo-Json $codeSearchDocumentList # Don't use a Pipe here, so Single Arrays become a JSON array too
                ContentType = "application/json"
                StatusCodeVariable = 'statusCode'
            }
            
            $requestMessage = ("[$repositoryName][REQ] Code Search Index Request`n" +
                               "[$repositoryName][REQ]`n" +
                               "[$repositoryName][REQ]     URL:            $codeSearchIndexUrl`n" +
                               "[$repositoryName][REQ]     File Count:     $($codeSearchDocumentList.Length)`n" + 
                               "[$repositoryName][REQ]`n")

            try {
                # Invokes the Requests, which will error out on HTTP Status Code >= 400, 
                # so we need to wrap it in a try / catch block. We can then extract the 
                # error message.
                $codeSearchIndexResponse = Invoke-RestMethod @codeSearchIndexRequest
                           
                Write-Host ($requestMessage + "[$repositoryName][RES]     HTTP Status:    $statusCode") -ForegroundColor Green
            } catch {
                
                Write-Host (ConvertTo-Json $codeSearchDocumentList)
                
                Write-Host ($requestMessage + "[ERR] Request failed with Error Message: " + $_.Exception.Message) -ForegroundColor Red
            }
        
        } -ThrottleLimit 10
    }
    finally {
        Write-Host "Deleting GIT Repository: $repositoryDirectory ..."
        
        if($repositoryDirectory.StartsWith("C:\Temp")) {
            Remove-Item -LiteralPath $repositoryDirectory -Force -Recurse
        }
    }
} -ThrottleLimit 1