// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Git;
using ElasticsearchCodeSearch.Infrastructure;
using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ElasticsearchCodeSearch.Services
{
    /// <summary>
    /// Git Indexer.
    /// </summary>
    public class GitIndexerService
    {
        private readonly ILogger<GitIndexerService> _logger;

        private readonly GitIndexerOptions _options;

        private readonly GitExecutor _gitExecutor;
        private readonly PermalinkGenerator _permalinkGenerator;
        private readonly ElasticCodeSearchClient _elasticCodeSearchClient;

        public GitIndexerService(ILogger<GitIndexerService> logger, IOptions<GitIndexerOptions> options, GitExecutor gitExecutor, PermalinkGenerator permalinkGenerator, ElasticCodeSearchClient elasticCodeSearchClient)
        {
            _logger = logger;
            _options = options.Value;
            _gitExecutor = gitExecutor;
            _permalinkGenerator = permalinkGenerator;
            _elasticCodeSearchClient = elasticCodeSearchClient;
        }

        /// <summary>
        /// Creates the Search Elasticsearch Search Index to use for indexing and searching.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token to cancel the asynchronous operations</param>
        /// <returns>An awaitable Task</returns>
        public async Task CreateSearchIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticCodeSearchClient
                .CreateIndexAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Indexes a Git Repository using the given <see cref="GitRepositoryMetadata.CloneUrl"/>.
        /// </summary>
        /// <param name="repositoryMetadata">Metadata of the Repository</param>
        /// <param name="cancellationToken">CancellationToken to cancel asynchronous processing</param>
        /// <returns>An awaitable task</returns>
        public async ValueTask IndexRepositoryAsync(GitRepositoryMetadata repositoryMetadata, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            if (repositoryMetadata.CloneUrl == null)
            {
                _logger.LogWarning("Could not clone '{RepositoryFullName}', because it has no Clone URL'", repositoryMetadata.FullName);

                throw new InvalidOperationException($"A Clone was requested for '{repositoryMetadata.FullName}', but no CloneUrl was given");
            }

            var allowedFilenames = _options.AllowedFilenames.ToHashSet();
            var allowedExtensions = _options.AllowedExtensions.ToHashSet();

            var repositoryDirectory = GetRepositoryDirectory(repositoryMetadata);

            try
            {
                if (repositoryDirectory.StartsWith(_options.BaseDirectory))
                {
                    if (Directory.Exists(repositoryDirectory))
                    {
                        if(_logger.IsDebugEnabled())
                        {
                            _logger.LogDebug("Deleting Repository '{RepositoryFullName}' in Directory '{RepositoryDirectory}' for a fresh clone", 
                                repositoryMetadata.FullName,
                                repositoryDirectory);
                        }

                        DeleteReadOnlyDirectory(repositoryDirectory);
                    }
                }

                await _elasticCodeSearchClient
                    .DeleteByOwnerRepositoryAndBranchAsync(owner: repositoryMetadata.Owner, repository: repositoryMetadata.Name,branch: repositoryMetadata.Branch, cancellationToken)
                    .ConfigureAwait(false);

                // Clone into the given Directory
                await _gitExecutor
                    .Clone(repositoryMetadata.CloneUrl, repositoryDirectory, cancellationToken)
                    .ConfigureAwait(false);

                // Get the list of allowed files, by matching against allowed extensions (.c, .cpp, ...)
                // and allowed filenames (.gitignore, README, ...). We don't want to parse binary data.
                //
                // We want to process the files in parallel, so we saturate the Hardware a bit better. So 
                // the files to be processed are chunked.
                var allFiles = await _gitExecutor
                    .ListFiles(repositoryDirectory, cancellationToken)
                    .ConfigureAwait(false);

                var batches = allFiles
                    .Where(filename => IsAllowedFile(filename, allowedExtensions, allowedFilenames))
                    .Chunk(_options.BatchSize);

                var parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = _options.MaxParallelBulkRequests,
                    CancellationToken = cancellationToken
                };

                await Parallel
                    .ForEachAsync(source: batches, parallelOptions: parallelOptions, body: (source, cancellationToken) => IndexDocumentsAsync(repositoryMetadata, source, cancellationToken))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if(_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "Indexing Repository '{Repository}' failed", repositoryMetadata.Name);
                }

                throw;
            }
            finally
            {
                if (repositoryDirectory.StartsWith(_options.BaseDirectory))
                {
                    try
                    {
                        if (Directory.Exists(repositoryDirectory))
                        {
                            if(_logger.IsDebugEnabled())
                            {
                                _logger.LogDebug("Deleting existing Repository '{RepositoryDirectory}'", repositoryDirectory);
                            }

                            DeleteReadOnlyDirectory(repositoryDirectory);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error deleting '{Repository}'", repositoryMetadata.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, 
        /// they are flagged as normal and then deleted. This is required for GIT folders, else you cannot empty 
        /// the directory correctly.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        private static void DeleteReadOnlyDirectory(string directory)
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteReadOnlyDirectory(subdirectory);
            }

            foreach (var fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName);
                fileInfo.Attributes = FileAttributes.Normal;
                fileInfo.Delete();
            }

            Directory.Delete(directory);
        }

        /// <summary>
        /// Indexes a list of files for a given <see cref="GitRepositoryMetadata"/>.
        /// </summary>
        /// <param name="repositoryMetadata">Repository Metadata</param>
        /// <param name="files">List of Files</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>An awaitable <see cref="ValueTask"/></returns>
        public async ValueTask IndexDocumentsAsync(GitRepositoryMetadata repositoryMetadata, string[] files, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            List<CodeSearchDocument> documents = new List<CodeSearchDocument>();

            foreach (var file in files)
            {
                if (_logger.IsDebugEnabled()) 
                {
                    _logger.LogDebug("Start Processing File (Repository = '{RepositoryFullName}', Branch = '{Branch}', File = '{Filename}')",
                        repositoryMetadata.FullName, repositoryMetadata.Branch, file);
                }

                Stopwatch codeSearchDocumentStopwatch = Stopwatch.StartNew();

                var codeSearchDocument = await BuildCodeSearchDocumentAsync(repositoryMetadata, file, cancellationToken).ConfigureAwait(false);

                codeSearchDocumentStopwatch.Stop();

                if (codeSearchDocument != null)
                {
                    documents.Add(codeSearchDocument);
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Finished Processing File (Repository = '{RepositoryFullName}', Branch = '{Branch}', File = '{Filename}', ElapsedTimeInMilliseconds = '{ElapsedTimeInMilliseconds}')",
                        repositoryMetadata.FullName, repositoryMetadata.Branch, file, codeSearchDocumentStopwatch.ElapsedMilliseconds);
                }
            }

            if(_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Bulk Indexing '{DocumentsCount}' Documents", documents.Count);
            }

            await _elasticCodeSearchClient
                .BulkIndexAsync(documents, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the <see cref="CodeSearchDocumentDto"/> to be indexed.
        /// </summary>
        /// <param name="repository">Physical Repository</param>
        /// <param name="repositoryMetadata">Repository Metadata</param>
        /// <param name="relativeFilename">Filename to process</param>
        /// <returns>An awaitable Task with the <see cref="CodeSearchDocument"/></returns>
        private async Task<CodeSearchDocument> BuildCodeSearchDocumentAsync(GitRepositoryMetadata repositoryMetadata, string relativeFilename, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var repositoryDirectory = GetRepositoryDirectory(repositoryMetadata);

            // Get the Commit Information to index, such as the File SHA1 Hash, the Commit Hash, ...
            var shaHash = await _gitExecutor
                .SHA1(repositoryDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            var commitHash = await _gitExecutor
                .CommitHash(repositoryDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);
            
            var latestCommitDate = await _gitExecutor
                .LatestCommitDate(repositoryDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            // Get the absolute Filename, so we can read it
            var absoluteFilename = Path.Combine(repositoryDirectory, relativeFilename);

            // Read all Content off the File
            var content = File.ReadAllText(absoluteFilename);

            // Build the final CodeSearchDocument with all relevant information for indexing.
            var codeSearchDocument = new CodeSearchDocument
            {
                Id = shaHash,
                Path = relativeFilename,
                Repository = repositoryMetadata.Name,
                Owner = repositoryMetadata.Owner,
                Content = content,
                Branch = repositoryMetadata.Branch,
                Filename = Path.GetFileName(relativeFilename),
                CommitHash = commitHash,
                Permalink = _permalinkGenerator.GeneratePermalink(repositoryMetadata, commitHash, relativeFilename),
                LatestCommitDate = latestCommitDate
            };

            return codeSearchDocument;
        }

        /// <summary>
        /// Checks if the file should be indexed.
        /// </summary>
        /// <param name="source">Filename</param>
        /// <param name="allowedExtensions">Allowed set of extensions</param>
        /// <param name="allowedFilenames">Allowed set of filenames</param>
        /// <returns>true, if allowed; else false</returns>
        private bool IsAllowedFile(string source, HashSet<string> allowedExtensions, HashSet<string> allowedFilenames)
        {
            _logger.TraceMethodEntry();

            var relativeFilename = Path.GetFileName(source);

            if (allowedFilenames.Contains(relativeFilename))
            {
                return true;
            }

            var fileExtension = Path.GetExtension(source);

            if (allowedExtensions.Contains(fileExtension))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the current GIT Working Directory.
        /// </summary>
        /// <param name="repository">GitHub Repository to index</param>
        /// <returns></returns>
        private string GetRepositoryDirectory(GitRepositoryMetadata repository)
        {
            _logger.TraceMethodEntry();

            return Path.Combine(_options.BaseDirectory, repository.Name);
        }
    }
}