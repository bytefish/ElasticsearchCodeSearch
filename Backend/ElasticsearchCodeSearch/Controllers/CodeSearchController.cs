// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Converters;
using ElasticsearchCodeSearch.Elasticsearch;
using ElasticsearchCodeSearch.Logging;
using ElasticsearchCodeSearch.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchCodeSearch.Controllers
{
    public class CodeSearchController : Controller
    {
        private readonly ILogger<CodeSearchController> _logger;
        private readonly ElasticCodeSearchClient _elasticsearchClient;

        public CodeSearchController(ILogger<CodeSearchController> logger, ElasticCodeSearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/search")]
        public async Task<IActionResult> QueryDocuments([FromBody] CodeSearchRequestDto request, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var searchRequest = CodeSearchRequestConverter.Convert(request);

            var searchResponse = await _elasticsearchClient.SearchAsync(searchRequest, cancellationToken);

            var codeSearchResults = new CodeSearchResultsDto
            {
                From = request.From,
                Size = request.Size,
                Query = request.Query,
                Sort = request.Sort,
                Results = CodeSearchResultConverter.Convert(searchResponse)
            };

            return Ok(codeSearchResults);
        }

        [HttpPost]
        [Route("/api/index")]
        public async Task<IActionResult> IndexDocuments([FromBody] List<CodeSearchDocumentDto> codeSearchDocuments, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var documents = CodeSearchDocumentConverter.Convert(codeSearchDocuments);

                var bulkIndexResponse = await _elasticsearchClient.BulkIndexAsync(documents, cancellationToken);

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