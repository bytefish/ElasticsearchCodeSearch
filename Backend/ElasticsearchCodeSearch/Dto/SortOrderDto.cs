using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Dto
{
    /// <summary>
    /// Sort Order.
    /// </summary>
    public enum SortOrderDto
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
