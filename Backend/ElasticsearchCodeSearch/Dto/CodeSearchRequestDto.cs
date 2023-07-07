// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Dto
{
    /// <summary>
    /// Search Request to the API.
    /// </summary>
    public class CodeSearchRequestDto
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
        public required int From { get; set; } = 0;

        /// <summary>
        /// Maximum number of hits to return, defaulting to 10.
        /// </summary>
        [Required]
        [JsonPropertyName("size")]
        public required int Size { get; set; } = 10;

        /// <summary>
        /// Sort Fields.
        /// </summary>
        [Required]
        [JsonPropertyName("sort")]
        public required List<SortFieldDto> Sort { get; set; } = new List<SortFieldDto>();

    }
}
