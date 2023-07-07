// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Converters;
using ElasticsearchCodeSearch.Dto;
using ElasticsearchCodeSearch.Elasticsearch;
using ElasticsearchCodeSearch.Logging;
using ElasticsearchCodeSearch.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchCodeSearch.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;
        private readonly ElasticCodeSearchClient _elasticsearchClient;

        public SearchController(ILogger<SearchController> logger, ElasticCodeSearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/search")]
        public async Task<IActionResult> Query([FromBody] CodeSearchRequestDto request, CancellationToken cancellationToken)
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
    }
}