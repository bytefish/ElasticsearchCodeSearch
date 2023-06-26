// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using ElasticsearchFulltextExample.Web.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
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

                if(!bulkIndexResponse.IsSuccess())
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
                Content = source.Content,
                LatestCommitDate = source.LatestCommitDate,
            };
        }
    }
}