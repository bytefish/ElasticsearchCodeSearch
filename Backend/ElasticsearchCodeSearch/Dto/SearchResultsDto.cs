// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Dto
{
    public class SearchResultsDto
    {
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        [JsonPropertyName("from")]
        public required int From { get; set; }

        [JsonPropertyName("size")]
        public required int Size { get; set; }

        [JsonPropertyName("sort")]
        public required List<SortFieldDto> Sort { get; set; } = new List<SortFieldDto>();

        [JsonPropertyName("results")]
        public required List<CodeSearchResultDto> Results { get; set; }
    }
}
