using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Sort Field.
    /// </summary>
    public class SortFieldDto
    {
        [Required]
        [JsonPropertyName("field")]
        public required string Field { get; set; }

        [Required]
        [JsonPropertyName("order")]
        public required SortOrderEnumDto Order { get; set; } = SortOrderEnumDto.Ascending;
    }
}
