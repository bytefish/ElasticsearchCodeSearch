// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Services;
using ElasticsearchCodeSearch.Shared.Logging;

namespace ElasticsearchCodeSearch.Hosting
{
    /// <summary>
    /// A very simple Background Service to Process Indexing Requests in the Background. It basically 
    /// contains two concurrent queues to queue the repositories or organization to be indexed. This 
    /// should be replaced by a proper framework, such as Quartz.NET.
    /// </summary>
    public class ElasticsearchIndexerHostedService : BackgroundService
    {
        private readonly ILogger<ElasticsearchIndexerHostedService> _logger;

        /// <summary>
        /// Indexer for GitHub Repositories.
        /// </summary>
        private readonly GitIndexerService _gitIndexerService;

        /// <summary>
        /// Indexer Job Queues to process.
        /// </summary>
        private readonly IndexerJobQueues _jobQueues;

        /// <summary>
        /// Creates a new Elasticsearch Indexer Background Service.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="gitIndexerService">GitHub Indexer Service</param>
        public ElasticsearchIndexerHostedService(ILogger<ElasticsearchIndexerHostedService> logger, IndexerJobQueues jobQueues, GitIndexerService gitIndexerService)
        {
            _logger = logger;
            _gitIndexerService = gitIndexerService;
            _jobQueues = jobQueues;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            while (!cancellationToken.IsCancellationRequested)
            {
                await ProcessQueuesAsync(cancellationToken);

                await Task.Delay(1000, cancellationToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return Task.CompletedTask;
        }

        private async Task ProcessQueuesAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            while (_jobQueues.GitHubOrganizations.TryDequeue(out var organization))
            {
                try
                {
                    await _gitIndexerService.IndexOrganizationAsync(organization, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to index Organization '{Organization}'", organization);
                }
            }

            while (_jobQueues.GitHubRepositories.TryDequeue(out var repositoryAndOwner))
            {
                if (TryGetOwnerAndRepository(repositoryAndOwner, out var owner, out var repository))
                {
                    try
                    {
                        await _gitIndexerService.IndexRepositoryAsync(owner, repository, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to index Repository '{RepositoryAndOwner}'", repositoryAndOwner);
                    }
                }
            }
        }

        private bool TryGetOwnerAndRepository(string repositoryAndOwner, out string owner, out string repository)
        {
            _logger.TraceMethodEntry();

            owner = repository = string.Empty;

            if (repositoryAndOwner == null)
            {
                return false;
            }

            var components = repositoryAndOwner.Split("/");

            if (components.Length != 2)
            {
                return false;
            }

            owner = components[0];
            repository = components[1];

            return true;
        }
    }
}