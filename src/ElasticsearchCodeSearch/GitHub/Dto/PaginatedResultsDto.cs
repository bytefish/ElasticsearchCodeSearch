// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Indexer.GitHub.Dto
{
    public class PaginatedResultsDto<TEntity>
    {
        /// <summary>
        /// Gets or sets the entities fetched.
        /// </summary>
        public required List<TEntity>? Values { get; set; }

        /// <summary>
        /// Gets or sets the Page Number.
        /// </summary>
        public required int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the Page Size.
        /// </summary>
        public required int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the link to the first page.
        /// </summary>
        public string? FirstPage { get; set; }

        /// <summary>
        /// Gets or sets the link to the previous page.
        /// </summary>
        public string? PreviousPage { get; set; }

        /// <summary>
        /// Gets or sets the link to the next page.
        /// </summary>
        public string? NextPage { get; set; }

        /// <summary>
        /// Gets or sets the link to the last page.
        /// </summary>
        public string? LastPage { get; set; }
    }
}