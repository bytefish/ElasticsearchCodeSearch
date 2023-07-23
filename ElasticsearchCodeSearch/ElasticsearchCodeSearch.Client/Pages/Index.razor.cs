// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Client.Infrastructure;
using ElasticsearchCodeSearch.Shared.Dto;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Client.Components.Pagination;

namespace ElasticsearchCodeSearch.Client.Pages
{
    public partial class Index : IAsyncDisposable
    {
        /// <summary>
        /// Elasticsearch Search Client.
        /// </summary>
        [Inject]
        public ElasticsearchCodeSearchService ElasticsearchCodeSearchService { get; set; } = default!;

        /// <summary>
        /// Pagination.
        /// </summary>
        private readonly PaginatorState Pagination = new PaginatorState { ItemsPerPage = 25, TotalItemCount = 10 };

        /// <summary>
        /// Reacts on Paginator Changes.
        /// </summary>
        private readonly EventCallbackSubscriber<PaginatorState> CurrentPageItemsChanged;

        /// <summary>
        /// Sort Options for all available fields.
        /// </summary>
        private static readonly List<Option<string>> SortOptions = new()
        {
            { new Option<string> { Value = "owner_asc", Text = "Owner (Ascending)" } },
            { new Option<string> { Value = "owner_desc", Text = "Owner (Descending)" } },
            { new Option<string> { Value = "repository_asc", Text = "Repository (Ascending)" } },
            { new Option<string> { Value = "repository_desc", Text = "Repository (Descending)" } },
            { new Option<string> { Value = "latestCommitDate_asc", Text = "Recently Updated (Ascending)" } },
            { new Option<string> { Value = "latestCommitDate_desc", Text = "Recently Updated (Descending)", Selected = true } },
        };

        /// <summary>
        /// The Selected Sort Option:
        /// </summary>
        public Option<string> SelectedSortOption { get; set; }
        
        /// <summary>
        /// When loading data, we need to cancel previous requests.
        /// </summary>
        private CancellationTokenSource? _pendingDataLoadCancellationTokenSource;

        /// <summary>
        /// Search Results for a given query.
        /// </summary>
        List<CodeSearchResultDto> CodeSearchResults { get; set; } = new List<CodeSearchResultDto>();

        /// <summary>
        /// The current Query String to send to the Server (Elasticsearch QueryString format).
        /// </summary>
        string QueryString { get; set; } = string.Empty;

        /// <summary>
        /// Total Item Count.
        /// </summary>
        int TotalItemCount { get; set; } = 0;

        public Index()
        {
            CurrentPageItemsChanged = new(EventCallback.Factory.Create<PaginatorState>(this, QueryAsync));
            SelectedSortOption = SortOptions.First(x => x.Value == "latestCommitDate_desc");
        }

        /// <inheritdoc />
        protected override Task OnParametersSetAsync()
        {
            // The associated pagination state may have been added/removed/replaced
            CurrentPageItemsChanged.SubscribeOrMove(Pagination?.CurrentPageItemsChanged);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Queries the Backend and cancels all pending requests.
        /// </summary>
        /// <returns>An awaitable task</returns>
        public async Task QueryAsync()
        {
            // Cancel all Pending Search Requests
            _pendingDataLoadCancellationTokenSource?.Cancel();
            
            // Initialize the new CancellationTokenSource
            var loadingCts = _pendingDataLoadCancellationTokenSource = new CancellationTokenSource();

            // Get From and Size for Pagination:
            var from = Pagination.CurrentPageIndex * Pagination.ItemsPerPage;
            var size = Pagination.ItemsPerPage;

            // Get the Sort Field to Sort results for
            var sortField = GetSortField();

            // Construct the Request
            var searchRequestDto = new CodeSearchRequestDto
            {
                Query = QueryString,
                From = from,
                Size = size,
                Sort = new List<SortFieldDto>() { sortField }
            };
            
            // Query the API
            var results = await ElasticsearchCodeSearchService.QueryAsync(searchRequestDto, loadingCts.Token);

            if(results == null)
            {
                return; // TODO Show Error ...
            }

            // Set the Search Results:
            CodeSearchResults = results.Results;
            TotalItemCount = results.Total;

            // Refresh the Pagination:
            await Pagination.SetTotalItemCountAsync(results.Total);

            StateHasChanged();
        }

        private async Task EnterSubmit(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await QueryAsync();
            }
        }

        private SortFieldDto GetSortField()
        {
            return SelectedSortOption.Value switch
            {
                "owner_asc" => new SortFieldDto() { Field = "owner", Order = SortOrderEnumDto.Asc },
                "owner_desc" => new SortFieldDto() { Field = "owner", Order = SortOrderEnumDto.Desc },
                "repository_asc" => new SortFieldDto() { Field = "repository", Order = SortOrderEnumDto.Asc },
                "repository_desc" => new SortFieldDto() { Field = "repository", Order = SortOrderEnumDto.Desc },
                "latestCommitDate_asc" => new SortFieldDto() { Field = "latestCommitDate", Order = SortOrderEnumDto.Asc },
                "latestCommitDate_desc" => new SortFieldDto() { Field = "latestCommitDate", Order = SortOrderEnumDto.Desc },
                _ => throw new ArgumentException($"Unknown SortField '{SelectedSortOption.Value}'"),
            };
        }

        public ValueTask DisposeAsync()
        {
            CurrentPageItemsChanged.Dispose();
            
            GC.SuppressFinalize(this);

            return ValueTask.CompletedTask;
        }
    }
}