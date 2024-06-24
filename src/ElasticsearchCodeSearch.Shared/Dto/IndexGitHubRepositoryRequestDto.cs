// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// A Request for indexing a repository.
    /// </summary>
    public class IndexGitHubRepositoryRequestDto
    {
        /// <summary>
        /// Gets or sets the repository url to index.
        /// </summary>
        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        [JsonPropertyName("repository")]
        public required string Repository { get; set; }
    }
}
