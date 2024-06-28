// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using Microsoft.Extensions.Localization;

namespace ElasticsearchCodeSearch.Web.Client.Pages
{
    public partial class GitHubOrganizationCodeIndex
    {
        /// <summary>
        /// GitHub Repositories.
        /// </summary>
        private IndexGitHubOrganizationRequestDto CurrentGitRepository = new IndexGitHubOrganizationRequestDto
        {
            Organization = string.Empty
        };

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleValidSubmitAsync()
        {
            await ElasticsearchCodeSearchService.IndexGitHubOrganizationAsync(CurrentGitRepository, default);

            CurrentGitRepository = new IndexGitHubOrganizationRequestDto
            {
                Organization = string.Empty
            };
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private Task HandleDiscardAsync()
        {
            CurrentGitRepository = new IndexGitHubOrganizationRequestDto
            {
                Organization = string.Empty
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates a <see cref="IndexGitHubOrganizationRequestDto "/>.
        /// </summary>
        /// <param name="repository">Item to validate</param>
        /// <returns>The list of validation errors for the EditContext model fields</returns>
        private IEnumerable<ValidationError> ValidateGitRepository(IndexGitHubOrganizationRequestDto repository)
        {
            if (string.IsNullOrWhiteSpace(repository.Organization))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Organization),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Organization))
                };
            }
        }
    }
}