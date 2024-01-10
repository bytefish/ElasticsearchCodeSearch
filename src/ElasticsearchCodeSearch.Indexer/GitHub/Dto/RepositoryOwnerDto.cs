// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Indexer.GitHub.Dto
{
    public class RepositoryOwnerDto
    {
        [JsonPropertyName("login")]
        public required string Login { get; set; }
    }
}
