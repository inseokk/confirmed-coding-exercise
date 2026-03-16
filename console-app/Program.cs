using System.Text.Json;
using console_app.Models;
using console_app.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configuration: appsettings.json + environment variables (env overrides)
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var outputPath = config["Output:FilePath"] ?? "repos.json";
var userAgent = config["GitHub:UserAgent"] ?? "Confirmed-Exercise";

// Dependency injection
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
services.AddSingleton<IConfiguration>(config);
services.AddHttpClient<GitHubRepoService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
});
var provider = services.BuildServiceProvider();

GitHubRepoService service;
using (var scope = provider.CreateScope())
{
    service = scope.ServiceProvider.GetRequiredService<GitHubRepoService>();
}

// Parse username from command line (first argument)
var username = args.Length > 0 ? args[0].Trim() : null;

if (string.IsNullOrEmpty(username))
{
    Console.WriteLine("Usage: dotnet run -- <github-username>");
    Console.WriteLine("Example: dotnet run -- octocat");
    Console.WriteLine("Configuration: appsettings.json and environment variables (e.g. Output__FilePath, GitHub__ApiBaseUrl).");
    return 1;
}

List<RepositorySummary>? repos;
try
{
    repos = await service.GetTopReposAsync(username);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Request failed: {ex.Message}");
    return 1;
}
catch (TaskCanceledException)
{
    Console.WriteLine("Request was cancelled or timed out.");
    return 1;
}

if (repos is null)
{
    Console.WriteLine($"User '{username}' not found or has no public repositories.");
    return 1;
}

if (repos.Count == 0)
{
    Console.WriteLine($"No public repositories found for '{username}'.");
    return 0;
}

// Print readable console output
Console.WriteLine($"Top {repos.Count} repository/repositories by stars for '{username}':");
Console.WriteLine();

for (var i = 0; i < repos.Count; i++)
{
    var r = repos[i];
    Console.WriteLine($"{i + 1}. {r.Name}");
    Console.WriteLine($"   Description: {r.Description}");
    Console.WriteLine($"   Stars: {r.Stars}  |  Language: {r.PrimaryLanguage}");
    Console.WriteLine($"   URL: {r.Url}");
    Console.WriteLine($"   Last updated: {r.LastUpdated}");
    Console.WriteLine();
}

// Write normalized results to JSON for React app
var json = JsonSerializer.Serialize(repos, new JsonSerializerOptions { WriteIndented = true });
await File.WriteAllTextAsync(outputPath, json);
Console.WriteLine($"Wrote {repos.Count} repo(s) to {outputPath}");

return 0;
