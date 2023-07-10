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
        /// Searches for Code in the Elasticsearch Cluster.
        /// </summary>
        /// <param name="codeSearchRequestDto">Query to send to Elasticsearch</param>
        /// <param name="cancellationToken">Cancellation Token to cancel asynchronous processing</param>
        /// <returns>Code Search Results with Hits and Metadata</returns>
        Task<CodeSearchResultsDto> SearchDocuments(CodeSearchRequestDto codeSearchRequestDto, CancellationToken cancellationToken);
    }
}