// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// A Request for indexing a repository.
    /// </summary>
    public class IndexGitRepositoryRequestDto
    {
        /// <summary>
        /// Gets or sets the repository url to index.
        /// </summary>
        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
