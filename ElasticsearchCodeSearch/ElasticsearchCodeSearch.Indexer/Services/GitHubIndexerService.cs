// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.Git;
using ElasticsearchCodeSearch.Indexer.GitHub;
using ElasticsearchCodeSearch.Indexer.GitHub.Dto;
using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Options;

namespace ElasticsearchCodeSearch.Indexer.Services
{
    /// <summary>
    /// Git Indexer.
    /// </summary>
    public class GitHubIndexerService
    {
        private readonly ILogger<GitHubIndexerService> _logger;

        private readonly GitIndexerOptions _options;

        private readonly GitExecutor _gitExecutor;
        private readonly GitHubClient _gitHubClient;
        private readonly ElasticCodeSearchClient _elasticCodeSearchClient;

        public GitHubIndexerService(ILogger<GitHubIndexerService> logger, IOptions<GitIndexerOptions> options, GitExecutor gitExecutor, GitHubClient gitHubClient, ElasticCodeSearchClient elasticCodeSearchClient)
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
            var response = await _gitHubClient
                .GetRepositoriesByOrganizationAsync(organization, 1, 20, cancellationToken)
                .ConfigureAwait(false);

            // Get the Repositories from the response.
            var repositories = response.Values!;

            // We could introduce some parallelism, when processing the repositories. Cloning 
            // a repository is probably blocking and so, we could for example clone 4
            // repositories in parallel and process them.
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
            catch(Exception e)
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
            if(repositoryMetadata.CloneUrl == null)
            {
                _logger.LogInformation("Not cloning repository '{Repository}, because it has no Clone URL'", repositoryMetadata.FullName);

                return;
            }

            var allowedFilenames = _options.AllowedFilenames.ToHashSet();
            var allowedExtensions = _options.AllowedExtensions.ToHashSet();

            var workingDirectory = GetWorkingDirectory(repositoryMetadata);

            try
            {
                if (workingDirectory.StartsWith(@"C:\Temp"))
                {
                    if (Directory.Exists(workingDirectory))
                    {
                        DeleteReadOnlyDirectory(workingDirectory);
                    }
                }

                await _elasticCodeSearchClient.DeleteByOwnerRepositoryAndBranchAsync(
                    owner: repositoryMetadata.Owner.Login, 
                    repository: repositoryMetadata.Name, 
                    branch: repositoryMetadata.DefaultBranch, cancellationToken);

                await _gitExecutor
                    .Clone(repositoryMetadata.CloneUrl, workingDirectory, cancellationToken)
                    .ConfigureAwait(false);

                // Get the list of allowed files, by matching against allowed extensions (.c, .cpp, ...)
                // and allowed filenames (.gitignore, README, ...). We don't want to parse binary data.
                var batches =  (await _gitExecutor.ListFiles(workingDirectory, cancellationToken).ConfigureAwait(false))
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
            catch(Exception e)
            {
                _logger.LogError(e, "Indexing Repository '{Repository}' failed", repositoryMetadata.FullName);

                throw;
            } 
            finally
            {
                if(workingDirectory.StartsWith(@"C:\Temp")) 
                {
                    try
                    {
                        if (Directory.Exists(workingDirectory))
                        {
                            DeleteReadOnlyDirectory(workingDirectory);
                        }
                    } 
                    catch(Exception e) 
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
                var codeSearchDocument = await GetCodeSearchDocumentAsync(repositoryMetadata, file, cancellationToken);

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
        private async Task<CodeSearchDocument> GetCodeSearchDocumentAsync(RepositoryMetadataDto repositoryMetadata, string relativeFilename, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var workingDirectory = GetWorkingDirectory(repositoryMetadata);

            var shaHash = await _gitExecutor
                .SHA1(workingDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            var commitHash = await _gitExecutor
                .CommitHash(workingDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            var latestCommitDate = await _gitExecutor
                .LatestCommitDate(workingDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            var absoluteFilename = Path.Combine(workingDirectory, relativeFilename);

            var content = File.ReadAllText(absoluteFilename);

            return new CodeSearchDocument
            {
                Id = shaHash,
                Path = relativeFilename,
                Repository = repositoryMetadata.Name,
                Owner = repositoryMetadata.Owner.Login,
                Content = content,
                Branch = repositoryMetadata.DefaultBranch,
                Filename = Path.GetFileName(relativeFilename),
                CommitHash = commitHash,
                Permalink = $"https://github.com/{repositoryMetadata.Owner}/{repositoryMetadata.Name}/blob/{commitHash}/{relativeFilename}",
                LatestCommitDate = latestCommitDate
            };
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
        /// Returns the current Working Directory.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        private string GetWorkingDirectory(RepositoryMetadataDto repository)
        {
            _logger.TraceMethodEntry();

            return Path.Combine(_options.BaseDirectory, repository.Name);
        }
    }
}
