// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Web.Client.Localization;
using ElasticsearchCodeSearch.Web.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ElasticsearchCodeSearch.Web.Client.Components
{
    public partial class SortOptionSelector
    {
        /// <summary>
        /// Localizer.
        /// </summary>
        [Inject]
        public IStringLocalizer<SharedResource> Loc { get; set; } = default!;

        /// <summary>
        /// Text used on aria-label attribute.
        /// </summary>
        [Parameter]
        public virtual string? Title { get; set; }

        /// <summary>
        /// If true, will disable the list of items.
        /// </summary>
        [Parameter]
        public virtual bool Disabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the content to be rendered inside the component.
        /// In this case list of FluentOptions
        /// </summary>
        [Parameter]
        public virtual RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// All selectable Sort Options.
        /// </summary>
        [Parameter]
        public required SortOptionEnum[] SortOptions { get; set; }

        /// <summary>
        /// The Sort Option.
        /// </summary>
        [Parameter]
        public SortOptionEnum SortOption { get; set; }

        /// <summary>
        /// Invoked, when the SortOption has changed.
        /// </summary>
        [Parameter]
        public EventCallback<SortOptionEnum> SortOptionChanged { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        string? _value { get; set; }

        /// <summary>
        /// Filter Operator.
        /// </summary>
        private SortOptionEnum _sortOption { get; set; }

        protected override void OnParametersSet()
        {
            _sortOption = SortOption;
            _value = SortOption.ToString();
        }

        public void OnSelectedValueChanged(SortOptionEnum value)
        {
            _sortOption = value;
            _value = value.ToString();

            SortOptionChanged.InvokeAsync(_sortOption);
        }
    }
}