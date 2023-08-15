// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Indexer.Settings
{
    /// <summary>
    /// AppSettings for the Indexer.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the base directory to clone to.
        /// </summary>
        public required string BaseDirectory { get; set; } 

        /// <summary>
        /// Gets or sets the allowed extensions.
        /// </summary>
        public required string[] AllowedExtensions { get; set; }

        /// <summary>
        /// Gets or sets the allowed filenames.
        /// </summary>
        public required string[] AllowedFilenames { get; set; }

        /// <summary>
        /// Gets or sets the degree of parallelism for clones.
        /// </summary>
        public int MaxParallelClones { get; set; }
        
        /// <summary>
        /// Gets or sets the degree of parallelism for bulk requests.
        /// </summary>
        public int MaxParallelBulkRequests { get; set; }
    }
}
