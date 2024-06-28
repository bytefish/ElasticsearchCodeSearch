// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using Microsoft.Extensions.Localization;
using ElasticsearchCodeSearch.Shared.Constants;

namespace ElasticsearchCodeSearch.Web.Client.Pages
{
    public partial class GitRepositoryCodeIndex
    {
        /// <summary>
        /// GitHub Repositories.
        /// </summary>
        private GitRepositoryMetadataDto CurrentGitRepository = new GitRepositoryMetadataDto
        {
            Owner = string.Empty,
            Name = string.Empty,
            Branch = string.Empty,
            CloneUrl = string.Empty,
            Language = string.Empty,
            Source = SourceSystems.GitHub,
        };


        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleValidSubmitAsync()
        {
            await ElasticsearchCodeSearchService.IndexGitRepositoryAsync(CurrentGitRepository, default);

            CurrentGitRepository = new GitRepositoryMetadataDto
            {
                Branch = string.Empty,
                Name = string.Empty,
                CloneUrl = string.Empty,
                Owner = string.Empty,
                Source = SourceSystems.GitHub,
            };
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private Task HandleDiscardAsync()
        {
            CurrentGitRepository = new GitRepositoryMetadataDto
            {
                Branch = string.Empty,
                Name = string.Empty,
                CloneUrl = string.Empty,
                Owner = string.Empty,
                Source = SourceSystems.GitHub
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates a <see cref="GitRepositoryMetadataDto"/>.
        /// </summary>
        /// <param name="repository">Item to validate</param>
        /// <returns>The list of validation errors for the EditContext model fields</returns>
        private IEnumerable<ValidationError> ValidateGitRepository(GitRepositoryMetadataDto repository)
        {
            if (string.IsNullOrWhiteSpace(repository.Owner))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Owner),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Owner))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.Name))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Name),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Name))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.Branch))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Branch),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Branch))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.CloneUrl))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.CloneUrl),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.CloneUrl))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.Source))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Source),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Source))
                };
            }
        }
    }
}