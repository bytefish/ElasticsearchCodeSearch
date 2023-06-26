// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
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
        /// The Data of the Document.
        /// </summary>
        [Required]
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        /// <summary>
        /// Latest Commit Date.
        /// </summary>
        [Required]
        [JsonPropertyName("latestCommitDate")]
        public required DateTime LatestCommitDate { get; set; }
    }
}
