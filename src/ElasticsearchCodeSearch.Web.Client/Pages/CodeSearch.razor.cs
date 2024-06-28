// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Web.Client.Components;
using ElasticsearchCodeSearch.Web.Client.Models;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Web.Client.Pages
{
    public partial class CodeSearch : IAsyncDisposable
    {
        /// <summary>
        /// Elasticsearch Search Client.
        /// </summary>
        [Inject]
        public ElasticsearchCodeSearchService ElasticsearchCodeSearchService { get; set; } = default!;

        /// <summary>
        /// The current Query String to send to the Server (Elasticsearch QueryString format).
        /// </summary>
        [Parameter]
        public string? QueryString { get; set; }

        /// <summary>
        /// The Selected Sort Option:
        /// </summary>
        [Parameter]
        public SortOptionEnum? SortOption { get; set; }

        /// <summary>
        /// Page Number.
        /// </summary>
        [Parameter]
        public int? Page { get; set; }

        /// <summary>
        /// Page Number.
        /// </summary>
        [Parameter]
        public int? PageSize { get; set; }

        /// <summary>
        /// Pagination.
        /// </summary>
        private PaginatorState _pagination = new PaginatorState
        {
            ItemsPerPage = 10
        };

        /// <summary>
        /// Reacts on Paginator Changes.
        /// </summary>
        private readonly EventCallbackSubscriber<PaginatorState> _currentPageItemsChanged;

        /// <summary>
        /// Sort Options for all available fields.
        /// </summary>
        private static readonly SortOptionEnum[] _sortOptions = new[]
        {
            SortOptionEnum.OwnerAscending,
            SortOptionEnum.OwnerDescending,
            SortOptionEnum.RepositoryAscending,
            SortOptionEnum.RepositoryDescending,
            SortOptionEnum.LatestCommitDateAscending,
            SortOptionEnum.LatestCommitDateDescending,
        };

        /// <summary>
        /// The currently selected Sort Option
        /// </summary>
        private SortOptionEnum _selectedSortOption { get; set; }
        
        /// <summary>
        /// When loading data, we need to cancel previous requests.
        /// </summary>
        private CancellationTokenSource? _pendingDataLoadCancellationTokenSource;

        /// <summary>
        /// Search Results for a given query.
        /// </summary>
        private List<CodeSearchResultDto> _codeSearchResults { get; set; } = new List<CodeSearchResultDto>();

        /// <summary>
        /// The current Query String to send to the Server (Elasticsearch QueryString format).
        /// </summary>
        private string _queryString { get; set; } = string.Empty;

        /// <summary>
        /// Total Item Count.
        /// </summary>
        private int _totalItemCount { get; set; } = 0;

        /// <summary>
        /// Processing Time.
        /// </summary>
        private decimal _tookInSeconds { get; set; } = 0;

        public CodeSearch()
        {
            _currentPageItemsChanged = new(EventCallback.Factory.Create<PaginatorState>(this, QueryAsync));
        }

        /// <inheritdoc />
        protected override Task OnParametersSetAsync()
        {
            // Set bound values, so we don't modify the parameters directly
            _queryString = QueryString ?? string.Empty;
            _selectedSortOption = SortOption ?? SortOptionEnum.LatestCommitDateDescending;
            _pagination.ItemsPerPage = PageSize ?? 10;

            // The associated pagination state may have been added/removed/replaced
            _currentPageItemsChanged.SubscribeOrMove(_pagination.CurrentPageItemsChanged);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Queries the Backend and cancels all pending requests.
        /// </summary>
        /// <returns>An awaitable task</returns>
        public async Task QueryAsync()
        {
            // Do not execute empty queries ...
            if(string.IsNullOrWhiteSpace(_queryString))
            {
                return;
            }

            try
            {
                // Cancel all Pending Search Requests
                _pendingDataLoadCancellationTokenSource?.Cancel();

                // Initialize the new CancellationTokenSource
                var loadingCts = _pendingDataLoadCancellationTokenSource = new CancellationTokenSource();

                // Get From and Size for Pagination:
                var from = _pagination.CurrentPageIndex * _pagination.ItemsPerPage;
                var size = _pagination.ItemsPerPage;

                // Get the Sort Field to Sort results for
                var sortField = GetSortField(_selectedSortOption);

                // Construct the Request
                var searchRequestDto = new CodeSearchRequestDto
                {
                    Query = _queryString,
                    From = from,
                    Size = size,
                    Sort = new List<SortFieldDto>() { sortField }
                };

                // Query the API
                var results = await ElasticsearchCodeSearchService.QueryAsync(searchRequestDto, loadingCts.Token);

                if (results == null)
                {
                    return; // TODO Show Error ...
                }

                // Set the Search Results:
                _codeSearchResults = results.Results;
                _tookInSeconds = results.TookInMilliseconds / (decimal) 1000;
                _totalItemCount = (int)results.Total;

                // Refresh the Pagination:
                await _pagination.SetTotalItemCountAsync(_totalItemCount);
            } 
            catch(Exception)
            {
                // Pokemon Exception Handling
            }

            StateHasChanged();
        }

        private async Task EnterSubmit(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await QueryAsync();
            }
        }

        private static SortOptionEnum GetSortOption(string? sortOptionString, SortOptionEnum defaultValue)
        {
            if(string.IsNullOrWhiteSpace(sortOptionString))
            {
                return defaultValue;
            }

            bool success = Enum.TryParse<SortOptionEnum>(sortOptionString, true, out var parsedSortOption);

            if(!success)
            {
                return defaultValue;
            }

            return parsedSortOption;
        }

        private static SortFieldDto GetSortField(SortOptionEnum sortOptionEnum)
        {
            // TODO This should be an enumeration in the Backend...
            return sortOptionEnum switch
            {
                SortOptionEnum.OwnerAscending => new SortFieldDto() { Field = "owner", Order = SortOrderEnumDto.Asc },
                SortOptionEnum.OwnerDescending => new SortFieldDto() { Field = "owner", Order = SortOrderEnumDto.Desc },
                SortOptionEnum.RepositoryAscending => new SortFieldDto() { Field = "repository", Order = SortOrderEnumDto.Asc },
                SortOptionEnum.RepositoryDescending => new SortFieldDto() { Field = "repository", Order = SortOrderEnumDto.Desc },
                SortOptionEnum.LatestCommitDateAscending => new SortFieldDto() { Field = "latestCommitDate", Order = SortOrderEnumDto.Asc },
                SortOptionEnum.LatestCommitDateDescending => new SortFieldDto() { Field = "latestCommitDate", Order = SortOrderEnumDto.Desc },
                _ => throw new ArgumentException($"Unknown SortField '{sortOptionEnum}'"),
            };
        }

        public ValueTask DisposeAsync()
        {
            _currentPageItemsChanged.Dispose();
            
            GC.SuppressFinalize(this);

            return ValueTask.CompletedTask;
        }
    }
}