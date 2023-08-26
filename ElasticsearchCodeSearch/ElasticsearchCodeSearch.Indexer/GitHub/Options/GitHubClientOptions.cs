namespace ElasticsearchCodeSearch.Indexer.Client.Options
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
