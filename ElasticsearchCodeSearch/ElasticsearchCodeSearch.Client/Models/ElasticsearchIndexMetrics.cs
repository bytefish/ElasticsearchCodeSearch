namespace ElasticsearchCodeSearch.Client.Models
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
