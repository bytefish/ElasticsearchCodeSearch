using System.Collections.Concurrent;

namespace ElasticsearchCodeSearch.Indexer.Hosted
{
    /// <summary>
    /// Holds the Job Queues.
    /// </summary>
    public class IndexerJobQueues
    {
        /// <summary>
        /// GitHub Organizations to index Git repositories from.
        /// </summary>
        public readonly ConcurrentQueue<string> GitHubOrganizations = new ConcurrentQueue<string>();

        /// <summary>
        /// GitHub Repositories to index.
        /// </summary>
        public readonly ConcurrentQueue<string> GitHubRepositories = new ConcurrentQueue<string>();
    }
}
