using System.Net;
using System.Text;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection(nameof(UpdateHttpListenerCollection))]
public sealed class CliCheckUpdateTest
{
    [Fact]
    public async Task CheckUpdate_GlobalOption_DisplaysLatestRelease()
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

        var result = await CliCommandRunner.RunAsync(
            ["--check-update", "--language", "en"],
            new Dictionary<string, string?>
            {
                ["LEGACY89_UPDATE_API_URL"] = prefix,
                ["LEGACY89_UPDATE_CURRENT_VERSION"] = "2.1.0"
            });

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Current version: 2.1.0", result.StandardOutput);
        Assert.Contains("Latest version: 2.1.1", result.StandardOutput);
        Assert.Contains("Windows MSI:", result.StandardOutput);
        Assert.Contains("An update is available.", result.StandardOutput);

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
