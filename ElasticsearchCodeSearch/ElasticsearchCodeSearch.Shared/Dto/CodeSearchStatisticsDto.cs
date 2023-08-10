using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// 
    /// Aggregates Statistics from the Elasticsearch Statistics Endpoints.
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
        public required string IndexName { get; set; }

        /// <summary>
        /// Total Index Size in bytes (indices.total.store.size_in_bytes).
        /// </summary>
        public required long? IndexSizeInBytes { get; set; }

        /// <summary>
        /// Total number of documents indexed (indices.indexing.index_total).
        /// </summary>
        public required long? TotalNumberOfDocumentsIndexed { get; set; }

        /// <summary>
        /// Total time spent indexing documents (indices.indexing.index_time_in_millis).
        /// </summary>
        public required long? TotalTimeSpentIndexingDocumentsInMilliseconds { get; set; }

        /// <summary>
        /// Number of documents currently being indexed (indices.indexing.index_current).
        /// </summary>
        public required long? NumberOfDocumentsCurrentlyBeingIndexed { get; set; }
                
        /// <summary>
        /// Total number of queries (indices.search.query_total).
        /// </summary>
        public required long? TotalNumberOfQueries { get; set; }

        /// <summary>
        /// Total time spent on queries (indices.search.query_time_in_millis).
        /// </summary>
        public required long? TotalTimeSpentOnQueriesInMilliseconds { get; set; }

        /// <summary>
        /// Number of queries currently in progress (indices.search.query_current).
        /// </summary>
        public required long? NumberOfQueriesCurrentlyInProgress { get; set; }

        /// <summary>
        /// Total number of fetches (indices.search.fetch_total).
        /// </summary>
        public required long? TotalNumberOfFetches { get; set; }

        /// <summary>
        /// Total time spent on fetches (indices.search.fetch_time_in_millis).
        /// </summary>
        public required long? TotalTimeSpentOnFetchesInMilliseconds { get; set; }

        /// <summary>
        /// Number of fetches currently in progress (indices.search.fetch_current).
        /// </summary>
        public required long? NumberOfFetchesCurrentlyInProgress { get; set; }
    }
}
