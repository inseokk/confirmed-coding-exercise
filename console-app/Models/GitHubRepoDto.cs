using System.Text.Json.Serialization;

namespace console_app.Models;

/// <summary>
/// DTO for GitHub API GET /users/{username}/repos response items.
/// </summary>
internal sealed class GitHubRepoDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("stargazers_count")]
    public int StargazersCount { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; init; }
}