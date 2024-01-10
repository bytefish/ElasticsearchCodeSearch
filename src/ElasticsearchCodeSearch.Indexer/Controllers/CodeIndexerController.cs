// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.AspNetCore.Mvc;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Indexer.Hosted;
using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Converters;

namespace ElasticsearchCodeSearch.Indexer.Controllers
{
    [ApiController]
    public class CodeIndexController : ControllerBase
    {
        private readonly ILogger<CodeIndexController> _logger;
        private readonly ElasticCodeSearchClient _elasticsearchClient;

        public CodeIndexController(ILogger<CodeIndexController> logger, ElasticCodeSearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _logger = logger;
        }

        [HttpPost]
        [Route("/delete-index")]
        public async Task<IActionResult> DeleteIndex(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var response = await _elasticsearchClient.DeleteIndexAsync(cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (_logger.IsErrorEnabled())
                    {
                        response.TryGetOriginalException(out var originalException);

                        _logger.LogError(originalException, "Elasticsearch failed with an unhandeled Exception");
                    }

                    return BadRequest("Invalid Search Response from Elasticsearch");
                }

                return Ok();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "Failed to delete index");
                }

                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("/delete-all-documents")]
        public async Task<IActionResult> DeleteAllDocuments(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var deleteAllResponse = await _elasticsearchClient.DeleteAllAsync(cancellationToken);

                if (!deleteAllResponse.IsValidResponse)
                {
                    if (_logger.IsErrorEnabled())
                    {
                        deleteAllResponse.TryGetOriginalException(out var originalException);

                        _logger.LogError(originalException, "Elasticsearch failed with an unhandeled Exception");
                    }

                    return BadRequest("Invalid Search Response from Elasticsearch");
                }

                return Ok();
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
        [Route("/create-index")]
        public async Task<IActionResult> CreateIndex(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var result = await _elasticsearchClient.CreateIndexAsync(cancellationToken);

                if(!result.Success)
                {
                    if (_logger.IsErrorEnabled())
                    {
                        if (result.ErrorResponse != null)
                        {
                            result.ErrorResponse.TryGetOriginalException(out var originalException);

                            _logger.LogError(originalException, "Elasticsearch failed with an unhandeled Exception");
                        }
                    }

                    return BadRequest("Invalid Search Response from Elasticsearch");
                }

                return Ok();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "Failed to create index");
                }

                return StatusCode(500);
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
                    if (_logger.IsErrorEnabled())
                    {
                        bulkIndexResponse.TryGetOriginalException(out var originalException);

                        _logger.LogError(originalException, "Indexing failed due to an invalid response from Elasticsearch");
                    }

                    return BadRequest($"ElasticSearch indexing failed with Errors");
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

        [HttpPost]
        [Route("/index-repository")]
        public IActionResult IndexRepository([FromServices] IndexerJobQueues jobQueue, [FromBody] IndexRepositoryRequestDto indexRepositoryRequest)
        {
            _logger.TraceMethodEntry();

            try
            {
                jobQueue.GitHubRepositories.Enqueue($"{indexRepositoryRequest.Owner}/{indexRepositoryRequest.Repository}");

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

        [HttpPost]
        [Route("/index-organization")]
        public IActionResult IndexOrganization([FromServices] IndexerJobQueues jobQueue, [FromBody] IndexOrganizationRequestDto indexOrganizationRequest)
        {
            _logger.TraceMethodEntry();

            try
            {
                jobQueue.GitHubOrganizations.Enqueue(indexOrganizationRequest.Organization);

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