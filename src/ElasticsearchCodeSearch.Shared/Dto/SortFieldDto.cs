// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Sort Field.
    /// </summary>
    public class SortFieldDto
    {
        /// <summary>
        /// Gets or sets the field name to sort.
        /// </summary>
        [Required]
        [JsonPropertyName("field")]
        public required string Field { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        [Required]
        [JsonPropertyName("order")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SortOrderEnumDto Order { get; set; } = SortOrderEnumDto.Asc;
    }
}