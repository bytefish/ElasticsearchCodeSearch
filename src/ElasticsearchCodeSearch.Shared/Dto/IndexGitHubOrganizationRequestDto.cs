// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Holds the Search Results along with the highlighted matches.
    /// </summary>
    public class IndexGitHubOrganizationRequestDto
    {
        /// <summary>
        /// Gets or sets the organization to index.
        /// </summary>
        [JsonPropertyName("organization")]
        public required string Organization { get; set; }
    }
}
