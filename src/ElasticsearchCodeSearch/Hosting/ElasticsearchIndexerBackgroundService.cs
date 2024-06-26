// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Infrastructure;
using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Services;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Options;

namespace ElasticsearchCodeSearch.Hosting
{
    /// <summary>
    /// A very simple Background Service to Process Indexing Requests in the Background. It basically 
    /// contains two concurrent queues to queue the repositories or organization to be indexed. This 
    /// should be replaced by a proper framework, such as Quartz.NET probably?
    /// </summary>
    public class ElasticsearchIndexerBackgroundService : BackgroundService
    {
        private readonly ILogger<ElasticsearchIndexerBackgroundService> _logger;

        /// <summary>
        /// Indexer for GitHub Repositories.
        /// </summary>
        private readonly GitIndexerService _gitIndexerService;

        /// <summary>
        /// Options for Git Repository indexing.
        /// </summary>
        private readonly GitIndexerOptions _gitIndexerOptions;

        /// <summary>
        /// Indexer Job Queues to process.
        /// </summary>
        private readonly GitRepositoryJobQueue _jobQueue;

        /// <summary>
        /// Creates a new Elasticsearch Indexer Background Service.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="gitIndexerService">GitHub Indexer Service</param>
        public ElasticsearchIndexerBackgroundService(ILogger<ElasticsearchIndexerBackgroundService> logger, GitRepositoryJobQueue jobQueue, GitIndexerService gitIndexerService, IOptions<GitIndexerOptions> gitIndexerOptions)
        {
            _logger = logger;
            _gitIndexerService = gitIndexerService;
            _jobQueue = jobQueue;
            _gitIndexerOptions = gitIndexerOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                await _gitIndexerService.CreateSearchIndexAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Search Index");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                ParallelOptions parallelOptions = new()
                {
                    MaxDegreeOfParallelism = _gitIndexerOptions.MaxParallelClones,
                    CancellationToken = cancellationToken,
                };

                await Parallel.ForEachAsync(
                    source: _jobQueue.ToAsyncEnumerable(cancellationToken), 
                    parallelOptions: parallelOptions, 
                    body: (x, ct) => ProcessRepositoryAsync(x, ct));
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return Task.CompletedTask;
        }

        private async ValueTask ProcessRepositoryAsync(GitRepositoryMetadata repository, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

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