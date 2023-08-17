namespace ElasticsearchCodeSearch.Shared.Dto
{
    public class CodeSearchRepositoryDto
    {
        /// <summary>
        /// Gets or sets a unique idenfitifer.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the repository.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the owner of the repository.
        /// </summary>
        public required string Owner { get; set; }

        /// <summary>
        /// Gets or sets the URL for cloning.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the SSH URL for cloning.
        /// </summary>
        public string? SshUrl { get; set; }

        /// <summary>
        /// Gets or sets the time the repository has been updated at.
        /// </summary>
        public required DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the time the repository has been created at.
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the time the repository has been pushed to.
        /// </summary>
        public required DateTime PushedAt { get; set; }

        /// <summary>
        /// Gets or sets the size in kilobytes for the repository.
        /// </summary>
        public required int SizeInKilobytes { get; set; }

        /// <summary>
        /// Gets or sets the language of the code in the repository.
        /// </summary>
        public string? Language { get; set; }
    }
}