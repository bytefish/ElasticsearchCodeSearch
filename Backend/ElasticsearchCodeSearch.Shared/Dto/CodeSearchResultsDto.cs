// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    public class CodeSearchResultsDto
    {
        /// <summary>
        /// Defines the query string to search.
        /// </summary>
        [Required]
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        /// <summary>
        /// Defines the number of hits to skip, defaulting to 0.
        /// </summary>
        [Required]
        [JsonPropertyName("from")]
        public required int From { get; set; }

        /// <summary>
        /// Maximum number of hits to return, defaulting to 10.
        /// </summary>
        [Required]
        [JsonPropertyName("size")]
        public required int Size { get; set; }

        /// <summary>
        /// Total Number of Matches.
        /// </summary>
        [Required]
        [JsonPropertyName("total")]
        public required int Total { get; set; }

        /// <summary>
        /// Sort Fields.
        /// </summary>
        [Required]
        [JsonPropertyName("sort")]
        public required List<SortFieldDto> Sort { get; set; } = new List<SortFieldDto>();

        /// <summary>
        /// Code Search Results.
        /// </summary>
        [Required]
        [JsonPropertyName("results")]
        public required List<CodeSearchResultDto> Results { get; set; }
    }
}
