namespace ElasticsearchCodeSearch.Database.Models
{
    public class CodeSearchIndexJob
    {
        /// <summary>
        /// Gets or sets the Repository Name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the Branch to index.
        /// </summary>
        public required string Branch { get; set; }

        /// <summary>
        /// Gets or sets the Git URL to clone.
        /// </summary>
        public string? GitUrl { get; set; }

        /// <summary>
        /// Gets or sets the repositories languages.
        /// </summary>
        public string? Language { get; set; }
    }
}