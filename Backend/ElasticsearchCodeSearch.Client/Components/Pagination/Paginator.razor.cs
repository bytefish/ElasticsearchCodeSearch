using ElasticsearchCodeSearch.Client.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;

namespace ElasticsearchCodeSearch.Client.Components.Pagination
{

    /// <summary>
    /// A component that provides a user interface for <see cref="PaginationState"/>.
    /// </summary>
    public partial class Paginator : FluentComponentBase, IDisposable
    {
        private readonly EventCallbackSubscriber<PaginatorState> _totalItemCountChanged;

        /// <summary>
        /// Specifies the associated <see cref="PaginatorState"/>. This parameter is required.
        /// </summary>
        [Parameter, EditorRequired] public PaginatorState State { get; set; } = default!;

        /// <summary>
        /// Optionally supplies a template for rendering the page count summary.
        /// </summary>
        [Parameter] public RenderFragment? SummaryTemplate { get; set; }

        /// <summary>
        /// Constructs an instance of <see cref="FluentPaginator" />.
        /// </summary>
        public Paginator()
        {
            // The "total item count" handler doesn't need to do anything except cause this component to re-render
            _totalItemCountChanged = new(new EventCallback<PaginatorState>(this, null));
        }

        private Task GoFirstAsync() => GoToPageAsync(0);
        private Task GoPreviousAsync() => GoToPageAsync(State.CurrentPageIndex - 1);
        private Task GoNextAsync() => GoToPageAsync(State.CurrentPageIndex + 1);
        private Task GoLastAsync() => GoToPageAsync(State.LastPageIndex.GetValueOrDefault(0));

        private bool CanGoBack => State.CurrentPageIndex > 0;
        private bool CanGoForwards => State.CurrentPageIndex < State.LastPageIndex;

        private Task GoToPageAsync(int pageIndex)
            => State.SetCurrentPageIndexAsync(pageIndex);

        /// <inheritdoc />
        protected override void OnParametersSet()
            => _totalItemCountChanged.SubscribeOrMove(State.TotalItemCountChangedSubscribable);

        /// <inheritdoc />
        public void Dispose()
            => _totalItemCountChanged.Dispose();
    }
}
