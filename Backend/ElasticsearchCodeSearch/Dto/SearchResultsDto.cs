﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

        [JsonPropertyName("results")]
        public required IReadOnlyCollection<SearchResultDto> Results { get; set; }
    }
}
