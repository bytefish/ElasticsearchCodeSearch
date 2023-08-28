// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Indexer.GitHub.Options
{
    /// <summary>
    /// GitHub Client Options.
    /// </summary>
    public class GitHubClientOptions
    {
        /// <summary>
        /// The Fine-Grained Access Token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Time to delay multiple requests.
        /// </summary>
        public int RequestDelayInMilliseconds { get; set; }
    }
}
