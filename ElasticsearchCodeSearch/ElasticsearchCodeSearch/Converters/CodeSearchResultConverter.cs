// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Elasticsearch;
using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Converters
{
    public static class CodeSearchResultConverter
    {
        public static List<CodeSearchResultDto> Convert(SearchResponse<CodeSearchDocument> source)
        {
            List<CodeSearchResultDto> results = new List<CodeSearchResultDto>();
            
            foreach (var hit in source.Hits)
            {
                if (hit.Source == null)
                {
                    continue;
                }

                var result = new CodeSearchResultDto
                {
                    Id = hit.Source.Id,
                    Owner = hit.Source.Owner,
                    Repository = hit.Source.Repository,
                    Filename = hit.Source.Filename,
                    Path = hit.Source.Path,
                    Permalink = hit.Source.Permalink,
                    LatestCommitDate = hit.Source.LatestCommitDate,
                    Content = GetContent(hit.Highlight),
                };

                results.Add(result);
            }

            return results;
        }

        private static List<HighlightedContentDto> GetContent(IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlight)
        {
            if (highlight == null)
            {
                return new();
            }

            highlight.TryGetValue("content", out var matchesForContent);

            if(matchesForContent == null)
            {
                return new();
            }

            var match = matchesForContent.FirstOrDefault();

            if (match == null)
            {
                return new();
            }

            var highlightedContent = ElasticsearchUtils.GetHighlightedContent(match);

            return HighlightedContentConverter.Convert(highlightedContent);
        }
    }
}