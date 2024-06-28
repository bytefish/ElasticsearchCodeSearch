// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using Microsoft.Extensions.Localization;

namespace ElasticsearchCodeSearch.Web.Client.Pages
{
    public partial class ManageSearchIndex
    {
        /// <summary>
        /// Recreates the Search Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleRecreateSearchIndexAsync()
        {
            await ElasticsearchCodeSearchService.DeleteSearchIndexAsync(default);
            await ElasticsearchCodeSearchService.CreateSearchIndexAsync(default);
        }

        /// <summary>
        /// Creates the Search Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleCreateSearchIndexAsync()
        {
            await ElasticsearchCodeSearchService.CreateSearchIndexAsync(default);
        }
        
        /// <summary>
        /// Deletes the Search Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleDeleteSearchIndexAsync()
        {
            await ElasticsearchCodeSearchService.DeleteSearchIndexAsync(default);
        }

        /// <summary>
        /// Deletes all Documents from Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleDeleteAllDocumentsAsync()
        {
            await ElasticsearchCodeSearchService.DeleteAllDocumentsAsync(default);
        }
    }
}