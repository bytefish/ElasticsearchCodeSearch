using ElasticsearchCodeSearch.Shared.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElasticsearchCodeSearch.Shared.Elasticsearch
{
    public static class ElasticsearchUtils
    {
        private static Regex regex = new Regex($"{ElasticsearchConstants.HighlightStartTag}(.*){ElasticsearchConstants.HighlightEndTag}");

        public static int? GetHighlightLineNo(string content)
        {
            var matchedLinesCount = GetMatchedLinesCount(content);


            var lines = content
                // Split into Lines:
                .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select((line, idx) => new
                {
                    Index = idx,
                    Line = line,
                    IsHighlightStart = line.Contains(ElasticsearchConstants.HighlightStartTag),
                    IsHighlightEnd = line.Contains(ElasticsearchConstants.HighlightEndTag)
                });
        }

        private static int GetMatchedLinesCount(string content)
        {
            var match = regex.Match(content);

            if(match.Groups.Count == 0)
            {
                return 0;
            }

            // Get the Matched Content:
            string matchedContent = match.Groups[1].Value;

            // Split and Count lines, do not count empty lines:
            int matchedLinesCount = matchedContent
                .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Count();

            return matchedLinesCount;
        }
    }
}
