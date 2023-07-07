using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// Sort Field.
    /// </summary>
    public class SortField
    {
        public required string Field { get; set; }

        public required SortOrderEnum Order { get; set; } = SortOrderEnum.Ascending;
    }
}
