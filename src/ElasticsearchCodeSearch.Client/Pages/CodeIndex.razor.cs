// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components;
using ElasticsearchCodeSearch.Shared.Services;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Client.Pages
{
    public partial class CodeIndex
    {
        /// <summary>
        /// Elasticsearch Search Client.
        /// </summary>
        [Inject]
        public ElasticsearchCodeSearchService ElasticsearchCodeSearchService { get; set; } = default!;

        /// <summary>
        /// GitHub Repositories.
        /// </summary>
        private string GitHubRepositories = string.Empty;

        /// <summary>
        /// GitHub Repositories.
        /// </summary>
        private string GitHubOrganizations = string.Empty;
        
        /// <summary>
        /// GitHub Repositories.
        /// </summary>
        private string GitRepositories = string.Empty;

        public async Task SendIndexRequests()
        {
            var gitHubOrganizations = GitHubOrganizations
                .Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            foreach(var gitHubOrganization in gitHubOrganizations)
            {
                var request = new IndexOrganizationRequestDto 
                { 
                    Organization = gitHubOrganization 
                };

                await ElasticsearchCodeSearchService.IndexGitHubOrganizationAsync(request, default);
            }

            var gitHubRepositories = GitHubRepositories
                .Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            foreach (var gitHubRepository in gitHubRepositories)
            {
                var components = gitHubRepository
                    .Split("/")
                    .ToList();

                if(components.Count != 2)
                {
                    continue;
                }

                var request = new IndexGitHubRepositoryRequestDto 
                { 
                    Owner = components[0],
                    Repository = components[1]
                };

                await ElasticsearchCodeSearchService.IndexGitHubRepositoryAsync(request, default);
            }

            GitHubOrganizations = string.Empty;
            GitHubRepositories = string.Empty;
            GitRepositories = string.Empty;
        }
    }
}