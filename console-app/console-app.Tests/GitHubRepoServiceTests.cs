using System.Net;
using System.Text.Json;
using console_app.Models;
using console_app.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace console_app.Tests;

public class GitHubRepoServiceTests
{
    private static IConfiguration CreateConfig(string? apiBaseUrl = null)
    {
        var dict = new Dictionary<string, string?>
        {
            ["GitHub:ApiBaseUrl"] = apiBaseUrl ?? "https://api.github.com"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static ILogger<GitHubRepoService> CreateLogger()
    {
        var mock = new Mock<ILogger<GitHubRepoService>>();
        return mock.Object;
    }

    [Fact]
    public void MapToSummary_maps_dto_to_summary()
    {
        var dto = new GitHubRepoDto
        {
            Name = "repo1",
            Description = "A repo",
            StargazersCount = 42,
            Language = "C#",
            HtmlUrl = "https://github.com/u/repo1",
            UpdatedAt = "2025-01-15T12:00:00Z"
        };

        var summary = GitHubRepoService.MapToSummary(dto);

        Assert.Equal("repo1", summary.Name);
        Assert.Equal("A repo", summary.Description);
        Assert.Equal(42, summary.Stars);
        Assert.Equal("C#", summary.PrimaryLanguage);
        Assert.Equal("https://github.com/u/repo1", summary.Url);
        Assert.Contains("2025-01-15", summary.LastUpdated);
    }

    [Fact]
    public void MapToSummary_handles_null_description_and_language()
    {
        var dto = new GitHubRepoDto
        {
            Name = "x",
            Description = null,
            StargazersCount = 0,
            Language = null,
            HtmlUrl = "https://github.com/u/x",
            UpdatedAt = null
        };

        var summary = GitHubRepoService.MapToSummary(dto);

        Assert.Equal("", summary.Description);
        Assert.Equal("", summary.PrimaryLanguage);
        Assert.Equal("", summary.LastUpdated);
    }

    [Fact]
    public async Task GetTopReposAsync_returns_null_for_empty_username()
    {
        var client = new HttpClient();
        var config = CreateConfig();
        var logger = CreateLogger();
        var service = new GitHubRepoService(client, config, logger);

        var result = await service.GetTopReposAsync("");
        Assert.Null(result);

        result = await service.GetTopReposAsync("   ");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTopReposAsync_returns_null_for_404()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "[]");
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com") };
        var config = CreateConfig("https://api.github.com");
        var logger = CreateLogger();
        var service = new GitHubRepoService(client, config, logger);

        var result = await service.GetTopReposAsync("nonexistent-user-xyz-12345");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTopReposAsync_returns_top_5_by_stars()
    {
        var repos = new[]
        {
            new { name = "low", stargazers_count = 1, description = "", language = "C#", html_url = "https://github.com/u/low", updated_at = "2025-01-01T00:00:00Z" },
            new { name = "high", stargazers_count = 100, description = "Hi", language = "F#", html_url = "https://github.com/u/high", updated_at = "2025-01-02T00:00:00Z" },
            new { name = "mid", stargazers_count = 50, description = "Mid", language = "VB", html_url = "https://github.com/u/mid", updated_at = "2025-01-03T00:00:00Z" },
        };
        var json = JsonSerializer.Serialize(repos);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com") };
        var config = CreateConfig("https://api.github.com");
        var logger = CreateLogger();
        var service = new GitHubRepoService(client, config, logger);

        var result = await service.GetTopReposAsync("someone");

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("high", result[0].Name);
        Assert.Equal(100, result[0].Stars);
        Assert.Equal("mid", result[1].Name);
        Assert.Equal("low", result[2].Name);
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            });
        }
    }
}
