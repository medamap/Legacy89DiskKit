using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace Legacy89DiskKit.Cli;

public sealed record ReleaseUpdateInfo(
    string CurrentVersion,
    string? LatestVersion,
    string? ReleaseUrl,
    string? WindowsMsiUrl,
    bool IsUpdateAvailable);

public sealed class ReleaseUpdateChecker
{
    private const string DefaultLatestReleaseApiUrl = "https://api.github.com/repos/medamap/Legacy89DiskKit/releases/latest";

    private readonly HttpClient httpClient;
    private readonly string latestReleaseApiUrl;
    private readonly string currentVersion;

    public ReleaseUpdateChecker(HttpClient? httpClient = null, string? latestReleaseApiUrl = null, string? currentVersion = null)
    {
        this.httpClient = httpClient ?? new HttpClient();
        this.latestReleaseApiUrl = latestReleaseApiUrl
            ?? Environment.GetEnvironmentVariable("LEGACY89_UPDATE_API_URL")
            ?? DefaultLatestReleaseApiUrl;
        this.currentVersion = NormalizeVersion(currentVersion
            ?? Environment.GetEnvironmentVariable("LEGACY89_UPDATE_CURRENT_VERSION")
            ?? ResolveCurrentVersion());

        if (!this.httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            this.httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Legacy89DiskKit", this.currentVersion));
        }
    }

    public async Task<ReleaseUpdateInfo> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(latestReleaseApiUrl, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new ReleaseUpdateInfo(currentVersion, null, null, null, false);
        }

        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        var latestTag = root.TryGetProperty("tag_name", out var tagNameElement) ? tagNameElement.GetString() : null;
        var latestVersion = NormalizeVersion(latestTag);
        var releaseUrl = root.TryGetProperty("html_url", out var htmlUrlElement) ? htmlUrlElement.GetString() : null;
        var windowsMsiUrl = TryFindWindowsMsiUrl(root);

        var current = ParseVersion(currentVersion);
        var latest = ParseVersion(latestVersion);
        var isUpdateAvailable = current != null && latest != null && latest > current;

        return new ReleaseUpdateInfo(currentVersion, latestVersion, releaseUrl, windowsMsiUrl, isUpdateAvailable);
    }

    private static string ResolveCurrentVersion()
    {
        var informationalVersion = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion;
        }

        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        return string.IsNullOrWhiteSpace(assemblyVersion) ? "0.0.0" : assemblyVersion;
    }

    private static string? TryFindWindowsMsiUrl(JsonElement root)
    {
        if (!root.TryGetProperty("assets", out var assetsElement) || assetsElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var asset in assetsElement.EnumerateArray())
        {
            var name = asset.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
            if (string.IsNullOrWhiteSpace(name) || !name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return asset.TryGetProperty("browser_download_url", out var urlElement)
                ? urlElement.GetString()
                : null;
        }

        return null;
    }

    private static string NormalizeVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "0.0.0";
        }

        var normalized = value.Trim();
        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        var separatorIndex = normalized.IndexOfAny(['-', '+']);
        if (separatorIndex >= 0)
        {
            normalized = normalized[..separatorIndex];
        }

        return normalized;
    }

    private static Version? ParseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Version.TryParse(NormalizeVersion(value), out var version) ? version : null;
    }
}
