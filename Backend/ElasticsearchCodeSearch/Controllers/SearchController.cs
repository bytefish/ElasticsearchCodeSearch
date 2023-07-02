﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Dto;
using ElasticsearchCodeSearch.Elasticsearch;
using ElasticsearchCodeSearch.Elasticsearch.Model;
using ElasticsearchCodeSearch.Logging;
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
        public async Task<IActionResult> Query([FromBody] SearchRequestDto request, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var searchResponse = await _elasticsearchClient.SearchAsync(request.Query, request.From, request.Size, cancellationToken);
            var searchResult = ConvertToSearchResults(request, searchResponse);

            return Ok(searchResult);
        }

        private SearchResultsDto ConvertToSearchResults(SearchRequestDto request, SearchResponse<CodeSearchDocument> searchResponse)
        {
            _logger.TraceMethodEntry();

            List<SearchResultDto> results = new List<SearchResultDto>();

            foreach (var hit in searchResponse.Hits)
            {
                if (hit.Source == null)
                {
                    continue;
                }

                var result = new SearchResultDto
                {
                    Id = hit.Source.Id,
                    Owner = hit.Source.Owner,
                    Repository = hit.Source.Repository,
                    Filename = hit.Source.Filename,
                    Permalink = hit.Source.Permalink,
                    LatestCommitDate = hit.Source.LatestCommitDate,
                    Matches = GetMatches(hit.Highlight),
                };

                results.Add(result);
            }

            return new SearchResultsDto
            {
                Query = request.Query,
                From = request.From,
                Size = request.Size,
                Results = results
            };
        }

        private IReadOnlyCollection<string>? GetMatches(IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlight)
        {
            _logger.TraceMethodEntry();

            if (highlight == null)
            {
                return null;
            }

            List<string> results = new List<string>();

            if (highlight.TryGetValue("content", out var matchesForContent))
            {
                results.AddRange(matchesForContent);
            }

            if (highlight.TryGetValue("filename", out var matchesForFilename))
            {
                results.AddRange(matchesForFilename);
            }

            return results;
        }

    }
}