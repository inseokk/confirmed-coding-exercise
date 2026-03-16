using System.Net;
using System.Text.Json;
using console_app.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace console_app.Services;

public sealed class GitHubRepoService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly ILogger<GitHubRepoService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const int PerPage = 100; // GitHub API max

    public GitHubRepoService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GitHubRepoService> logger)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["GitHub:ApiBaseUrl"] ?? "https://api.github.com";
        _logger = logger;
    }

    /// <summary>
    /// Fetches all public repos for the user (with pagination), then returns the top 5 by stargazer count.
    /// </summary>
    /// <returns>List of normalized repo summaries, or null if the user was not found or request failed.</returns>
    public async Task<List<RepositorySummary>?> GetTopReposAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("GetTopReposAsync called with empty username");
            return null;
        }

        username = username.Trim();
        _logger.LogInformation("Fetching repositories for user {Username}", username);

        var allDtos = new List<GitHubRepoDto>();
        var page = 1;

        try
        {
            while (true)
            {
                var url = $"{_apiBaseUrl.TrimEnd('/')}/users/{username}/repos?per_page={PerPage}&page={page}";
                using var response = await ExecuteWithRetryAsync(async () =>
                {
                    var res = await _httpClient.GetAsync(url, cancellationToken);
                    if ((int)res.StatusCode >= 500)
                        throw new HttpRequestException($"Server error: {res.StatusCode}");
                    return res;
                }, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User {Username} not found (404)", username);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var dtos = JsonSerializer.Deserialize<List<GitHubRepoDto>>(json, JsonOptions);

                if (dtos is null)
                    break;

                allDtos.AddRange(dtos);
                _logger.LogDebug("Fetched page {Page}, got {Count} repos (total so far: {Total})", page, dtos.Count, allDtos.Count);

                if (dtos.Count < PerPage)
                    break;

                page++;
            }

            if (allDtos.Count == 0)
            {
                _logger.LogInformation("User {Username} has no public repositories", username);
                return [];
            }

            var topFive = allDtos
                .OrderByDescending(r => r.StargazersCount)
                .Take(5)
                .Select(MapToSummary)
                .ToList();

            _logger.LogInformation("Returning top {Count} repos for {Username} (from {Total} total)", topFive.Count, username, allDtos.Count);
            return topFive;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for user {Username}", username);
            throw;
        }
    }

    private static async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<Task<HttpResponseMessage>> action,
        CancellationToken cancellationToken)
    {
        var retryPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<HttpRequestException>(),
                OnRetry = args =>
                {
                    Console.WriteLine($"Retry {args.AttemptNumber} after transient failure. Next retry in {args.RetryDelay.TotalSeconds}s.");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        return await retryPolicy.ExecuteAsync(async _ => await action(), cancellationToken);
    }

    internal static RepositorySummary MapToSummary(GitHubRepoDto dto)
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
