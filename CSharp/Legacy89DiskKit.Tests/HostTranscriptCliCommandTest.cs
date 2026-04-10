using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.Fdc.Application.Hosts.Scripting;
using Legacy89DiskKit.Domain.Fdc.Model;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostTranscriptCliCommandTest
{
    [Fact]
    public async Task HostTranscriptInspect_PrintsProofSummary()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var transcriptPath = Path.Combine(workingDirectory, "proof.transcript.jsonl");
            await EmulatorHostTranscriptFileStore.SaveAsync(transcriptPath, CreateEventDrivenD88Transcript());

            var result = await CliCommandRunner.RunAsync(
                "host",
                "transcript",
                "inspect",
                transcriptPath,
                "--open-mode",
                "OpenDiskPath",
                "--exchange-mode",
                "observable");

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("TranscriptEntries: 4", result.StandardOutput);
            Assert.Contains("SupportsNotificationExchange: True", result.StandardOutput);
            Assert.Contains("SupportsObservableStdio: True", result.StandardOutput);
            Assert.Contains("DataReadSucceeded: True", result.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task HostTranscriptReport_WritesMarkdownFile()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var transcriptPath = Path.Combine(workingDirectory, "proof.transcript.jsonl");
            var reportPath = Path.Combine(workingDirectory, "proof.md");
            await EmulatorHostTranscriptFileStore.SaveAsync(transcriptPath, CreateEventDrivenD88Transcript());

            var result = await CliCommandRunner.RunAsync(
                "host",
                "transcript",
                "report",
                transcriptPath,
                reportPath,
                "--open-mode",
                "OpenDiskPath",
                "--exchange-mode",
                "observable");

            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(reportPath));
            var markdown = await File.ReadAllTextAsync(reportPath);
            Assert.Contains("# Host Proof Report", markdown);
            Assert.Contains("Data read succeeded: True", markdown);
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task HostTranscriptVerify_MatchesEventDrivenBaseline()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var transcriptPath = Path.Combine(workingDirectory, "proof.transcript.jsonl");
            await EmulatorHostTranscriptFileStore.SaveAsync(transcriptPath, CreateEventDrivenD88Transcript());

            var result = await CliCommandRunner.RunAsync(
                "host",
                "transcript",
                "verify",
                transcriptPath,
                "event-d88",
                "--open-mode",
                "OpenDiskPath",
                "--exchange-mode",
                "observable");

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Host-proof transcript matched baseline: event-d88", result.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task HostTranscriptVerify_ReturnsNonZeroForUnsupportedBaseline()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var transcriptPath = Path.Combine(workingDirectory, "proof.transcript.jsonl");
            await EmulatorHostTranscriptFileStore.SaveAsync(transcriptPath, CreateEventDrivenD88Transcript());

            var result = await CliCommandRunner.RunAsync(
                "host",
                "transcript",
                "verify",
                transcriptPath,
                "bad-baseline",
                "--open-mode",
                "OpenDiskPath",
                "--exchange-mode",
                "observable");

            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("Unsupported host baseline: bad-baseline", result.StandardError);
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
            }
        }
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
}
