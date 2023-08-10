namespace ElasticsearchCodeSearch.Client.Models
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
