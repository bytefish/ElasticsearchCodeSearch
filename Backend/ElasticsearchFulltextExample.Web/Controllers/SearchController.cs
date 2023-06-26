// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using ElasticsearchFulltextExample.Web.Logging;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchFulltextExample.Web.Controllers
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

        [HttpGet]
        [Route("/api/search")]
        public async Task<IActionResult> Query([FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var searchResponse = await _elasticsearchClient.SearchAsync(query, cancellationToken);
            var searchResult = ConvertToSearchResults(query, searchResponse);

            return Ok(searchResult);
        }

        private SearchResultsDto ConvertToSearchResults(string query, SearchResponse<CodeSearchDocument> searchResponse)
        {
            _logger.TraceMethodEntry();

            List<SearchResultDto> results = new List<SearchResultDto>();

            foreach(var hit in searchResponse.Hits)
            {
                if(hit.Source == null)
                {
                    continue;
                }

                var result = new SearchResultDto
                {
                    Id = hit.Source.Id,
                    Owner = hit.Source.Owner,
                    Repository = hit.Source.Repository,
                    Filename = hit.Source.Filename,
                    LatestCommitDate = hit.Source.LatestCommitDate,
                    Matches = GetMatches(hit.Highlight),
                };

                results.Add(result);
            }

            return new SearchResultsDto
            {
                Query = query,
                Results = results
            };
        }


        private IReadOnlyCollection<string>? GetMatches(IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlight)
        {
            _logger.TraceMethodEntry();

            if(highlight == null)
            {
                return null;
            }

            List<string> results = new List<string>();

            if (highlight.TryGetValue(Infer.Field<CodeSearchDocument>(x => x.Content).Name, out var matchesForContent))
            {
                results.AddRange(matchesForContent);
            }

            if (highlight.TryGetValue(Infer.Field<CodeSearchDocument>(x => x.Filename).Name, out var matchesForFilename))
            {
                results.AddRange(matchesForFilename);
            }

            return results;
        }

    }
}