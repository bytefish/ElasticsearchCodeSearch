using ElasticsearchCodeSearch.Indexer.Client;
using ElasticsearchCodeSearch.Indexer.Client.Dto;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Logging;
using ElasticsearchCodeSearch.Shared.Services;
using LibGit2Sharp;
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
        private readonly ElasticsearchCodeSearchService _elasticCodeSearchService;
        private readonly GitHubClient _gitHubClient;

        public GitIndexerService(ILogger<GitIndexerService> logger, IOptions<GitIndexerOptions> options, GitHubClient gitHubClient, ElasticsearchCodeSearchService elasticCodeSearchService)
        {
            _logger = logger;
            _options = options.Value;
            _elasticCodeSearchService = elasticCodeSearchService;
            _gitHubClient = gitHubClient;
        }


        public async Task IndexOrganizationAsync(string organization, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var page = await _gitHubClient.GetRepositoriesByOrganizationAsync(organization, 1, 1, cancellationToken);
            var repositories = page.Values!;

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = _options.MaxParallelClones,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(source: repositories, parallelOptions: parallelOptions, body: (source, cancellationToken) => IndexRepositoryAsync(source, cancellationToken));
        }

        public async ValueTask IndexRepositoryAsync(RepositoryMetadataDto repositoryMetadata, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var allowedFilenames = _options.AllowedFilenames.ToHashSet();
            var allowedExtensions = _options.AllowedExtensions.ToHashSet();

            var workingDirectory = GetWorkingDirectory(repositoryMetadata);

            try
            {
                if (!Directory.Exists(workingDirectory))
                {
                    Repository.Clone(repositoryMetadata.CloneUrl, workingDirectory);
                }

                // Get the list of allowed files, by matching against allowed extensions (.c, .cpp, ...)
                // and allowed filenames (.gitignore, README, ...). We don't want to parse binary data.
                var batches = GetListOfFiles(repositoryMetadata)
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
                _logger.LogError(e, $"Indexing Repository '{repositoryMetadata.FullName}' failed");
            } 
            finally
            {
                if(workingDirectory.StartsWith(@"C:\Temp")) 
                {
                    if (Directory.Exists(workingDirectory))
                    {
                        Directory.Delete(workingDirectory, true);
                    }
                }
            }
        }

        public async ValueTask IndexDocumentsAsync(RepositoryMetadataDto repositoryMetadata, string[] files, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var workingDirectory = GetWorkingDirectory(repositoryMetadata);

            List<CodeSearchDocumentDto> documents = new List<CodeSearchDocumentDto>();

            using (var repository = new Repository(workingDirectory))
            {
                foreach (var file in files)
                {
                    var codeSearchDocument = GetCodeSearchDocumentDto(repository, repositoryMetadata, file);

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
        private CodeSearchDocumentDto GetCodeSearchDocumentDto(Repository repository, RepositoryMetadataDto repositoryMetadata, string relativeFilename)
        {
            _logger.TraceMethodEntry();

            // Get the History of the File, so we can extract all relevant GIT data
            var fileHistoryEntries = repository.Commits
                .QueryBy(relativeFilename, new CommitFilter() { SortBy = CommitSortStrategies.Time })
                .ToList();

            var logEntry = fileHistoryEntries.First();

            var relativePath = logEntry.Path;
            var shaHash = logEntry.Commit.Sha;
            var commitHash = logEntry.Commit.Id.Sha;
            var absoluteFilename = Path.Combine(GetWorkingDirectory(repositoryMetadata), relativeFilename);

            return new CodeSearchDocumentDto
            {
                Id = shaHash,
                Path = relativePath,
                Repository = repositoryMetadata.Name,
                Owner = repositoryMetadata.Owner.Login,
                Content = File.ReadAllText(absoluteFilename),
                Filename = Path.GetFileName(relativeFilename),
                CommitHash = logEntry.Commit.Id.Sha,
                Permalink = $"https://github.com/{repositoryMetadata.Owner}/{repositoryMetadata.Name}/blob/{commitHash}/{relativePath}",
                LatestCommitDate = logEntry.Commit.Committer.When.ToUniversalTime(),
            };
        }

        /// <summary>
        /// Returns the List of files for the given Repository.
        /// </summary>
        /// <param name="repositoryMetadataDto">Repository Metadata</param>
        /// <returns>List of all files in the Git repository</returns>
        private List<string> GetListOfFiles(RepositoryMetadataDto repositoryMetadataDto)
        {
            _logger.TraceMethodEntry();

            List<string> relativeFilenames = new List<string>();

            var workingDirectory = GetWorkingDirectory(repositoryMetadataDto);

            using (var repository = new Repository(workingDirectory))
            {
                foreach(var indexEntry in repository.Index)
                {     
                    relativeFilenames.Add(indexEntry.Path);
                }
            }

            return relativeFilenames;
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
