// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
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

            // Now we can wait for the Shards to boot up
            var healthTimeout = TimeSpan.FromSeconds(60);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Waiting for at least 1 Node and at least 1 Active Shard, with a Timeout of {HealthTimeout} seconds.", healthTimeout.TotalSeconds);
            }

            await _elasticsearchClient.WaitForClusterAsync(healthTimeout, cancellationToken);

            var indexExistsResponse = await _elasticsearchClient.IndexExistsAsync(cancellationToken);

            if (!indexExistsResponse.Exists)
            {
                await _elasticsearchClient.CreateIndexAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
