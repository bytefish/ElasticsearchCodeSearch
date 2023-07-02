// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Dto
{
    public class SearchResultDto
    {
        [JsonPropertyName("Id")]
        public required string Id { get; set; }

        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        [JsonPropertyName("repository")]
        public required string Repository { get; set; }

        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        [JsonPropertyName("matches")]
        public required IReadOnlyCollection<string> Matches { get; set; } = new List<string>();

        [JsonPropertyName("latestCommitDate")]
        public required DateTime LatestCommitDate { get; set; }
    }
}
