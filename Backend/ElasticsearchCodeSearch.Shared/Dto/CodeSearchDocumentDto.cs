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
        /// A unique document id.
        /// </summary>
        [Required]
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Owner (User or Organization).
        /// </summary>
        [Required]
        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        /// <summary>
        /// Repository.
        /// </summary>
        [Required]
        [JsonPropertyName("repository")]
        public required string Repository { get; set; }

        /// <summary>
        /// The Filename of the uploaded document.
        [Required]
        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        /// <summary>
        /// The Filename of the uploaded document.
        [Required]
        [JsonPropertyName("path")]
        public required string Path { get; set; }

        /// The Filename of the uploaded document.
        [Required]
        [JsonPropertyName("commitHash")]
        public required string CommitHash { get; set; }

        /// <summary>
        /// Content to Index.
        /// </summary>
        [Required]
        [JsonPropertyName("content")]
        public required string Content { get; set; } = string.Empty;

        /// <summary>
        /// Permalink to the indexed file.
        /// </summary>
        [Required]
        [JsonPropertyName("permalink")]
        public required string Permalink { get; set; }

        /// <summary>
        /// Latest Commit Date.
        /// </summary>
        [Required]
        [JsonPropertyName("latestCommitDate")]
        public required DateTimeOffset LatestCommitDate { get; set; }
    }
}
