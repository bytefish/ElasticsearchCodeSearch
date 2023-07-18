// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
