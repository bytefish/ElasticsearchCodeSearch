// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Logging;

namespace ElasticsearchCodeSearch.Hosting
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
            
            try
            {
                await _elasticsearchClient.CreateIndexAsync(cancellationToken);
            } 
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to create Search Index");
            }            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return Task.CompletedTask;
        }
    }
}