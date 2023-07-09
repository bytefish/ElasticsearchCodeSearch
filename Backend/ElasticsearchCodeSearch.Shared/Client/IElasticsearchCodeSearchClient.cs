// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Shared.Client
{
    /// <summary>
    /// Client for the ElasticsearchCodeSearch API.
    /// </summary>
    public interface IElasticsearchCodeSearchClient : IDisposable
    {
        /// <summary>
        /// Searches for Documents.
        /// </summary>
        /// <param name="codeSearchRequestDto">SearchRequest to send</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<CodeSearchResultsDto> SearchDocuments(CodeSearchRequestDto codeSearchRequestDto, CancellationToken cancellationToken);
    }
}