using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.Fdc.Application.Hosts.Scripting;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostScriptCliCommandTest
{
    [Fact]
    public async Task HostScriptInspect_PrintsRequestSequenceSummary()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var scriptPath = Path.Combine(workingDirectory, "proof.requests.jsonl");
            var requests = new[]
            {
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskPath, ImagePath: "/tmp/test.d88"),
                new EmulatorHostRequest(EmulatorHostRequestKind.CloseDisk),
            };

            await EmulatorHostRequestScriptFileStore.SaveAsync(scriptPath, requests);

            var result = await CliCommandRunner.RunAsync("host", "script", "inspect", scriptPath);

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("RequestEntries: 3", result.StandardOutput);
            Assert.Contains("FirstKind: QueryCapabilities", result.StandardOutput);
            Assert.Contains("LastKind: CloseDisk", result.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
            }
        }
    }
}
