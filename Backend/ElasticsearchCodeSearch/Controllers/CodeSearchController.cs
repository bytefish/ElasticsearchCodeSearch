﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Converters;
using ElasticsearchCodeSearch.Elasticsearch;
using ElasticsearchCodeSearch.Logging;
using ElasticsearchCodeSearch.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        [Route("/search-documents")]
        public async Task<IActionResult> SearchDocuments([FromBody] CodeSearchRequestDto request, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                // Convert to Elasticsearch DTO.
                var searchRequest = CodeSearchRequestConverter.Convert(request);

                // Query the Elasticsearch API.
                var searchResponse = await _elasticsearchClient.SearchAsync(searchRequest, cancellationToken);

                // If it's not a valid response from Elasticsearch, then exit and return a BadRequest.
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
                    Total = (int) searchResponse.Total,
                    Results = CodeSearchResultConverter.Convert(searchResponse)
                };

                return Ok(codeSearchResults);
            } 
            catch(Exception e)
            {
                if(_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "An unhandeled exception occured");
                }

                return StatusCode(500, "An internal Server Error occured");
            }
        }

        [HttpPost]
        [Route("/index-documents")]
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