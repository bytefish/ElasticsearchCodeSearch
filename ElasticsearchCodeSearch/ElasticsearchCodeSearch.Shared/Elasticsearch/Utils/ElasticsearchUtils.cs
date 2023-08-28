// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Models;
using System.Text.RegularExpressions;

namespace ElasticsearchCodeSearch.Shared.Elasticsearch.Utils
{
    /// <summary>
    /// Auxillary methods to simplify working with Elasticsearch.
    /// </summary>
    public static class ElasticsearchUtils
    {
        /// <summary>
        /// Matches all content between a start and an end tag.
        /// </summary>
        private static readonly Regex regex = new Regex($"{ElasticsearchConstants.HighlightStartTag}(.*){ElasticsearchConstants.HighlightEndTag}");

        /// <summary>
        /// Returns the Highlighted Content, with the line number, line content and the 
        /// information wether to highlight a line or not.
        /// </summary>
        /// <param name="content">Matching Content from the Elasticsearch response</param>
        /// <returns>List of highlighted content</returns>
        public static List<HighlightedContent> GetHighlightedContent(string content)
        {
            // We want to highlight entire lines of code and don't want to only
            // highlight the match. So we need to get the number of matched lines
            // to highlight first:
            int matchedLinesCount = GetMatchedLinesCount(content);

            // We now want to process each line separately. We don't need to scale
            // massively, so we read the entire file content into memory. This won't 
            // be too much hopefully ...
            var lines = content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);

            // We need to know the total number of lines, so we know the maximum number of lines, 
            // that we can take and highlight:
            int totalLines = lines.Length;

            // Now we need the start index, we begin the highlightning at:
            bool highlightFound = TryGetHighlightStartIndex(lines, out var startIdx);

            // Holds the Search Results:
            var result = new List<HighlightedContent>();

            // If no highlight was found, we return an empty list, because there is 
            // nothing to highlight in the results anyway. Probably the filename
            // matched?
            if (!highlightFound)
            {
                return result;
            }

            // If there are at least 2 preceeding lines, we will
            // use the two preceeding lines in the snippet.
            int from = startIdx >= 2 ? startIdx - 2 : startIdx;

            // If there are more than 2 lines left, we will use 
            // these trailing two lines in the snippet.
            int to = totalLines - startIdx > 3 ? startIdx + 2 : startIdx;

            // Build the result.
            for (int lineIdx = from; lineIdx <= to; lineIdx++)
            {
                // The raw line with the possible match tags.
                var line = lines[lineIdx];

                // Remove the Start and End Tags from the content.
                var sanitizedLine = line
                    .Replace(ElasticsearchConstants.HighlightStartTag, string.Empty)
                    .Replace(ElasticsearchConstants.HighlightEndTag, string.Empty);

                // Check if this line has been a match. We could probably simplify the code
                // but I don't know.
                bool isHighlight = lineIdx >= startIdx && lineIdx < startIdx + matchedLinesCount;

                result.Add(new HighlightedContent
                {
                    LineNo = lineIdx + 1,
                    IsHighlight = isHighlight,
                    Content = sanitizedLine
                });
            }

            return result;
        }

        private static int GetMatchedLinesCount(string content)
        {
            var match = regex.Match(content);

            if (match.Groups.Count == 0)
            {
                // Just return 5 lines by default...
                return 0;
            }

            string matchedContent = match.Groups[1].Value;

            int matchedLinesCount = matchedContent
                .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Length;

            return matchedLinesCount;
        }

        private static bool TryGetHighlightStartIndex(string[] lines, out int startIdx)
        {
            startIdx = 0;

            for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                var line = lines[lineIdx];

                if (line.Contains(ElasticsearchConstants.HighlightStartTag))
                {
                    startIdx = lineIdx;
                    return true;
                }
            }

            return false;
        }
    }
}
