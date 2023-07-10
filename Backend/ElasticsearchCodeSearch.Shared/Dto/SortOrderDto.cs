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
        Asc = 1,

        /// <summary>
        /// Descending.
        /// </summary>
        Desc = 2
    }
}
