using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Sort Order.
    /// </summary>
    public enum SortOrderEnumDto
    {
        /// <summary>
        /// Ascending.
        /// </summary>
        [JsonPropertyName("asc")]
        Ascending = 1,

        /// <summary>
        /// Descending.
        /// </summary>
        [JsonPropertyName("desc")]
        Descending = 2
    }
}
