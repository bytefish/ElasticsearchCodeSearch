using ElasticsearchCodeSearch.Models;
using System.Threading.Channels;

namespace ElasticsearchCodeSearch.Infrastructure
{
    /// <summary>
    /// Simple In-Memory Job Queues to be processed by the Indexer.
    /// </summary>
    public class GitRepositoryJobQueue
    {
        public readonly Channel<GitRepositoryMetadata> Channel = System.Threading.Channels.Channel.CreateUnbounded<GitRepositoryMetadata>();

        public bool Post(GitRepositoryMetadata repository)
        {
            return Channel.Writer.TryWrite(repository);
        }

        public IAsyncEnumerable<GitRepositoryMetadata> ToAsyncEnumerable(CancellationToken cancellationToken)
        {
            return Channel.Reader.ReadAllAsync(cancellationToken);

        }
    }
}
