using System.Net;
using System.Text.Json;
using console_app.Models;
using Microsoft.Extensions.Configuration;

namespace console_app.Services;

public sealed class GitHubRepoService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GitHubRepoService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["GitHub:ApiBaseUrl"] ?? "https://api.github.com";
    }

    /// <summary>
    /// Fetches public repos for the given username and returns the top 5 by stargazer count.
    /// </summary>
    /// <returns>List of normalized repo summaries, or null if the user was not found or request failed.</returns>
    public async Task<List<RepositorySummary>?> GetTopReposAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var url = $"{_apiBaseUrl.TrimEnd('/')}/users/{username.Trim()}/repos";
        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var dtos = JsonSerializer.Deserialize<List<GitHubRepoDto>>(json, JsonOptions);

        if (dtos is null || dtos.Count == 0)
            return [];

        var topFive = dtos
            .OrderByDescending(r => r.StargazersCount)
            .Take(5)
            .Select(MapToSummary)
            .ToList();

        return topFive;
    }

    private static RepositorySummary MapToSummary(GitHubRepoDto dto)
    {
        return new RepositorySummary
        {
            Name = dto.Name,
            Description = dto.Description ?? "",
            Stars = dto.StargazersCount,
            PrimaryLanguage = dto.Language ?? "",
            Url = dto.HtmlUrl,
            LastUpdated = ParseAndFormatDate(dto.UpdatedAt)
        };
    }

    private static string ParseAndFormatDate(string? updatedAt)
    {
        if (string.IsNullOrWhiteSpace(updatedAt))
            return "";

        if (DateTime.TryParse(updatedAt, out var dt))
            return dt.ToString("yyyy-MM-dd HH:mm UTC");

        return updatedAt;
    }
}
