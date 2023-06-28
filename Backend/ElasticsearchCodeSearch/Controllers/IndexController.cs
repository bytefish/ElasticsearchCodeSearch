// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Dto;
using ElasticsearchCodeSearch.Elasticsearch;
using ElasticsearchCodeSearch.Elasticsearch.Model;
using ElasticsearchCodeSearch.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
        public async Task<IActionResult> IndexDocument([FromBody] CodeSearchDocumentDto[] codeSearchDocuments, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var documents = ConvertFromDto(codeSearchDocuments);

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

        private CodeSearchDocument[] ConvertFromDto(CodeSearchDocumentDto[] source)
        {
            _logger.TraceMethodEntry();

            return source
                .Select(x => ConvertFromDto(x))
                .ToArray();
        }

        private CodeSearchDocument ConvertFromDto(CodeSearchDocumentDto source)
        {
            _logger.TraceMethodEntry();

            return new CodeSearchDocument
            {
                Id = source.Id,
                Owner = source.Owner,
                Repository = source.Repository,
                Filename = source.Filename,
                Content = GetContentFromBase64(source.Content),
                LatestCommitDate = source.LatestCommitDate,
            };
        }

        private string? GetContentFromBase64(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            var contentBytes = Convert.FromBase64String(source);

            return Encoding.UTF8.GetString(contentBytes);
        }
    }
}