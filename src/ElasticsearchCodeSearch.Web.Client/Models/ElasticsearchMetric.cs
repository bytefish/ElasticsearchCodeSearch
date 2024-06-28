// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Web.Client.Models
{
    public class ElasticsearchMetric
    {
        /// <summary>
        /// Name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Elasticsearch Key.
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public required string? Value { get; set; }
    }
}
