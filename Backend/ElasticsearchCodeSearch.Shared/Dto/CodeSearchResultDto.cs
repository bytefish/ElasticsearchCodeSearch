// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    public class CodeSearchResultDto
    {
        [JsonPropertyName("Id")]
        public required string Id { get; set; }

        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        [JsonPropertyName("repository")]
        public required string Repository { get; set; }

        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        [JsonPropertyName("permalink")]
        public required string Permalink { get; set; }

        [JsonPropertyName("matches")]
        public required List<string>? Matches { get; set; }

        [JsonPropertyName("latestCommitDate")]
        public required DateTimeOffset LatestCommitDate { get; set; }
    }
}
