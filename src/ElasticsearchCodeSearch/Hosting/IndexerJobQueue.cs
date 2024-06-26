using ElasticsearchCodeSearch.Models;
using System.Collections.Concurrent;

namespace ElasticsearchCodeSearch.Hosting
{
    /// <summary>
    /// Simple In-Memory Job Queues to be processed by the Indexer.
    /// </summary>
    public class IndexerJobQueue
    {
        /// <summary>
        /// GitHub Organizations to index Git repositories from.
        /// </summary>
        public readonly ConcurrentQueue<GitRepositoryMetadata> GitRepositories = new ConcurrentQueue<GitRepositoryMetadata>();
    }
}
