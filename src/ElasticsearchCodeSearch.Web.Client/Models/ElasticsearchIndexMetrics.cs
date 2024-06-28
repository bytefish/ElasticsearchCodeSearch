// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Web.Client.Models
{
    public class ElasticsearchIndexMetrics
    {
        /// <summary>
        /// Index.
        /// </summary>
        public required string Index { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public required List<ElasticsearchMetric> Metrics { get; set; }
    }
}
