namespace console_app.Models;

/// <summary>
/// Normalized repository data for console output and JSON export (consumed by React app).
/// </summary>
public sealed class RepositorySummary
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stars { get; init; }
    public string PrimaryLanguage { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string LastUpdated { get; init; } = string.Empty;
}
