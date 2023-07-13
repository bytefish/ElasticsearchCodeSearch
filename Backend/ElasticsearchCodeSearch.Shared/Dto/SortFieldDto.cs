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
        /// SortBy Field.
        /// </summary>
        [Required]
        [JsonPropertyName("field")]
        public required string Field { get; set; }

        /// <summary>
        /// Sort Order.
        /// </summary>
        [Required]
        [JsonPropertyName("order")]
        public SortOrderEnumDto Order { get; set; } = SortOrderEnumDto.Asc;
    }
}
