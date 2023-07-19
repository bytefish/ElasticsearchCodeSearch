// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Holds the Search Results along with the highlighted matches.
    /// </summary>
    public class CodeSearchResultDto
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonPropertyName("Id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        /// <summary>
        /// Gets or sets the repository name.
        /// </summary>
        [JsonPropertyName("repository")]
        public required string Repository { get; set; }

        /// <summary>
        /// Gets or sets the relative file path.
        /// </summary>
        [JsonPropertyName("path")]
        public required string Path { get; set; }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        /// <summary>
        /// Gets or sets the Permalink.
        /// </summary>
        [JsonPropertyName("permalink")]
        public required string Permalink { get; set; }

        /// <summary>
        /// Gets or sets the Highlighted Content, which is the lines.
        /// </summary>
        [JsonPropertyName("content")]
        public required List<HighlightedContentDto> Content { get; set; }

        /// <summary>
        /// Gets or sets the latest commit date.
        /// </summary>
        [JsonPropertyName("latestCommitDate")]
        public required DateTimeOffset LatestCommitDate { get; set; }
    }
}
