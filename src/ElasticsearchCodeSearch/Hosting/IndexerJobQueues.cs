using System.Collections.Concurrent;

namespace ElasticsearchCodeSearch.Hosting
{
    /// <summary>
    /// Simple In-Memory Job Queues to be processed by the Indexer.
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

        /// <summary>
        /// GitHub Repositories to index.
        /// </summary>
        public readonly ConcurrentQueue<string> GitRepositoryUrls = new ConcurrentQueue<string>();
    }
}
