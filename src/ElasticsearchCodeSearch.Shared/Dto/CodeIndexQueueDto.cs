// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    public class CodeIndexQueueDto
    {
        /// <summary>
        /// Gets or sets the current repositories queued for indexing.
        /// </summary>
        [JsonPropertyName("repositories")]
        public required List<GitRepositoryMetadataDto> Repositories { get; set; } = [];
    }
}
