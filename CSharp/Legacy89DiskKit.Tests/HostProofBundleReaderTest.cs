using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofBundleReaderTest
{
    [Fact]
    public async Task Reader_CanReadBundleDirectory()
    {
        var report = new HostProofReport(
            OpenMode: "OpenDiskPath",
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

        var requests = new[]
        {
            new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities)
        };

        var outputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            await HostProofBundleWriter.WriteAsync(outputDirectory, "proof", report, transcript, requests);
            var bundle = await HostProofBundleReader.ReadAsync(outputDirectory, "proof");

            Assert.Equal("proof", bundle.Manifest.BaseName);
            Assert.Contains("Host Proof Report", bundle.MarkdownReport);
            Assert.Single(bundle.Transcript);
            Assert.Single(bundle.RequestScript);
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
