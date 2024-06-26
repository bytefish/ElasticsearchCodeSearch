﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components;
using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Client.Infrastructure;
using Microsoft.Extensions.Localization;

namespace ElasticsearchCodeSearch.Client.Pages
{
    public partial class CodeIndex
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
            };

            return Task.CompletedTask;
        }


        /// <summary>
        /// Validates a <see cref="TaskItem"/>.
        /// </summary>
        /// <param name="taskItem">TaskItem to validate</param>
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
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Branch))
                };
            }

        }

    }
}