// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Converters;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.AspNetCore.Mvc;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Controllers
{
    [ApiController]
    public class CodeSearchController : ControllerBase
    {
        private readonly ILogger<CodeSearchController> _logger;
        private readonly ElasticCodeSearchClient _elasticsearchClient;

        public CodeSearchController(ILogger<CodeSearchController> logger, ElasticCodeSearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _logger = logger;
        }

        [HttpGet]
        [Route("/search-statistics")]
        public async Task<IActionResult> GetSearchStatistics(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var indicesStatResponse = await _elasticsearchClient.GetSearchStatistics(cancellationToken);

                if (!indicesStatResponse.IsValidResponse)
                {
                    if (_logger.IsErrorEnabled())
                    {
                        indicesStatResponse.TryGetOriginalException(out var originalException);

                        _logger.LogError(originalException, "Elasticsearch failed with an unhandeled Exception");
                    }

                    return BadRequest("Invalid Search Response from Elasticsearch");
                }

                var result = CodeSearchStatisticsConverter.Convert(indicesStatResponse);

                return Ok(result);
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "An unhandeled exception occured");
                }

                return StatusCode(500, "An internal Server Error occured");
            }
        }

        [HttpPost]
        [Route("/search-documents")]
        public async Task<IActionResult> SearchDocuments([FromBody] CodeSearchRequestDto request, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var searchRequest = CodeSearchRequestConverter.Convert(request);

                var searchResponse = await _elasticsearchClient.SearchAsync(searchRequest, cancellationToken);

                if (!searchResponse.IsValidResponse)
                {
                    if (_logger.IsErrorEnabled())
                    {
                        searchResponse.TryGetOriginalException(out var originalException);

                        _logger.LogError(originalException, "Elasticsearch failed with an unhandeled Exception");
                    }

                    return BadRequest("Invalid Search Response from Elasticsearch");
                }

                var codeSearchResults = new CodeSearchResultsDto
                {
                    Query = request.Query,
                    From = request.From,
                    Size = request.Size,
                    Sort = request.Sort,
                    Total = searchResponse.Total,
                    TookInMilliseconds = searchResponse.Took,
                    Results = CodeSearchResultConverter.Convert(searchResponse)
                };

                return Ok(codeSearchResults);
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "An unhandeled exception occured");
                }

                return StatusCode(500, "An internal Server Error occured");
            }
        }
    }
}