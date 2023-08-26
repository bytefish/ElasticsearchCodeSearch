// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.Client;
using ElasticsearchCodeSearch.Indexer.Client.Dto;
using ElasticsearchCodeSearch.Indexer.Git;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Logging;
using ElasticsearchCodeSearch.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElasticsearchCodeSearch.Indexer.Services
{
    /// <summary>
    /// Git Indexer.
    /// </summary>
    public class GitIndexerService
    {
        private readonly ILogger<GitIndexerService> _logger;

        private readonly GitIndexerOptions _options;

        private readonly GitClient _git;
        private readonly GitHubClient _gitHubClient;
        private readonly ElasticsearchCodeSearchService _elasticCodeSearchService;

        public GitIndexerService(ILogger<GitIndexerService> logger, IOptions<GitIndexerOptions> options, GitClient git, GitHubClient gitHubClient, ElasticsearchCodeSearchService elasticCodeSearchService)
        {
            _logger = logger;
            _options = options.Value;
            _git = git;
            _gitHubClient = gitHubClient;
            _elasticCodeSearchService = elasticCodeSearchService;
        }

        public async Task IndexOrganizationAsync(string organization, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _gitHubClient
                .GetRepositoriesByOrganizationAsync(organization, 1, 20, cancellationToken)
                .ConfigureAwait(false);

            var repositories = response.Values!;

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = _options.MaxParallelClones,
                CancellationToken = cancellationToken
            };

            await Parallel
                .ForEachAsync(source: repositories, parallelOptions: parallelOptions, body: (source, cancellationToken) => IndexRepositoryAsync(source, cancellationToken))
                .ConfigureAwait(false);
        }

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

                await _git
                    .Clone(repositoryMetadata.CloneUrl, workingDirectory, cancellationToken)
                    .ConfigureAwait(false);

                // Get the list of allowed files, by matching against allowed extensions (.c, .cpp, ...)
                // and allowed filenames (.gitignore, README, ...). We don't want to parse binary data.
                var batches =  (await _git.ListFiles(workingDirectory, cancellationToken).ConfigureAwait(false))
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
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
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

        public async ValueTask IndexDocumentsAsync(RepositoryMetadataDto repositoryMetadata, string[] files, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var workingDirectory = GetWorkingDirectory(repositoryMetadata);

            List<CodeSearchDocumentDto> documents = new List<CodeSearchDocumentDto>();

            foreach (var file in files)
            {
                var codeSearchDocument = await GetCodeSearchDocumentDtoAsync(repositoryMetadata, file, cancellationToken);

                if (codeSearchDocument != null)
                {
                    documents.Add(codeSearchDocument);
                }
            }

            await _elasticCodeSearchService.IndexDocumentsAsync(documents, cancellationToken);
        }

        /// <summary>
        /// Gets the <see cref="CodeSearchDocumentDto"/> to be indexed.
        /// </summary>
        /// <param name="repository">Physical Repository</param>
        /// <param name="repositoryMetadata">Repository Metadata</param>
        /// <param name="relativeFilename">Filename to process</param>
        /// <returns></returns>
        private async Task<CodeSearchDocumentDto> GetCodeSearchDocumentDtoAsync(RepositoryMetadataDto repositoryMetadata, string relativeFilename, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var workingDirectory = GetWorkingDirectory(repositoryMetadata);

            var shaHash = await _git
                .SHA1(workingDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            var commitHash = await _git
                .CommitHash(workingDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            var latestCommitDate = await _git
                .LatestCommitDate(workingDirectory, relativeFilename, cancellationToken)
                .ConfigureAwait(false);

            var absoluteFilename = Path.Combine(workingDirectory, relativeFilename);

            var content = File.ReadAllText(absoluteFilename);

            return new CodeSearchDocumentDto
            {
                Id = shaHash,
                Path = relativeFilename,
                Repository = repositoryMetadata.Name,
                Owner = repositoryMetadata.Owner.Login,
                Content = content,
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
