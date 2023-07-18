// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Highlighted Line of Code.
    /// </summary>
    public class HighlightedContentDto
    {
        /// <summary>
        /// Line Number in the data.
        /// </summary>
        public int LineNo { get; set; }

        /// <summary>
        /// Content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Has this been a match?
        /// </summary>
        public bool IsHighlight { get; set; }
    }
}
