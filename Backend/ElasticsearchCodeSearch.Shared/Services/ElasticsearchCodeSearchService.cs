﻿using ElasticsearchCodeSearch.Shared.Dto;
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
    }
}