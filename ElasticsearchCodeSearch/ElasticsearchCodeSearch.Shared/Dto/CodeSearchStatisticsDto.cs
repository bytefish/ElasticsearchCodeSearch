// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Summarizes useful metrics about the Elasticsearch instance.
    /// 
    /// There is a great guide for Elasticsearch monitoring and statistics by the DataDog team:
    /// 
    ///     https://www.datadoghq.com/blog/monitor-elasticsearch-performance-metrics/#search-performance-metrics
    ///     
    /// </summary>
    public class CodeSearchStatisticsDto
    {
        /// <summary>
        /// Index Name.
        /// </summary>
        [JsonPropertyName("indexName")]
        public required string IndexName { get; set; }

        /// <summary>
        /// Total Index Size in bytes (indices.store.size_in_bytes).
        /// </summary>
        [JsonPropertyName("indexSizeInBytes")]
        public required long? IndexSizeInBytes { get; set; }

        /// <summary>
        /// Total number of documents indexed (indices.docs.count).
        /// </summary>
        [JsonPropertyName("totalNumberOfDocumentsIndexed")]
        public required long? TotalNumberOfDocumentsIndexed { get; set; }

        /// <summary>
        /// Number of documents currently being indexed (indices.indexing.index_current).
        /// </summary>
        [JsonPropertyName("numberOfDocumentsCurrentlyBeingIndexed")]
        public required long? NumberOfDocumentsCurrentlyBeingIndexed { get; set; }

        /// <summary>
        /// Total time spent indexing documents (indices.bulk.total_time_in_millis).
        /// </summary>
        [JsonPropertyName("totalTimeSpentBulkIndexingDocumentsInMilliseconds")]
        public required long? TotalTimeSpentBulkIndexingDocumentsInMilliseconds { get; set; }

        /// <summary>
        /// Total time spent indexing documents (indices.indexing.index_time_in_millis).
        /// </summary>
        [JsonPropertyName("totalTimeSpentIndexingDocumentsInMilliseconds")]
        public required long? TotalTimeSpentIndexingDocumentsInMilliseconds { get; set; }

        /// <summary>
        /// Total number of queries (indices.search.query_total).
        /// </summary>
        [JsonPropertyName("totalNumberOfQueries")]
        public required long? TotalNumberOfQueries { get; set; }

        /// <summary>
        /// Total time spent on queries (indices.search.query_time_in_millis).
        /// </summary>
        [JsonPropertyName("totalTimeSpentOnQueriesInMilliseconds")]
        public required long? TotalTimeSpentOnQueriesInMilliseconds { get; set; }

        /// <summary>
        /// Number of queries currently in progress (indices.search.query_current).
        /// </summary>
        [JsonPropertyName("numberOfQueriesCurrentlyInProgress")]
        public required long? NumberOfQueriesCurrentlyInProgress { get; set; }

        /// <summary>
        /// Total number of fetches (indices.search.fetch_total).
        /// </summary>
        [JsonPropertyName("totalNumberOfFetches")]
        public required long? TotalNumberOfFetches { get; set; }

        /// <summary>
        /// Total time spent on fetches (indices.search.fetch_time_in_millis).
        /// </summary>
        [JsonPropertyName("totalTimeSpentOnFetchesInMilliseconds")]
        public required long? TotalTimeSpentOnFetchesInMilliseconds { get; set; }

        /// <summary>
        /// Number of fetches currently in progress (indices.search.fetch_current).
        /// </summary>
        [JsonPropertyName("numberOfFetchesCurrentlyInProgress")]
        public required long? NumberOfFetchesCurrentlyInProgress { get; set; }
    }
}
