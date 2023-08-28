// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace ElasticsearchCodeSearch.Indexer.GitHub.Exceptions
{
    [Serializable]
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

        protected GitHubApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}