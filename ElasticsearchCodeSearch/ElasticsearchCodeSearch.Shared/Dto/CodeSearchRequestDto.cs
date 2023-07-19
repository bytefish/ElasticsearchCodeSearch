// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// The Client sends a <see cref="CodeSearchDocumentDto"/> to filter for 
    /// documents, paginate and sort the results. The Search Query is given as 
    /// a Query String.
    /// </summary>
    public class CodeSearchRequestDto
    {
        /// <summary>
        /// Gets or sets the Query String.
        /// </summary>
        [Required]
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        /// <summary>
        /// Gets or sets the number of hits to skip, defaulting to 0.
        /// </summary>
        [Required]
        [JsonPropertyName("from")]
        public int From { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of hits to return, defaulting to 10.
        /// </summary>
        [Required]
        [JsonPropertyName("size")]
        public int Size { get; set; } = 10;

        /// <summary>
        /// Gets or sets the sort fields for the results.
        /// </summary>
        [Required]
        [JsonPropertyName("sort")]
        public List<SortFieldDto> Sort { get; set; } = new List<SortFieldDto>();
    }
}
