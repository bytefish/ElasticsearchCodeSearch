// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Indexer.Client.Dto
{
    public class PaginatedResultsDto<TEntity>
    {
        public required List<TEntity>? Values { get; set; }

        public required int PageNumber { get; set; }

        public required int PageSize { get; set; }
                
        public string? FirstPage { get; set; }

        public string? PreviousPage { get; set; }

        public string? NextPage { get; set; }

        public string? LastPage { get; set; }

    }
}
