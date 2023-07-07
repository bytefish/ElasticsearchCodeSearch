using Elastic.Clients.Elasticsearch;
using ElasticsearchCodeSearch.Dto;
using ElasticsearchCodeSearch.Models;

namespace ElasticsearchCodeSearch.Converters
{
    public static class CodeSearchResultConverter
    {
        public static List<CodeSearchResult> Convert(SearchResponse<CodeSearchDocument> source)
        {
            List<CodeSearchResult> results = new List<CodeSearchResult>();

            foreach (var hit in source.Hits)
            {
                if (hit.Source == null)
                {
                    continue;
                }

                var result = new CodeSearchResult
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

            if (highlight.TryGetValue("filename", out var matchesForFilename)) // TODO Can we replace the "filename"?
            {
                results.AddRange(matchesForFilename);
            }

            return results;
        }
    }
}
