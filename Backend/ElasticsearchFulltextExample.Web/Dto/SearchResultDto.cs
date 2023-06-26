// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
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
        public required IReadOnlyCollection<string>? Matches { get; set; }

        [JsonPropertyName("latestCommitDate")]
        public required DateTime LatestCommitDate { get; set; }
    }
}
