// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Logging;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    /// <summary>
    /// Used to create the Elasticsearch index at Startup.
    /// </summary>
    public class ElasticsearchInitializerHostedService : IHostedService
    {
        private readonly ElasticCodeSearchClient _elasticsearchClient;
        private readonly ILogger<ElasticsearchInitializerHostedService> _logger;

        public ElasticsearchInitializerHostedService(ILogger<ElasticsearchInitializerHostedService> logger, ElasticCodeSearchClient elasticsearchClient)
        {
            _logger = logger;
            _elasticsearchClient = elasticsearchClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var healthTimeout = TimeSpan.FromSeconds(60);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Waiting for at least 1 Node and at least 1 Active Shard, with a Timeout of {HealthTimeout} seconds.", healthTimeout.TotalSeconds);
            }

            var clusterHealthResponse = await _elasticsearchClient.WaitForClusterAsync(healthTimeout, cancellationToken);

            if(!clusterHealthResponse.IsValidResponse)
            {
                _logger.LogError("Invalid Request to get Cluster Health: {DebugInformation}", clusterHealthResponse.DebugInformation);
            }

            var indexExistsResponse = await _elasticsearchClient.IndexExistsAsync(cancellationToken);

            if (!indexExistsResponse.Exists)
            {
                await _elasticsearchClient.CreateIndexAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return Task.CompletedTask;
        }
    }
}
