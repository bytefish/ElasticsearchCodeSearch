// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// A Search Request, which will be converted to Elasticsearch.
    /// </summary>
    public class CodeSearchRequest
    {
        /// <summary>
        /// Gets or sets the Search Query.
        /// </summary>
        public required string Query { get; set; }

        /// <summary>
        /// Gets or sets the number of documents to skip.
        /// </summary>
        public required int From { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of documents to fetch.
        /// </summary>
        public required int Size { get; set; } = 10;

        /// <summary>
        /// Gets or sets the sort fields.
        /// </summary>
        public required List<SortField> Sort { get; set; } = new List<SortField>();

    }
}
