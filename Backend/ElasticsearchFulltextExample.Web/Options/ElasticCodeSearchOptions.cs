// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Web.Options
{
    /// <summary>
    /// Elasticsearch options.
    /// </summary>
    public class ElasticCodeSearchOptions
    {
        /// <summary>
        /// Endpoint of the Elasticsearch Node.
        /// </summary>
        public required string Uri { get; set; }

        /// <summary>
        /// Index to use for Code Search.
        /// </summary>
        public required string IndexName { get; set; }
    }
}
