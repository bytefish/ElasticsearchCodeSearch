// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Holds the Paginated Code Search Results.
    /// </summary>
    public class CodeSearchResultsDto
    {
        /// <summary>
        /// Gets or sets the query string, that has been used.
        /// </summary>
        [Required]
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        /// <summary>
        /// Gets or sets the number of hits to skip, defaulting to 0.
        /// </summary>
        [Required]
        [JsonPropertyName("from")]
        public required int From { get; set; }

        /// <summary>
        /// Gets or sets the number of hits to return, defaulting to 10.
        /// </summary>
        [Required]
        [JsonPropertyName("size")]
        public required int Size { get; set; }

        /// <summary>
        /// Gets or sets the total Number of matched documents.
        /// </summary>
        [Required]
        [JsonPropertyName("total")]
        public required int Total { get; set; }

        /// <summary>
        /// Gets or sets the sort fields used.
        /// </summary>
        [Required]
        [JsonPropertyName("sort")]
        public required List<SortFieldDto> Sort { get; set; } = new List<SortFieldDto>();

        /// <summary>
        /// Gets or sets the search results.
        /// </summary>
        [Required]
        [JsonPropertyName("results")]
        public required List<CodeSearchResultDto> Results { get; set; }
    }
}
