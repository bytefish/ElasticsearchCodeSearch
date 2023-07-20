// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// Sort Field.
    /// </summary>
    public class SortField
    {
        /// <summary>
        /// Gets or sets the Sort Field.
        /// </summary>
        public required string Field { get; set; }
        
        /// <summary>
        /// Gets or sets the Sort Order.
        /// </summary>
        public required SortOrderEnum Order { get; set; } = SortOrderEnum.Ascending;
    }
}
