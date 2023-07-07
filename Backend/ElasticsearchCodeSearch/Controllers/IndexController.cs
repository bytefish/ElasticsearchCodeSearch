// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Converters;
using ElasticsearchCodeSearch.Elasticsearch;
using ElasticsearchCodeSearch.Logging;
using ElasticsearchCodeSearch.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchCodeSearch.Controllers
{
    public class IndexController : Controller
    {
        private readonly ILogger<IndexController> _logger;
        private readonly ElasticCodeSearchClient _client;

        public IndexController(ILogger<IndexController> logger, ElasticCodeSearchClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpPost]
        [Route("/api/index")]
        public async Task<IActionResult> IndexDocument([FromBody] List<CodeSearchDocumentDto> codeSearchDocuments, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var documents = CodeSearchDocumentConverter.Convert(codeSearchDocuments);

                var bulkIndexResponse = await _client.BulkIndexAsync(documents, cancellationToken);

                if (!bulkIndexResponse.IsSuccess())
                {
                    return BadRequest($"ElasticSearch Indexing failed with Errors");
                }

                return Ok();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "Failed to index documents");
                }

                return StatusCode(500);
            }
        }


    }
}