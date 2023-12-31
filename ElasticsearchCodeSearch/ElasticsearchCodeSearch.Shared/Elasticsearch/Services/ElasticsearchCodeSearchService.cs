﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Exceptions;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;

namespace ElasticsearchCodeSearch.Shared.Services
{
    public class ElasticsearchCodeSearchService
    {
        private readonly ILogger<ElasticsearchCodeSearchService> _logger;
        private readonly HttpClient _httpClient;

        public ElasticsearchCodeSearchService(ILogger<ElasticsearchCodeSearchService> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CodeSearchResultsDto?> QueryAsync(CodeSearchRequestDto codeSearchRequestDto, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsJsonAsync("search-documents", codeSearchRequestDto, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<CodeSearchResultsDto>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task IndexDocumentsAsync(List<CodeSearchDocumentDto> documents, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsJsonAsync("index-documents", documents, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task<List<CodeSearchStatisticsDto>?> SearchStatisticsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .GetAsync("search-statistics", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<List<CodeSearchStatisticsDto>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}