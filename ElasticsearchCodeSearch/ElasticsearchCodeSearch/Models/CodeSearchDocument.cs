// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// A code document, which should be indexed and searchable by Elasticsearch. 
    /// </summary>
    public class CodeSearchDocument
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the owner (organization or user).
        /// </summary>
        public required string Owner { get; set; }

        /// <summary>
        /// Gets or sets the repository.
        /// </summary>
        public required string Repository { get; set; }

        /// <summary>
        /// Gets or sets the filepath.
        public required string Path { get; set; }

        /// <summary>
        /// Gets or sets the filename.
        public required string Filename { get; set; }

        /// <summary>
        /// Gets or sets the commit hash.
        public required string CommitHash { get; set; }

        /// <summary>
        /// Gets or sets the content to index.
        /// </summary>
        public required string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Permalink to the file.
        /// </summary>
        public string Permalink { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the latest commit date.
        /// </summary>
        public required DateTimeOffset LatestCommitDate { get; set; }
    }
}
