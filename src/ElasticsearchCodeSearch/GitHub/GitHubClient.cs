// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.GitHub.Dto;
using ElasticsearchCodeSearch.Indexer.GitHub.Options;
using ElasticsearchCodeSearch.Shared.Exceptions;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Threading;

namespace ElasticsearchCodeSearch.Indexer.GitHub
{
    public class GitHubClient : IDisposable
    {
        private readonly ILogger<GitHubClient> _logger;
        private readonly GitHubClientOptions _options;
        private readonly HttpClient _httpClient;
        private bool disposedValue;

        public GitHubClient(ILogger<GitHubClient> logger, IOptions<GitHubClientOptions> options)
            : this(logger, options, new HttpClient())
        {
        }

        public GitHubClient(ILogger<GitHubClient> logger, IOptions<GitHubClientOptions> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options.Value;
            _httpClient = httpClient;
        }

        public async Task<List<RepositoryMetadataDto>> GetAllRepositoriesByOrganizationAsync(string organization, int pageSize, CancellationToken cancellationToken)
        {
            // Holds the Results:
            List<RepositoryMetadataDto> repositories = new List<RepositoryMetadataDto>();

            // Get the first page:
            var page = await GetRepositoriesByOrganizationAsync(organization, 1, pageSize, cancellationToken).ConfigureAwait(false);

            // If it has values, add them to the result:
            if (page.Values != null)
            {
                repositories.AddRange(page.Values);
            }

            await WaitForNextRequest(cancellationToken).ConfigureAwait(false);

            // If there is a next page, we iterate to it:
            while (page.NextPage != null)
            {
                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Gettings Repositories of Organization '{Organization}', Page '{PageNumber}' and Page Size = '{PageSize}'",
                         organization, page.PageNumber + 1, page.PageSize);
                }

                page = await GetRepositoriesByOrganizationAsync(organization, page.PageNumber + 1, pageSize, cancellationToken).ConfigureAwait(false);

                if (page.Values != null)
                {
                    repositories.AddRange(page.Values);
                }
            }

            return repositories;
        }

        public async Task WaitForNextRequest(CancellationToken cancellationToken)
        {
            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Waiting '{RequestDelayInMilliseconds}' milliseconds to query for next Request",
                    _options.RequestDelayInMilliseconds);
            }

            await Task.Delay(_options.RequestDelayInMilliseconds, cancellationToken).ConfigureAwait(false);

        }

        public async Task<RepositoryMetadataDto?> GetRepositoryByOwnerAndRepositoryAsync(string owner, string repository, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.github.com/repos/{owner}/{repository}"),
                Headers =
                {
                    { "User-Agent", "curl/8.0.1" },
                    { "Accept", "application/vnd.github+json" },
                    { "Authorization", $"Bearer {_options.AccessToken}" },
                    { "X-GitHub-Api-Version", $"2022-11-28" },
                }
            };

            var response = await _httpClient
                .SendAsync(httpRequestMessage, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture, 
                    "HTTP Request failed with Status: '{0}' ({1})", (int)response.StatusCode, response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            var repositoryMetadata = await response.Content
                .ReadFromJsonAsync<RepositoryMetadataDto>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return repositoryMetadata;
        }

        public async Task<PaginatedResultsDto<RepositoryMetadataDto>> GetRepositoriesByOrganizationAsync(string organization, int pageNum, int pageSize, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.github.com/orgs/{organization}/repos?page={pageNum}&per_page={pageSize}"),
                Headers =
                {
                    { "User-Agent", "curl/8.0.1" },
                    { "Accept", "application/vnd.github+json" },
                    { "Authorization", $"Bearer {_options.AccessToken}" },
                    { "X-GitHub-Api-Version", $"2022-11-28" },
                }
            };

            var response = await _httpClient
                .SendAsync(httpRequestMessage, cancellationToken)
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

            // Get the pagination links from the response
            var links = ParseLinks(response);

            var repositories = await response.Content
                .ReadFromJsonAsync<List<RepositoryMetadataDto>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var result = new PaginatedResultsDto<RepositoryMetadataDto>
            {
                PageNumber = pageNum,
                PageSize = pageSize,
                FirstPage = links.FirstUrl,
                PreviousPage = links.PrevUrl,
                NextPage = links.NextUrl,
                LastPage = links.LastUrl,
                Values = repositories
            };

            if(_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Paginated Repositories Response (PageNumber = '{PageNumber}', PageSize = '{PageSize}', FirstPage = '{FirstPage}', PreviousPage = '{PrevPage}', NextPage = '{NextPage}', LastPage = '{LastPage}')",
                    result.PageNumber, result.PageSize, result.FirstPage, result.PreviousPage, result.NextPage, result.LastPage);
            }

            return result;
        }

        /// <summary>
        /// Parses the Links in the Response's "Links" Header into the components.
        /// </summary>
        /// <param name="httpResponseMessage">Response Header with the Links Header</param>
        /// <returns>Links to the various pages</returns>
        public (string? FirstUrl, string? PrevUrl, string? NextUrl, string? LastUrl) ParseLinks(HttpResponseMessage httpResponseMessage)
        {
            _logger.TraceMethodEntry();

            // Get the Value for the first "Links" header, which looks like this
            //
            // <https://api.github.com/organizations/6154722/repos?per_page=1&page=2>; rel="next", <https://api.github.com/organizations/6154722/repos?per_page=1&page=5762>; rel="last"
            //
            if (!httpResponseMessage.Headers.TryGetValues("Link", out var linkHeaders))
            {
                return (null, null, null, null);
            }

            var linkValue = linkHeaders.FirstOrDefault();

            if (linkValue == null)
            {
                return (null, null, null, null);
            }

            // Split at the comma, so we get it like this:
            // [0] <https://api.github.com/organizations/6154722/repos?per_page=1&page=2>; rel="next"
            // [1] <https://api.github.com/organizations/6154722/repos?per_page=1&page=5762>; rel="last"
            var linksEntries = linkValue.Split(',', StringSplitOptions.TrimEntries);

            // Build a Dictionary with the link Types available
            var links = linksEntries
                // Split at semicolon, so it looks like this
                //
                //      [0] <https://api.github.com/organizations/6154722/repos?per_page=1&page=2>
                //      [1] rel="next"
                .Select(x => x.Split(";"))
                // We need two elements here, so we can make up a dictionary, that 
                // maps a type (first, prev, ...) to a link.
                .Where(x => x.Length == 2)
                // Get the Type and the Link, so it looks like this:
                //
                //      ["next"] = https://api.github.com/organizations/6154722/repos?per_page=1&page=2
                // 
                .ToDictionary(x => GetLinkType(x[1]).Trim(), x => GetLinkValue(x[0]).Trim());


            (string? FirstUrl, string? PrevUrl, string? NextUrl, string? LastUrl)  result = (links.GetValueOrDefault("first"), links.GetValueOrDefault("prev"), links.GetValueOrDefault("next"), links.GetValueOrDefault("last"));

            if(_logger.IsTraceEnabled())
            {
                _logger.LogTrace("Parsed Pagination from LinkValue (LinkValue = '{LinkValue}', FirstUrl = '{FirstUrl}', PrevUrl = '{PrevUrl}', NextUrl = '{NextUrl}', LastUrl = '{LastUrl}')",
                    linkValue, result.FirstUrl, result.PrevUrl, result.NextUrl, result.LastUrl);
            }

            return result;
        }

        private string GetLinkType(string source)
        {
            _logger.TraceMethodEntry();

            var linkType = source
                .Replace("rel=\"", string.Empty)
                .Replace("\"", string.Empty);

            if (_logger.IsTraceEnabled())
            {
                _logger.LogTrace("LinkType '{LinkType}' extracted from Source '{SourceLinkType}'", linkType, source);
            }

            return linkType;
        }

        private string GetLinkValue(string source)
        {
            _logger.TraceMethodEntry();

            var linkValue = source
                .Replace("<", string.Empty)
                .Replace(">", string.Empty);

            if (_logger.IsTraceEnabled())
            {
                _logger.LogTrace("Extracted LinkValue '{LinkValue}' from Source '{SourceLinkValue}'", linkValue, source);
            }

            return linkValue;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}