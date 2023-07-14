using ElasticsearchCodeSearch.Client.Infrastructure;
using ElasticsearchCodeSearch.Shared.Dto;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
using ElasticsearchCodeSearch.Shared.Services;
using System.Reflection;
using ElasticsearchCodeSearch.Client.Components.Pagination;
using Microsoft.Fast.Components.FluentUI.DataGrid.Infrastructure;

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
        PaginatorState Pagination = new PaginatorState { ItemsPerPage = 25, TotalItemCount = 10 };

        /// <summary>
        /// Reacts on Paginator Changes.
        /// </summary>
        private readonly EventCallbackSubscriber<PaginatorState> CurrentPageItemsChanged;

        /// <summary>
        /// Sort Options for all available fields.
        /// </summary>
        private static List<Option<string>> SortOptions = new()
        {
            { new Option<string> { Value = "owner_asc", Text = "Owner (Ascending)" } },
            { new Option<string> { Value = "owner_desc", Text = "Owner (Descending)" } },
            { new Option<string> { Value = "repository_asc", Text = "Repository (Ascending)" } },
            { new Option<string> { Value = "repository_desc", Text = "Repository (Descending)" } },
            { new Option<string> { Value = "filename_asc", Text = "Filename (Ascending)" } },
            { new Option<string> { Value = "filename_desc", Text = "Filename (Descending)" } },
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
            switch(SelectedSortOption.Value)
            {
                case "owner_asc":
                    return new SortFieldDto() { Field = "owner", Order = SortOrderEnumDto.Asc };
                case "owner_desc":
                    return new SortFieldDto() { Field = "owner", Order = SortOrderEnumDto.Desc };
                case "repository_asc":
                    return new SortFieldDto() { Field = "repository", Order = SortOrderEnumDto.Asc };
                case "repository_desc":
                    return new SortFieldDto() { Field = "repository", Order = SortOrderEnumDto.Desc };
                case "filename_asc":
                    return new SortFieldDto() { Field = "filename.tree", Order = SortOrderEnumDto.Asc };
                case "filename_desc":
                    return new SortFieldDto() { Field = "filename.tree", Order = SortOrderEnumDto.Desc };
                case "latestCommitDate_asc":
                    return new SortFieldDto() { Field = "latestCommitDate", Order = SortOrderEnumDto.Asc };
                case "latestCommitDate_desc":
                    return new SortFieldDto() { Field = "latestCommitDate", Order = SortOrderEnumDto.Desc };
                default:
                    throw new ArgumentException($"Unknown SortField '{SelectedSortOption.Value}'");
            }
        }

        public ValueTask DisposeAsync()
        {
            CurrentPageItemsChanged.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}