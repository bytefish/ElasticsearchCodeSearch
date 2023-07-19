// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Highlighted Line of Code.
    /// </summary>
    public class HighlightedContentDto
    {
        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        public int LineNo { get; set; }

        /// <summary>
        /// Gets or sets the line content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the information, if the content needs to be highlighted.
        /// </summary>
        public bool IsHighlight { get; set; }
    }
}
