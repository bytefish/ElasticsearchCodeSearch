using Elastic.Clients.Elasticsearch;
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
                    Permalink = hit.Source.Permalink,
                    LatestCommitDate = hit.Source.LatestCommitDate,
                    Matches = GetMatches(hit.Highlight),
                };

                results.Add(result);
            }

            return results;
        }

        private static List<string>? GetMatches(IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlight)
        {
            if (highlight == null)
            {
                return null;
            }

            List<string> results = new List<string>();

            if (highlight.TryGetValue("content", out var matchesForContent)) // TODO Can we replace the "content"?
            {
                results.AddRange(matchesForContent);
            }

            return results;
        }
    }
}
