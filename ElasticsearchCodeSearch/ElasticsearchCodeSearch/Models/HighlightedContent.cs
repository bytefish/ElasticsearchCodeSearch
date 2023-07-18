// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// Highlighted Line of Code. We want to keep the UI simple, so we 
    /// prepare the data for easy display.
    /// </summary>
    public class HighlightedContent
    {
        public int LineNo { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsHighlight { get; set; }
    }
}
