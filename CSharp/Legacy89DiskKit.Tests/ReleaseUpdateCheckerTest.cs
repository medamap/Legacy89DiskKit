using System.Net;
using System.Text;
using Legacy89DiskKit.Cli;
using Xunit;

namespace Legacy89DiskKit.Tests;

public sealed class ReleaseUpdateCheckerTest
{
    [Fact]
    public async Task CheckAsync_ReturnsLatestVersionAndWindowsMsi()
    {
        using var listener = new HttpListener();
        var prefix = $"http://127.0.0.1:{GetAvailablePort()}/";
        listener.Prefixes.Add(prefix);
        listener.Start();

        var serveTask = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            var payload = """
            {
              "tag_name": "v2.1.1",
              "html_url": "https://example.test/releases/v2.1.1",
              "assets": [
                {
                  "name": "Legacy89DiskKit.Cli-v2.1.1-win-x64.msi",
                  "browser_download_url": "https://example.test/releases/download/v2.1.1/Legacy89DiskKit.Cli-v2.1.1-win-x64.msi"
                }
              ]
            }
            """;
            var bytes = Encoding.UTF8.GetBytes(payload);
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = bytes.Length;
            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        });

        var checker = new ReleaseUpdateChecker(
            httpClient: new HttpClient(),
            latestReleaseApiUrl: prefix,
            currentVersion: "2.1.0");

        var result = await checker.CheckAsync();

        Assert.Equal("2.1.0", result.CurrentVersion);
        Assert.Equal("2.1.1", result.LatestVersion);
        Assert.True(result.IsUpdateAvailable);
        Assert.Equal("https://example.test/releases/v2.1.1", result.ReleaseUrl);
        Assert.Equal("https://example.test/releases/download/v2.1.1/Legacy89DiskKit.Cli-v2.1.1-win-x64.msi", result.WindowsMsiUrl);

        await serveTask;
        listener.Stop();
    }

    private static int GetAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
