<#
.SYNOPSIS
    Code Indexer for the Elasticsearch Code Search.
#>

# The Write-Log function to write Log Messages for Repositories in a standard 
# format and assign a Severity to it.
function Write-Log {
    param(

        [Parameter(Mandatory = $false)]    
        [ValidateSet('Debug', 'Info', 'Warn', 'Error', IgnoreCase = $false)]
        [string]$Severity = "Info",

        [Parameter(Mandatory = $true)]
        [string]$Repository,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )
    
    $timestamp = Get-Date  -Format 'hh:mm:ss'
    $threadId = [System.Threading.Thread]::CurrentThread.ManagedThreadId

    Write-Output "$timestamp $Severity [$threadId] $Repository $Message"
}

# We are going to use Parallel Blocks, and they don't play nice 
# with importing modules and such. It's such a simple function, 
# we are just going to source it in each Script Block.
$writeLogDef = ${function:Write-Log}.ToString()

# The AppConfig, where you can configure the Indexer. It allows 
# you to set the owner to index, the URL to send documents, 
# and whitelist filenames and extensions. 
$appConfig = @{
    # The organization (or user) we are going to index all repositories 
    # from. This should be passed as a parameter in a later version ...
    Owner = "microsoft"
    # We want to index repositories with a maximum of 700 MB initially, so we 
    # filter all large directories ...
    MaxDiskUsageInKilobytes = 100 * 1024
    # Maximum Numbers of GitHub Repositories to process. This needs to be 
    # passed to the gh CLI, because it doesn't support pagination.
    MaxNumberOfRepositories = 1000
    # Only indexes repositories updated between these timestamps
    UpdatedBetween = @([datetime]::Parse("2020-01-01T00:00:00Z"), [datetime]::Parse("9999-12-31T00:00:00Z"))
    # A flag no index archived repositories or not.
    IndexArchived = $false
    # LogFile to write the logs to. We don't print to screen directly, because 
    # we cannot trust the results.
    LogFile = "C:\Temp\log_indexer.log"
    # The Url, where the Index Service is running at. This is the ASP.NET 
    # Core WebAPI, which is responsible to send the indexing requests ...
    CodeSearchIndexUrl = "http://localhost:5000/index-documents"
    # This is where we are going to clone the temporary Git repositories to, 
    # which will be created for reading the file content and sending it to 
    # the ElasticSearch instance.
    BaseDirectory = "C:\Temp"
    # Files allowed for processing.
    AllowedFilenames = (
        ".gitignore",
        ".editorconfig",
        "README",
        "CHANGELOG"
    )
    # Extensions allowed for processing.
    AllowedExtensions = (
        # C / C++
        ".c",
        ".cpp",
        ".h",
        ".hpp",
        # .NET
        ".cs", 
        ".cshtml", 
        ".csproj", 
        ".fs",
        ".razor", 
        ".sln",
        ".xaml",
        # CSS
        ".css",
        ".scss",
        ".sass",
        # CSV / TSV
        ".csv",
        ".tsv",
        # HTML
        ".html", 
        ".htm",
        # JSON
        ".json", 
        # JavaScript
        ".js",
        ".jsx",
        ".spec.js",
        ".config.js",
        # Typescript
        ".ts", 
        ".tsx", 
        # TXT
        ".txt", 
        # Powershell
        ".ps1",
        # Python
        ".py",
        # Configuration
        ".ini",
        ".config",
        # XML
        ".xml",
        ".xsl",
        ".xsd",
        ".dtd",
        ".wsdl",
        # Markdown
        ".md",
        ".markdown",
        # reStructured Text
        ".rst",
        # LaTeX
        ".tex",
        ".bib",
        ".bbx",
        ".cbx"
    )
    # Batch Size for Elasticsearch Requests.
    BatchSize = 30
    # Throttles the number of parallel clones. This way we can 
    # clone multiple repositories in parallel, so we don't have 
    # to wait for each clone.
    MaxParallelClones = 1
    # Throttles the number of parallel bulk index requests to 
    # the backend, so we can send multiple requests in parallel 
    # and don't need to wait for the request to return.
    MaxParallelBulkRequests = 10
}

# Start Writing the Log
Start-Transcript $appConfig.LogFile -Append

# Get all GitHub Repositories for an organization or a user, using the GitHub CLI.

$repositories = gh repo list $appConfig.Owner --limit $appConfig.MaxNumberOfRepositories --json id,name,owner,nameWithOwner,languages,url,sshUrl,diskUsage,updatedAt
    | ConvertFrom-Json    
    | Where-Object {$_.diskUsage -lt $appConfig.MaxDiskUsageInKilobytes} 
    | Where-Object { ($_.updatedAt -ge $appConfig.UpdatedBetween[0]) -and  ($_.updatedAt -le $appConfig.UpdatedBetween[1]) }

Write-Host ($repositories | Select-Object -Property name,updatedAt)

# Index all files of the organization or user, cloning is going to take some time, 
# so we should probably be able to clone and process 4 repositories in parallel. We 
# need to check if it reduces the waiting time ...
$repositories | ForEach-Object -ThrottleLimit $appConfig.MaxParallelClones -Parallel {

    # We need to re-assign the defintion, because we are 
    # going to have another nested Parallel block, which 
    # needs to source the function.
    $writeLogDef = $using:writeLogDef;

    # Source the Write-Log function, so we can use it in the
    # Parallel ScriptBlock. Somewhat ugly, but I don't know a 
    # good way around.
    ${function:Write-Log} = $using:writeLogDef

    # Get the global AppConfig.
    $appConfig = $using:appConfig

    # Rename, so we know what we are operating on.
    $repository = $_

    # Repository Name.
    $repositoryName = $repository.name
    
    # Repository URL.
    $repositoryUrl = $repository.url

    # Repository URL.
    $repositoryOwner = $repository.owner.login
    
    # Repository Path.
    $repositoryDirectory = "$($appConfig.BaseDirectory)\$repositoryName"
       
    Write-Log -Severity Debug -Repository $repositoryName -Message "Processing started ..."
    
    # Wrap the whole thing in a try - finally, so we always delete the repositories.
    try {
        
        # If the Repository already exists, we don't need to clone it again. This could be problematic, 
        # when the Repository has been updated in between, but the idea is to re-index the entire 
        # organization in case of error.
        if(Test-Path $repositoryDirectory) {
            Write-Log -Severity Debug -Repository $repositoryName -Message "Directory '$repositoryDirectory' already exists"
        } else {
            Write-Log -Severity Debug -Repository $repositoryName -Message "Cloning to '$repositoryDirectory'"

            git clone $repositoryUrl $repositoryDirectory 2>&1 | Out-Null
        }
                
        # Get all files in the repositrory using the GIT CLI. This command 
        # returns relative filenames starting at the Repository Path.
        $relativeFilepathsFromGit = git --git-dir "$repositoryDirectory/.git" ls-files 2>&1 
            | ForEach-Object {$_ -Split "`r`n"}
            
        # We get all files, but images and so on are probably too large to index. I want 
        # to start by whitelisting some extensions. If this leads to crappy results, we 
        # will try blacklisting ...
        $relativeFilepaths = @()
        
        foreach($relativeFilepath in $relativeFilepathsFromGit) {
            
            # Get the filename from the relative Path, we use it to check 
            # against a set of whitelisted files, which we can read the data 
            # from.
            $filename = [System.IO.Path]::GetFileName($relativeFilepath)
            
            # We need to get the Extension for the given File, so we can add it.
            # This ignores all files like CHANGELOG, README, ... we may need some 
            # additional logic here.
            $extension = [System.IO.Path]::GetExtension($relativeFilepath)    
            
            # If the filename or extension is allowed, we are adding it to the 
            # list of files to process. Don't add duplicate files.
            if($appConfig.AllowedFilenames -contains $filename) {
                $relativeFilepaths += $relativeFilepath
            } elseif($appConfig.AllowedExtensions -contains $extension) {
                $relativeFilepaths += $relativeFilepath
            }
        }

        Write-Log -Severity Debug -Repository $repositoryName -Message "$($relativeFilepaths.Length)' files to Process ..."
        
        # We want to create Bulk Requests, so we don't send a million
        # Requests to the Elasticsearch API. We will use Skip and Take, 
        # probably not efficient, but who cares.
        $batches = @()
        
        for($batchStartIdx = 0; $batchStartIdx -lt $relativeFilepaths.Length; $batchStartIdx += $appConfig.BatchSize) {
            
            # A Batch is going to hold all information for processing the Data in parallel, so 
            # we don't have to introduce race conditions, when sharing variables on different 
            # threads.
            $batch = @{
                RepositoryName = $repositoryName
                RepositoryOwner = $repositoryOwner
                RepositoryUrl = $repositoryUrl
                RepositoryDirectory = $repositoryDirectory
                Elements = @($relativeFilepaths
                    | Select-Object -Skip $batchStartIdx
                    | Select-Object -First $appConfig.BatchSize)
            }
            
            # Add the current batch to the list of batches 
            $batches += , $batch
        }

        Write-Log -Severity Debug -Repository $repositoryName -Message "'$($batches.Length)' Bulk Index Requests will be sent to Indexing Service"
        
        # Process all File Chunks in Parallel. This allows us to send 
        # Bulk Requests to the Elasticsearch API, without complex code 
        # ...
        $batches | ForEach-Object -ThrottleLimit $appConfig.MaxParallelBulkRequests -Parallel {  
            
            # Source the Write-Log function, so we can use it in the
            # Parallel ScriptBlock. Somewhat ugly, but I don't know a 
            # good way around.
            ${function:Write-Log} = $using:writeLogDef

            # Get the global AppConfig.
            $appConfig = $using:appConfig

            # Rename, so we know what we are working with.
            $batch = $_
            
            # We need the variables from the outer scope...
            $repositoryDirectory = $batch.RepositoryDirectory
            $repositoryOwner = $batch.RepositoryOwner
            $repositoryName = $batch.RepositoryName
            
            # Holds the File Index Data, that we are going to send 
            # to Elasticsearch for indexing.
            $codeSearchDocumentList = @()
                        
            # Each batch contains a list of files.
            foreach($relativeFilepath in $batch.Elements) {
                
                # Apparently git sometimes returns " around the Filenames,
                # "optimistically" we trim it at the begin and end, and 
                # hope it works...
                $relativeFilepath = $relativeFilepath.Trim("`"")
                
                # We need the absolute filename for the Powershell Utility functions,
                # so we concatenate the path to the repository with the relative filename 
                # as returned by git.
                $absoluteFilepath = "{0}\{1}" -f $repositoryDirectory,$relativeFilepath
                
                # Sometimes there is an issue, that the files returned by git are empty 
                # directories on Windows. Who knows, why? We don't want to index them, 
                # because they won't have content.
                #
                # We could filter this out in the pipe, but better we print the problematic 
                # filenames for further investigation
                if(Test-Path -Path $absoluteFilepath -PathType Container) {
                    Write-Log -Severity Warn -Repository $repositoryName -Message "The given Filename is a directory: '$absoluteFilepath'"
                    continue
                }
                
                # Totally valid, that GIT returns garbage! Probably a file isn't on disk 
                # actually, who knows how all this stuff behaves anyway? What should we 
                # do here? Best we can do... print the problem and call it a day.
                if((Test-Path $absoluteFilepath) -eq $false) {
                    Write-Log -Severity Warn -Repository $repositoryName -Message "The given Filename does not exist: '$absoluteFilepath'"
                    continue
                }
                
                # Read the Content, that should be indexed, we may 
                # exceed memory limits, on very large files, but who 
                # cares...
                $content  = $null
                
                try {
                    $content = Get-Content -Path $absoluteFilepath -Raw -ErrorAction Stop
                } catch {
                    Write-Log -Severity Warn -Repository $repositoryName -Message ("[ERR] Failed to read file content: " + $_.Exception.Message)
                    continue
                }
                
                # Gets the SHA1 Hash of the Git File. We need this to reconstruct the URL to the GitHub 
                # file, so we have a unique identitfier for the file and we are able to generate a link 
                # in the Frontend.
                $sha1Hash = git --git-dir "$repositoryDirectory\.git" ls-files -s $relativeFilepath 2>&1
                    | ForEach-Object {$_ -Split " "} # Split at Whitespaces
                    | Select-Object -Skip 1 -First 1
                
                # We need the Commit Hash for the Permalink and 
                $commitHash = git --git-dir "$repositoryDirectory\.git" log --pretty=format:"%H" -n 1 -- $relativeFilepath 2>&1

                # Get the latest Commit Date from the File, so we 
                # can later sort by commit date, which is the only 
                # reason for building this thing...
                $latestCommitDate = git --git-dir "$repositoryDirectory\.git" log -1  --date=iso-strict --format="%ad" -- $relativeFilepath 2>&1
                          
                # We are generating a Permalink to the file, which is based on the owner, repository, SHA1 Hash 
                # of the commit to the file and the relative filename inside the repo. This is a good way to link 
                # to it from the search page, without needing to serve it by ourselves.
                $permalink = "https://github.com/$repositoryOwner/$repositoryName/blob/$commitHash/$relativeFilepath"
                
                # The filename with an extension for the given path. 
                $filename = [System.IO.Path]::GetFileName($relativeFilepath)

                # This is the Document, which will be included in the 
                # bulk request to Elasticsearch. We will append it to 
                # a list. 
                # 
                # Since the Content should be a maximum of 1 MB, we should be on 
                # the safe side to not have the memory exploding.
                $codeSearchDocument = @{
                    id = $sha1Hash
                    owner = $repositoryOwner
                    repository = $repositoryName
                    path = $relativeFilepath
                    filename = $filename
                    commitHash = $commitHash
                    content = $content 
                    permalink = $permalink
                    latestCommitDate = $latestCommitDate
                }
                        
                # Holds all documents to be included in the Bulk Request.
                $codeSearchDocumentList += , $codeSearchDocument
            }

            # Build the actual HTTP Request.
            $codeSearchIndexRequest = @{
                Method = "POST"
                Uri = $appConfig.CodeSearchIndexUrl
                Body = ConvertTo-Json -InputObject $codeSearchDocumentList -EscapeHandling EscapeNonAscii # Don't use a Pipe here, so Single Arrays become a JSON array too
                ContentType = "application/json"
                StatusCodeVariable = 'statusCode'
            }
            
            Write-Log -Severity Debug -Repository $repositoryName -Message "Sending CodeIndexRequest with Document Count: $($codeSearchDocumentList.Length)"

            try {
                # Invokes the Requests, which will error out on HTTP Status Code >= 400, 
                # so we need to wrap it in a try / catch block. We can then extract the 
                # error message.
                $resp = Invoke-RestMethod @codeSearchIndexRequest

                Write-Log -Severity Debug -Repository $repositoryName -Message "CodeIndexRequest sent successfully with HTTP Status Code = $statusCode"
            } catch {
                Write-Log -Severity Error -Repository $repositoryName -Message ("CodeIndexRequest failed with Message: " + $_.Exception.Message)
            }
        }
    }
    finally {
        if($repositoryDirectory.StartsWith("C:\Temp")) {
            Write-Log -Repository $repositoryName -Severity Debug -Message "Deleting GIT Repository: $repositoryDirectory ..."
        
            Remove-Item -LiteralPath $repositoryDirectory -Force -Recurse
        }
    }
}

Stop-Transcript