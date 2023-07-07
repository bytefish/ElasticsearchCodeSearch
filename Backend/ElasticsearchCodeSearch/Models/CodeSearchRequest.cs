// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// Search Request to the API.
    /// </summary>
    public class CodeSearchRequest
    {
        /// <summary>
        /// Defines the query string to search.
        /// </summary>
        public required string Query { get; set; }

        /// <summary>
        /// Defines the number of hits to skip, defaulting to 0.
        /// </summary>
        public required int From { get; set; } = 0;

        /// <summary>
        /// Maximum number of hits to return, defaulting to 10.
        /// </summary>
        public required int Size { get; set; } = 10;

        /// <summary>
        /// Sort Fields.
        /// </summary>
        public required List<SortField> Sort { get; set; } = new List<SortField>();

    }
}
