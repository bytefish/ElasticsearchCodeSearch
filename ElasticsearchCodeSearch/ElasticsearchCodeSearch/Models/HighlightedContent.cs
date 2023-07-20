// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// Holds the line number and line content for a match, and it 
    /// has the information if the content needs highlighting.
    /// </summary>
    public class HighlightedContent
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
        /// Gets or sets the flag, if this line needs to be highlighted.
        /// </summary>
        public bool IsHighlight { get; set; }
    }
}
