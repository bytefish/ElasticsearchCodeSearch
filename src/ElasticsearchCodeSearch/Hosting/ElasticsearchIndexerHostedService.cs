// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Services;
using ElasticsearchCodeSearch.Shared.Logging;

namespace ElasticsearchCodeSearch.Hosting
{
    /// <summary>
    /// A very simple Background Service to Process Indexing Requests in the Background. It basically 
    /// contains two concurrent queues to queue the repositories or organization to be indexed. This 
    /// should be replaced by a proper framework, such as Quartz.NET probably?
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
        private readonly IndexerJobQueue _jobQueue;

        /// <summary>
        /// Creates a new Elasticsearch Indexer Background Service.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="gitIndexerService">GitHub Indexer Service</param>
        public ElasticsearchIndexerHostedService(ILogger<ElasticsearchIndexerHostedService> logger, IndexerJobQueue jobQueue, GitIndexerService gitIndexerService)
        {
            _logger = logger;
            _gitIndexerService = gitIndexerService;
            _jobQueue = jobQueue;
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

            while (_jobQueue.GitRepositories.TryDequeue(out var repository))
            {
                try
                {
                    await _gitIndexerService.IndexRepositoryAsync(repository, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to index Organization '{Repository}'", repository.FullName);
                }
            }
        }
    }
}