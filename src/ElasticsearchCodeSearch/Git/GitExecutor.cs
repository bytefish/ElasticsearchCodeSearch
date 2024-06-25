// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Logging;
using LibGit2Sharp;

namespace ElasticsearchCodeSearch.Indexer.Git
{
    /// <summary>
    /// Exposes various GIT commands useful for indexing files.
    /// </summary>
    public class GitExecutor
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<GitExecutor> _logger;

        /// <summary>
        /// Creates a new GitExecutor.
        /// </summary>
        /// <param name="logger">Logger</param>
        public GitExecutor(ILogger<GitExecutor> logger) 
        {
            _logger = logger;
        }

        /// <summary>
        /// Clones a Repository.
        /// </summary>
        /// <param name="repositoryUrl"></param>
        /// <param name="repositoryDirectory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public string Clone(string repositoryUrl, string repositoryDirectory)
        {
            _logger.TraceMethodEntry();

            if(_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Cloning Repository '{RepositoryUrl}' to Directory '{RepositoryDirectory}'", repositoryUrl, repositoryDirectory);
            }

            var repositoryPath = Repository.Clone(repositoryUrl, repositoryDirectory);

            if(_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Cloned Repository Url '{RepositoryUrl}' to Path '{RepositoryPath}'", repositoryUrl, repositoryPath);
            }

            return repositoryPath;
        }

        /// <summary>
        /// Gets all relevant information for the given file.
        /// </summary>
        /// <param name="repositoryDirectory">Git Directory to check</param>
        /// <param name="relativeFilePath">File to check</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public (string fileSha1Hash, string commitSha1Hash, DateTimeOffset latestCommitDate) GetCommitInformation(string repositoryDirectory, string relativeFilePath)
        {
            _logger.TraceMethodEntry();

            using (var repository = new Repository(repositoryDirectory))
            {
                IndexEntry entryWithPath = repository.Index[relativeFilePath];

                if (entryWithPath == null)
                {
                    throw new InvalidOperationException($"Cannot get SHA1 Hash from file {relativeFilePath}");
                }

                var fileSha1Hash = entryWithPath.Id.Sha;

                // Get the Commits to the File
                var commits = repository.Commits
                    // Commits for this file
                    .QueryBy(relativeFilePath, new CommitFilter
                    {
                        SortBy = CommitSortStrategies.Time
                    })
                    // Get the latest one:
                    .Take(1);

                var latestCommit = commits.FirstOrDefault();

                if (latestCommit == null)
                {
                    throw new InvalidOperationException("Cannot get latest Commit Hash");
                }

                var latestCommitSha1Hash = latestCommit.Commit.Sha;
                var latestCommitDate = latestCommit.Commit.Committer.When;

                return (fileSha1Hash, latestCommitSha1Hash, latestCommitDate);
            }
        }

        /// <summary>
        /// Lists all files in a given Git Repository, which is the git command:
        ///     
        ///     ls-files 
        ///     
        /// </summary>
        /// <param name="repositoryDirectory">Repository</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Files for the repository</returns>
        public string[] ListFiles(string repositoryDirectory)
        {
            _logger.TraceMethodEntry();

            using (var repository = new Repository(repositoryDirectory))
            {
                var repositoryStatus = repository.RetrieveStatus(new StatusOptions { IncludeUnaltered = true });

                var unalteredFiles = repositoryStatus.Unaltered
                    .Select(x => x.FilePath)
                    .ToArray();

                return unalteredFiles;
            }
        }
    }
}