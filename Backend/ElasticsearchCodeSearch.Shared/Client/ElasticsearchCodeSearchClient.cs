// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text;
using System.Text.Json;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Http.Builder;
using ElasticsearchCodeSearch.Shared.Http.Constants;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace ElasticsearchCodeSearch.Shared.Client
{
    public class ElasticsearchCodeSearchClient : IElasticsearchCodeSearchClient
    {
        private readonly ILogger<ElasticsearchCodeSearchClient> _logger;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        
        private bool _disposedValue;

        public ElasticsearchCodeSearchClient(ILogger<ElasticsearchCodeSearchClient> logger, string baseUrl)
        {
            _logger = logger;
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        public async Task<CodeSearchResultsDto> SearchDocuments(CodeSearchRequestDto codeSearchRequestDto, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var stringContent = JsonSerializer.Serialize(codeSearchRequestDto);

            var httpMessageBuilder = new HttpRequestMessageBuilder($"{_baseUrl}/", HttpMethod.Post)
                .SetStringContent(stringContent, Encoding.UTF8, MediaTypeNames.ApplicationJson);

            var result = await SendAsync<CodeSearchResultsDto>(httpMessageBuilder, default, cancellationToken);

            if(result == null)
            {
                throw new Exception("Received empty response");
            }

            return result;
        }

        protected virtual Task<TResponseType?> SendAsync<TResponseType>(HttpRequestMessageBuilder builder, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return SendAsync<TResponseType>(builder, default, cancellationToken);
        }

        protected virtual async Task<TResponseType?> SendAsync<TResponseType>(HttpRequestMessageBuilder builder, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Build the Request Message:
            var httpRequestMessage = builder.Build();

            // Invoke actions before the Request:
            OnBeforeRequest(httpRequestMessage);

            // Invoke the Request:
            HttpResponseMessage httpResponseMessage = await _httpClient
                .SendAsync(httpRequestMessage, completionOption, cancellationToken)
                .ConfigureAwait(false);

            // Invoke actions after the Request:
            OnAfterResponse(httpRequestMessage, httpResponseMessage);

            // Apply the Response Interceptors:
            EvaluateResponse(httpResponseMessage);

            // Now read the Response Content as String:
            string httpResponseContentAsString = await httpResponseMessage.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            // And finally return the Object:
            return JsonSerializer.Deserialize<TResponseType>(httpResponseContentAsString);
        }

        protected virtual Task SendAsync(HttpRequestMessageBuilder builder, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return SendAsync(builder, default, cancellationToken);
        }

        protected virtual async Task SendAsync(HttpRequestMessageBuilder builder, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Build the Request Message:
            var httpRequestMessage = builder.Build();

            // Invoke actions before the Request:
            OnBeforeRequest(httpRequestMessage);

            // Invoke the Request:
            HttpResponseMessage httpResponseMessage = await _httpClient
                .SendAsync(httpRequestMessage, completionOption, cancellationToken)
                .ConfigureAwait(false);

            // Invoke actions after the Request:
            OnAfterResponse(httpRequestMessage, httpResponseMessage);

            // Apply the Response Interceptors:
            EvaluateResponse(httpResponseMessage);
        }

        protected virtual void OnBeforeRequest(HttpRequestMessage httpRequestMessage)
        {
            _logger.TraceMethodEntry();
        }

        protected virtual void OnAfterResponse(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
        {
            _logger.TraceMethodEntry();
        }

        protected virtual void EvaluateResponse(HttpResponseMessage response)
        {
            _logger.TraceMethodEntry();

            if (response == null)
            {
                return;
            }

            HttpStatusCode httpStatusCode = response.StatusCode;

            if (httpStatusCode == HttpStatusCode.OK)
            {
                return;
            }

            if ((int)httpStatusCode >= 400)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
                
                _httpClient.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}