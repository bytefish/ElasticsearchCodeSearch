// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components;
using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using Microsoft.Extensions.Localization;

namespace ElasticsearchCodeSearch.Web.Client.Pages
{
    public partial class GitHubRepositoryCodeIndex
    {
        /// <summary>
        /// GitHub Repositories.
        /// </summary>
        private IndexGitHubRepositoryRequestDto CurrentGitRepository = new IndexGitHubRepositoryRequestDto
        {
            Owner = string.Empty,
            Repository = string.Empty,
        };

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleValidSubmitAsync()
        {
            await ElasticsearchCodeSearchService.IndexGitHubRepositoryAsync(CurrentGitRepository, default);

            CurrentGitRepository = new IndexGitHubRepositoryRequestDto
            {
                Owner = string.Empty,
                Repository = string.Empty,
            };
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private Task HandleDiscardAsync()
        {
            CurrentGitRepository = new IndexGitHubRepositoryRequestDto
            {
                Owner = string.Empty,
                Repository = string.Empty,
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates a <see cref="IndexGitHubRepositoryRequestDto"/>.
        /// </summary>
        /// <param name="repository">Item to validate</param>
        /// <returns>The list of validation errors for the EditContext model fields</returns>
        private IEnumerable<ValidationError> ValidateGitRepository(IndexGitHubRepositoryRequestDto repository)
        {
            if (string.IsNullOrWhiteSpace(repository.Owner))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Owner),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Owner))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.Repository))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Repository),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Repository))
                };
            }
        }
    }
}