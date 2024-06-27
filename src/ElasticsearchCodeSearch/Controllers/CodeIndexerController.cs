// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.AspNetCore.Mvc;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Shared.Dto;
using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Converters;
using ElasticsearchCodeSearch.Services;
using ElasticsearchCodeSearch.Infrastructure;

namespace ElasticsearchCodeSearch.Controllers
{
    [ApiController]
    public class CodeIndexController : ControllerBase
    {
        private readonly ILogger<CodeIndexController> _logger;
        private readonly GitHubService _gitHubService;
        private readonly ElasticCodeSearchClient _elasticsearchClient;

        public CodeIndexController(ILogger<CodeIndexController> logger, GitHubService gitHubService, ElasticCodeSearchClient elasticsearchClient)
        {
            _logger = logger;
            _gitHubService = gitHubService;
            _elasticsearchClient = elasticsearchClient;
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

                if (!result.IsValidResponse)
                {
                    if (_logger.IsErrorEnabled())
                    {
                        result.TryGetOriginalException(out var originalException);

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
        [Route("/index-git-repository")]
        public IActionResult IndexGitRepository([FromServices] GitRepositoryJobQueue jobQueue, [FromBody] IndexGitHubRepositoryRequestDto indexRepositoryRequest)
        {
            _logger.TraceMethodEntry();

            try
            {
                return BadRequest("Not implemented yet");
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
        [Route("/index-github-repository")]
        public async Task<IActionResult> IndexGitHubRepository([FromServices] GitRepositoryJobQueue jobQueue, [FromBody] IndexGitHubRepositoryRequestDto indexRepositoryRequest, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var repository = await _gitHubService
                    .GetRepositoryByOwnerAndNameAsync(indexRepositoryRequest.Owner, indexRepositoryRequest.Repository, cancellationToken)
                    .ConfigureAwait(false);

                if(_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("GitHub Repository '{GitHubRepository}' enqueued", repository.FullName);
                }

                jobQueue.Post(repository);

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
        [Route("/index-github-organization")]
        public async Task<IActionResult> IndexGitHubOrganization([FromServices] GitRepositoryJobQueue jobQueue, [FromBody] IndexGitHubOrganizationRequestDto indexOrganizationRequest, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var repositories = await _gitHubService
                    .GetAllRepositoriesByOrganizationAsync(indexOrganizationRequest.Organization, cancellationToken)
                    .ConfigureAwait(false);

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("'{NumberOfRepositories}' GitHub Repositories for Organization '{Organization}' enqueued", 
                        repositories.Count,
                        indexOrganizationRequest.Organization);
                }

                foreach(var repository in repositories)
                {
                    jobQueue.Post(repository);
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