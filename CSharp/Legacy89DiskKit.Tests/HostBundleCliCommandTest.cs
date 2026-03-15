using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Application.Fdc.Hosts.Scripting;
using Legacy89DiskKit.Domain.Fdc.Model;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostBundleCliCommandTest
{
    [Fact]
    public async Task HostBundleInspect_PrintsProofSummary()
    {
        var bundleDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(bundleDirectory);

        try
        {
            await WriteEventDrivenD88BundleAsync(bundleDirectory, "proof");

            var result = await CliCommandRunner.RunAsync("host", "bundle", "inspect", bundleDirectory, "proof");

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("CapabilityHandshakeSucceeded: True", result.StandardOutput);
            Assert.Contains("SupportsNotificationExchange: True", result.StandardOutput);
            Assert.Contains("DataReadSucceeded: True", result.StandardOutput);
            Assert.Contains("CloseSucceeded: True", result.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(bundleDirectory))
            {
                Directory.Delete(bundleDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task HostBundleVerify_MatchesEventDrivenBaseline()
    {
        var bundleDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(bundleDirectory);

        try
        {
            await WriteEventDrivenD88BundleAsync(bundleDirectory, "proof");

            var result = await CliCommandRunner.RunAsync("host", "bundle", "verify", bundleDirectory, "proof", "event-d88");

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Host-proof bundle matched baseline: event-d88", result.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(bundleDirectory))
            {
                Directory.Delete(bundleDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task HostBundleVerify_ReturnsNonZeroForUnsupportedBaseline()
    {
        var bundleDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(bundleDirectory);

        try
        {
            await WriteEventDrivenD88BundleAsync(bundleDirectory, "proof");

            var result = await CliCommandRunner.RunAsync("host", "bundle", "verify", bundleDirectory, "proof", "bad-baseline");

            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("Unsupported host baseline: bad-baseline", result.StandardError);
        }
        finally
        {
            if (Directory.Exists(bundleDirectory))
            {
                Directory.Delete(bundleDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task HostBundlePack_WritesBundleFromTranscriptAndRequestScript()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var transcriptPath = Path.Combine(workingDirectory, "proof.transcript.jsonl");
            var requestScriptPath = Path.Combine(workingDirectory, "proof.requests.jsonl");
            var bundleDirectory = Path.Combine(workingDirectory, "bundle");

            var transcript = CreateEventDrivenD88Transcript();
            var requests = CreateEventDrivenD88RequestScript();

            await EmulatorHostTranscriptFileStore.SaveAsync(transcriptPath, transcript);
            await EmulatorHostRequestScriptFileStore.SaveAsync(requestScriptPath, requests);

            var result = await CliCommandRunner.RunAsync(
                "host",
                "bundle",
                "pack",
                transcriptPath,
                bundleDirectory,
                "proof",
                "--request-script",
                requestScriptPath,
                "--open-mode",
                "OpenDiskPath",
                "--exchange-mode",
                "observable");

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Host-proof bundle written", result.StandardOutput);
            Assert.True(File.Exists(Path.Combine(bundleDirectory, "proof.manifest.json")));
            Assert.True(File.Exists(Path.Combine(bundleDirectory, "proof.md")));
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
            }
        }
    }

    private static async Task WriteEventDrivenD88BundleAsync(string bundleDirectory, string baseName)
    {
        var manifest = new EmulatorHostBundleManifest(
            BaseName: baseName,
            ReportFileName: $"{baseName}.md",
            TranscriptFileName: $"{baseName}.transcript.jsonl",
            RequestScriptFileName: $"{baseName}.requests.jsonl",
            OpenMode: "OpenDiskPath",
            ExchangeMode: "observable");

        var transcript = CreateEventDrivenD88Transcript();
        var requests = CreateEventDrivenD88RequestScript();

        await File.WriteAllTextAsync(
            Path.Combine(bundleDirectory, manifest.ReportFileName),
            "# proof");
        await File.WriteAllTextAsync(
            Path.Combine(bundleDirectory, $"{baseName}.manifest.json"),
            EmulatorHostBundleManifestCodec.Serialize(manifest));
        await EmulatorHostTranscriptFileStore.SaveAsync(
            Path.Combine(bundleDirectory, manifest.TranscriptFileName),
            transcript);
        await EmulatorHostRequestScriptFileStore.SaveAsync(
            Path.Combine(bundleDirectory, manifest.RequestScriptFileName!),
            requests);
    }

    private static IReadOnlyList<EmulatorHostTranscriptEntry> CreateEventDrivenD88Transcript()
    {
        return
        [
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null,
                        Capabilities: new EmulatorHostCapabilities(1, true, true, true, true, true)),
                    [])),
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskPath, ImagePath: "/tmp/test.d88"),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: new FdcVisibleState(0, 0, 1, 0, 0, 0, true, false, false),
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: 1000),
                    [])),
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: 0x41,
                        VisibleState: new FdcVisibleState(0, 0, 1, 0x41, 0, 0, false, true, true),
                        IrqAsserted: true,
                        DrqAsserted: true,
                        PendingAdvanceMicroseconds: null),
                    [])),
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.CloseDisk),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    [])),
        ];
    }

    private static IReadOnlyList<EmulatorHostRequest> CreateEventDrivenD88RequestScript()
    {
        return
        [
            new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
            new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskPath, ImagePath: "/tmp/test.d88"),
            new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
            new EmulatorHostRequest(EmulatorHostRequestKind.CloseDisk),
        ];
    }
}
