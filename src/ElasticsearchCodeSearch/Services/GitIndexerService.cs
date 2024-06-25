// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.Git;
using ElasticsearchCodeSearch.Indexer.GitHub;
using ElasticsearchCodeSearch.Indexer.GitHub.Dto;
using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Options;

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
        private readonly GitHubClient _gitHubClient;
        private readonly ElasticCodeSearchClient _elasticCodeSearchClient;

        public GitIndexerService(ILogger<GitIndexerService> logger, IOptions<GitIndexerOptions> options, GitExecutor gitExecutor, GitHubClient gitHubClient, ElasticCodeSearchClient elasticCodeSearchClient)
        {
            _logger = logger;
            _options = options.Value;
            _gitExecutor = gitExecutor;
            _gitHubClient = gitHubClient;
            _elasticCodeSearchClient = elasticCodeSearchClient;
        }

        public async Task CreateSearchIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticCodeSearchClient.CreateIndexAsync(cancellationToken);
        }

        public async Task IndexOrganizationAsync(string organization, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // If we are instructed to index an organization, we start by deleting all their
            // documents. The reasoning is simple: We don't want to introduce complex
            // synchronization code to diff the repositories.
            await _elasticCodeSearchClient.DeleteByOwnerAsync(organization, cancellationToken);

            // Gets all Repositories of the Organization:
            var repositories = await _gitHubClient
                .GetAllRepositoriesByOrganizationAsync(organization, 20, cancellationToken)
                .ConfigureAwait(false);

            // Filter for languages, if any languages are preferred:
            if(_options.FilterLanguages.Any())
            {
                repositories = repositories
                    .Where(x => _options.FilterLanguages.Contains(x.Language))
                    .ToList();
            }

            // We could introduce some parallelism, when processing the repositories. Cloning 
            // a repository is probably blocking while sending or cloning. And so, we could
            // for example clone 4 repositories in parallel and process them.
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = _options.MaxParallelClones,
                CancellationToken = cancellationToken
            };

            // Now we throw off the Parallel Cloning for the repositories.
            await Parallel
                .ForEachAsync(source: repositories, parallelOptions: parallelOptions, body: (source, cancellationToken) => IndexRepositoryAsync(source, cancellationToken))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Indexes a GitHub Repository.
        /// </summary>
        /// <param name="owner">Name of the owner, which is a user or an organization</param>
        /// <param name="repository">Name of the Repository</param>
        /// <param name="cancellationToken">CancellationToken to cancel asynchronous processing</param>
        /// <returns>Awaitable Task</returns>
        public async Task IndexRepositoryAsync(string owner, string repository, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var repositoryMetadata = await _gitHubClient
                    .GetRepositoryByOwnerAndRepositoryAsync(owner, repository, cancellationToken)
                    .ConfigureAwait(false);

                if (repositoryMetadata == null)
                {
                    throw new Exception($"Unable to read repository metadata for Owner '{owner}' and Repository '{repository}'");
                }

                await IndexRepositoryAsync(repositoryMetadata, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to index repository '{Repository}'", $"{owner}/{repository}");
            }
        }

        /// <summary>
        /// Indexes a GitHub Repository.
        /// </summary>
        /// <param name="repositoryMetadata">Metadata of the Repository</param>
        /// <param name="cancellationToken">CancellationToken to cancel asynchronous processing</param>
        /// <returns>An awaitable task</returns>
        public async ValueTask IndexRepositoryAsync(RepositoryMetadataDto repositoryMetadata, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Nothing to do, if we cannot clone ...
            if (repositoryMetadata.CloneUrl == null)
            {
                _logger.LogInformation("Not cloning repository '{Repository}, because it has no Clone URL'", repositoryMetadata.FullName);

                return;
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
                            _logger.LogDebug("Deleting Repository '{RepositoryDirectory}' for a fresh Clone", repositoryDirectory)
                        }

                        DeleteReadOnlyDirectory(repositoryDirectory);
                    }
                }

                await _elasticCodeSearchClient.DeleteByOwnerRepositoryAndBranchAsync(
                    owner: repositoryMetadata.Owner.Login,
                    repository: repositoryMetadata.Name,
                    branch: repositoryMetadata.DefaultBranch, cancellationToken);

                // Clone into the given Directory
                _gitExecutor.Clone(repositoryMetadata.CloneUrl, repositoryDirectory);

                // Get the list of allowed files, by matching against allowed extensions (.c, .cpp, ...)
                // and allowed filenames (.gitignore, README, ...). We don't want to parse binary data.
                var batches = _gitExecutor.ListFiles(repositoryDirectory)
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
                _logger.LogError(e, "Indexing Repository '{Repository}' failed", repositoryMetadata.FullName);

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
                            DeleteReadOnlyDirectory(repositoryDirectory);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error deleting '{Repository}'", repositoryMetadata.FullName);
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
        public static void DeleteReadOnlyDirectory(string directory)
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
        /// Indexes files, given by their relative path.
        /// </summary>
        /// <param name="repositoryMetadata">Repository Metadata</param>
        /// <param name="files">List of Files</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>An awaitable <see cref="ValueTask"/></returns>
        public async ValueTask IndexDocumentsAsync(RepositoryMetadataDto repositoryMetadata, string[] files, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            List<CodeSearchDocument> documents = new List<CodeSearchDocument>();

            foreach (var file in files)
            {
                var codeSearchDocument = GetCodeSearchDocument(repositoryMetadata, file);

                if (codeSearchDocument != null)
                {
                    documents.Add(codeSearchDocument);
                }
            }

            await _elasticCodeSearchClient.BulkIndexAsync(documents, cancellationToken);
        }

        /// <summary>
        /// Gets the <see cref="CodeSearchDocumentDto"/> to be indexed.
        /// </summary>
        /// <param name="repository">Physical Repository</param>
        /// <param name="repositoryMetadata">Repository Metadata</param>
        /// <param name="relativeFilename">Filename to process</param>
        /// <returns>An awaitable Task with the <see cref="CodeSearchDocument"/></returns>
        private CodeSearchDocument GetCodeSearchDocument(RepositoryMetadataDto repositoryMetadata, string relativeFilename)
        {
            _logger.TraceMethodEntry();

            var repositoryDirectory = GetRepositoryDirectory(repositoryMetadata);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Start indexing {Repository}: {Filename}", repositoryDirectory, relativeFilename);
            }

            // Get the Commit Information to index, such as the File SHA1 Hash, the Commit Hash, ...
            (var shaHash, var commitHash, var latestCommitDate) = _gitExecutor.GetCommitInformation(repositoryDirectory, relativeFilename);

            // Get the absolute Filename, so we can read it
            var absoluteFilename = Path.Combine(repositoryDirectory, relativeFilename);

            var content = File.ReadAllText(absoluteFilename);

            // Build the final CodeSearchDocument with all relevant information for indexing.
            var codeSearchDocument = new CodeSearchDocument
            {
                Id = shaHash,
                Path = relativeFilename,
                Repository = repositoryMetadata.Name,
                Owner = repositoryMetadata.Owner.Login,
                Content = content,
                Branch = repositoryMetadata.DefaultBranch,
                Filename = Path.GetFileName(relativeFilename),
                CommitHash = commitHash,
                Permalink = $"https://github.com/{repositoryMetadata.Owner.Login}/{repositoryMetadata.Name}/blob/{commitHash}/{relativeFilename}",
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
        private string GetRepositoryDirectory(RepositoryMetadataDto repository)
        {
            _logger.TraceMethodEntry();

            return Path.Combine(_options.BaseDirectory, repository.Name);
        }
    }
}
