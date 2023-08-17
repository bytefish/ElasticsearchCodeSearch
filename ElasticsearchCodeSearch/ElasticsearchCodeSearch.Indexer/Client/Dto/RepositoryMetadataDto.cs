// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchCodeSearch.Indexer.Client.Dto
{
    public class RepositoryMetadataDto
    {
        [JsonPropertyName("id")]
        public required int Id { get; set; }

        [JsonPropertyName("node_id")]
        public required string NodeId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("full_name")]
        public required string FullName { get; set; }

        [JsonPropertyName("default_branch")]
        public required string DefaultBranch { get; set; }

        [JsonPropertyName("owner")]
        public required RepositoryOwnerDto Owner { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("git_url")]
        public string? GitUrl { get; set; }
        
        [JsonPropertyName("clone_url")]
        public string? CloneUrl { get; set; }

        [JsonPropertyName("sshUrl")]
        public string? SshUrl { get; set; }

        [JsonPropertyName("updated_at")]
        public required DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("created_at")]
        public required DateTime CreatedAt { get; set; }

        [JsonPropertyName("pushed_at")]
        public required DateTime PushedAt { get; set; }

        [JsonPropertyName("size")]
        public required int Size { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

    }
}
