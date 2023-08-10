// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// A code document, which should be indexed and searchable by Elasticsearch. 
    /// </summary>
    public class CodeSearchDocumentDto
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Required]
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the owner (user or organization).
        /// </summary>
        [Required]
        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        /// <summary>
        /// Gets or sets the Repository Name.
        /// </summary>
        [Required]
        [JsonPropertyName("repository")]
        public required string Repository { get; set; }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        [Required]
        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        /// <summary>
        /// Gets or sets the relative file path.
        /// </summary>
        [Required]
        [JsonPropertyName("path")]
        public required string Path { get; set; }

        /// <summary>
        /// Gets or sets the commit hash.
        /// </summary>
        [Required]
        [JsonPropertyName("commitHash")]
        public required string CommitHash { get; set; }

        /// <summary>
        /// Gets or sets the content to index.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Permalink to the file.
        /// </summary>
        [Required]
        [JsonPropertyName("permalink")]
        public required string Permalink { get; set; }

        /// <summary>
        /// Gets or sets the latest commit date.
        /// </summary>
        [Required]
        [JsonPropertyName("latestCommitDate")]
        public required DateTimeOffset LatestCommitDate { get; set; }
    }
}