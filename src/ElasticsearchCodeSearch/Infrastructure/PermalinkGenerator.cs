// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Constants;

namespace ElasticsearchCodeSearch.Infrastructure
{
    public class PermalinkGenerator
    {
        private readonly ILogger<PermalinkGenerator> _logger;

        public PermalinkGenerator(ILogger<PermalinkGenerator> logger)
        {
            _logger = logger;
        }

        public virtual string GeneratePermalink(GitRepositoryMetadata repository, string commitHash, string relativeFilename)
        {
            switch (repository.Source)
            {
                case SourceSystems.GitHub:
                    return $"https://github.com/{repository.Owner}/{repository.Name}/blob/{commitHash}/{relativeFilename}";
                case SourceSystems.Codeberg:
                    return $"https://codeberg.org/{repository.Owner}/{repository.Name}/src/commit/{commitHash}/{relativeFilename}";
                case SourceSystems.GitLab:
                    return $"https://gitlab.com/{repository.Owner}/{repository.Name}/-/blob/{commitHash}/{relativeFilename}"; // TODO Is this really the Commit Hash?
                default:
                    return "Unknown Source System";
            }
        }
    }
}
