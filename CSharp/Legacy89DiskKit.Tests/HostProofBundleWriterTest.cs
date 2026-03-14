using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofBundleWriterTest
{
    [Fact]
    public async Task BundleWriter_CanWriteReportAndTranscript()
    {
        var report = new HostProofReport(
            OpenMode: "OpenDiskImage",
            ExchangeMode: "observable",
            CapabilityHandshakeSucceeded: true,
            SupportsPathOpen: true,
            SupportsBufferOpen: true,
            SupportsNotificationExchange: true,
            SupportsPlainStdio: false,
            SupportsObservableStdio: true,
            DiskOpenSucceeded: true,
            BusyObserved: true,
            IrqObserved: true,
            DrqObserved: true,
            DataReadSucceeded: true,
            CloseSucceeded: true);

        var transcript = new[]
        {
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    []))
        };

        var outputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var requestScript = new[]
            {
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities)
            };

            await HostProofBundleWriter.WriteAsync(outputDirectory, "proof", report, transcript, requestScript);

            var markdownPath = Path.Combine(outputDirectory, "proof.md");
            var transcriptPath = Path.Combine(outputDirectory, "proof.jsonl");
            var requestPath = Path.Combine(outputDirectory, "proof.requests.jsonl");

            Assert.True(File.Exists(markdownPath));
            Assert.True(File.Exists(transcriptPath));
            Assert.True(File.Exists(requestPath));
            Assert.Contains("Host Proof Report", await File.ReadAllTextAsync(markdownPath));

            var roundTrip = await HostProofTranscriptFileStore.LoadAsync(transcriptPath);
            Assert.Single(roundTrip);
            var roundTripRequests = await HostProofRequestScriptFileStore.LoadAsync(requestPath);
            Assert.Single(roundTripRequests);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}
