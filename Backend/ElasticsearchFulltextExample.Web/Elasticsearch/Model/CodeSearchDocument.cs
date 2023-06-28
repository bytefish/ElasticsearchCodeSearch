// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Web.Elasticsearch.Model
{
    /// <summary>
    /// Sourcecode Document, which is going to be indexed.
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
        /// The Filename of the uploaded document.
        /// </summary>
        public required string Filename { get; set; }

        /// <summary>
        /// The Data of the Document.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// Latest Commit Date.
        /// </summary>
        public required DateTime LatestCommitDate { get; set; }
    }
}
