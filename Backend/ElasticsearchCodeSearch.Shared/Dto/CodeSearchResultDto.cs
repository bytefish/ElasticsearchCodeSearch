// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Holds the Search Results with the highlighted matches.
    /// </summary>
    public class CodeSearchResultDto
    {
        /// <summary>
        /// Id of the document.
        /// </summary>
        [JsonPropertyName("Id")]
        public required string Id { get; set; }

        /// <summary>
        /// Owner.
        /// </summary>
        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        /// <summary>
        /// Repository.
        /// </summary>
        [JsonPropertyName("repository")]
        public required string Repository { get; set; }

        /// <summary>
        /// Path.
        /// </summary>
        [JsonPropertyName("path")]
        public required string Path { get; set; }

        /// <summary>
        /// Filename.
        /// </summary>
        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        /// <summary>
        /// Permalink.
        /// </summary>
        [JsonPropertyName("permalink")]
        public required string Permalink { get; set; }

        /// <summary>
        /// Highlighted matches in code.
        /// </summary>
        [JsonPropertyName("matches")]
        public required List<string>? Matches { get; set; }

        /// <summary>
        /// Latest Commit Date.
        /// </summary>
        [JsonPropertyName("latestCommitDate")]
        public required DateTimeOffset LatestCommitDate { get; set; }
    }
}
