using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    /// <summary>
    /// Git Repository Metadata.
    /// </summary>
    public class GitRepositoryMetadataDto
    {
        /// <summary>
        /// Gets or sets the owner of the Repository, for example a Username or Organization.
        /// </summary>
        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Repository.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets the FullName given by Owner and Repository Name.
        /// </summary>
        [JsonPropertyName("fullName")]
        public string FullName => $"{Owner}/{Name}";

        /// <summary>
        /// Gets or sets the Branch to index.
        /// </summary>
        [JsonPropertyName("branch")]
        public required string Branch { get; set; }

        /// <summary>
        /// Gets or sets the Repositories languages, if any.
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Url to clone from.
        /// </summary>
        [JsonPropertyName("cloneUrl")]
        public required string CloneUrl { get; set; }

        /// <summary>
        /// Gets or sets the Source System of this Repository.
        /// </summary>
        [JsonPropertyName("Source")]
        public required string Source { get; set; }


    }
}
