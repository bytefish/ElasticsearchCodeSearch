// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// A code document, which should be indexed and searchable by Elasticsearch. 
    /// </summary>
    public class CodeSearchDocument
    {
        /// <summary>
        /// A unique document id.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Owner (User or Organization).
        /// </summary>
        public required string Owner { get; set; }

        /// <summary>
        /// Repository.
        /// </summary>
        public required string Repository { get; set; }

        /// <summary>
        /// The Path of the uploaded document.
        public required string Path { get; set; }

        /// <summary>
        /// The Filename of the uploaded document.
        public required string Filename { get; set; }

        /// <summary>
        /// Content to Index.
        /// </summary>
        public required string Content { get; set; } =string.Empty;

        /// <summary>
        /// Permalink to the indexed file.
        /// </summary>
        public string Permalink { get; set; } = string.Empty;

        /// <summary>
        /// Latest Commit Date.
        /// </summary>
        public required DateTimeOffset LatestCommitDate { get; set; }
    }
}
