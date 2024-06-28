// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Indexer.GitHub.Exceptions
{
    public class GitHubApiException : Exception
    {
        public GitHubApiException()
        {
        }

        public GitHubApiException(string? message) : base(message)
        {
        }

        public GitHubApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}