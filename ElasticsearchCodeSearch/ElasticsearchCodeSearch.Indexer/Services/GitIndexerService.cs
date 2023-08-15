using ElasticsearchCodeSearch.Indexer.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElasticsearchCodeSearch.Indexer.Services
{
    /// <summary>
    /// Git Indexer.
    /// </summary>
    public class GitIndexerService
    {
        private readonly ILogger<GitIndexerService> _logger;
        private readonly GitHubClient _gitHubClient;

        public GitIndexerService(ILogger<GitIndexerService> logger, GitHubClient gitHubClient)
        {
            _logger = logger;
            
            _gitHubClient = gitHubClient;
        }

    }
}
