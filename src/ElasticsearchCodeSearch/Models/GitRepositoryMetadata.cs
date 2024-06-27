namespace ElasticsearchCodeSearch.Models
{
    /// <summary>
    /// Git Repository Metadata.
    /// </summary>
    public record GitRepositoryMetadata
    {
        /// <summary>
        /// Gets or sets the owner of the Repository, for example a Username or Organization.
        /// </summary>
        public required string Owner { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Repository.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets the FullName given by Owner and Repository Name.
        /// </summary>
        public string FullName => $"{Owner}/{Name}";

        /// <summary>
        /// Gets or sets the Branch to index.
        /// </summary>
        public required string Branch { get; set; }

        /// <summary>
        /// Gets or sets the Repositories languages, if any.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Url to clone from.
        /// </summary>
        public required string CloneUrl { get; set; }

        /// <summary>
        /// Gets or sets the Source System.
        /// </summary>
        public required string Source { get; set; }
    }
}
