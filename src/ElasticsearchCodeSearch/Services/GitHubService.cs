// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.Git;
using ElasticsearchCodeSearch.Indexer.GitHub;
using ElasticsearchCodeSearch.Indexer.GitHub.Dto;
using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Logging;

namespace ElasticsearchCodeSearch.Services
{
    /// <summary>
    /// Reads the <see cref="GitRepositoryMetadata"> from GitHub.
    /// </summary>
    public class GitHubService
    {
        private readonly ILogger<GitIndexerService> _logger;

        private readonly GitHubClient _gitHubClient;

        public GitHubService(ILogger<GitIndexerService> logger, GitExecutor gitExecutor, GitHubClient gitHubClient, ElasticCodeSearchClient elasticCodeSearchClient)
        {
            _logger = logger;
            _gitHubClient = gitHubClient;
        }

        /// <summary>
        /// Gets all Repositories for a given Organization.
        /// </summary>
        /// <param name="organization">Organization Name, for example "dotnet"</param>
        /// <param name="cancellationToken">Cancellation Token to cancel asynchronous operations</param>
        /// <returns>List of Git Repositories</returns>
        public async Task<List<GitRepositoryMetadata>> GetAllRepositoriesByOrganizationAsync(string organization, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Gets all Repositories of the Organization:
            var repositories = await _gitHubClient
                .GetAllRepositoriesByOrganizationAsync(organization, 20, cancellationToken)
                .ConfigureAwait(false);

            return repositories
                .Select(repository => Convert(repository))
                .ToList();
        }

        /// <summary>
        /// Gets the <see cref="GitRepositoryMetadata"/> from a given GitHub Repository.
        /// </summary>
        /// <param name="owner">Owner of the Repository, which is either an Organization or User</param>
        /// <param name="repository">Repository Name</param>
        /// <param name="cancellationToken">Cancellation Token to cancel asynchronous operations</param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown, when the Metadata couldn't be read</exception>
        public async Task<GitRepositoryMetadata> GetRepositoryByOwnerAndNameAsync(string owner, string repository, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var repositoryMetadata = await _gitHubClient
                .GetRepositoryByOwnerAndRepositoryAsync(owner, repository, cancellationToken)
                .ConfigureAwait(false);

            if (repositoryMetadata == null)
            {
                throw new Exception($"Unable to read repository metadata for Owner '{owner}' and Repository '{repository}'");
            }

            var result = Convert(repositoryMetadata);

            return result;
        }

        private GitRepositoryMetadata Convert(RepositoryMetadataDto source)
        {
            _logger.TraceMethodEntry();

            var result = new GitRepositoryMetadata
            {
                Owner = source.Owner.Login,
                Name = source.Name,
                Branch = source.DefaultBranch,
                CloneUrl = source.CloneUrl!,
                Language = source.Language!
            };

            return result;
        }
    }
}
