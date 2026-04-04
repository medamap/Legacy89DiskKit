using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.Fdc.Application.Hosts.Scripting;
using Legacy89DiskKit.Domain.Fdc.Model;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostBundleWriterTest
{
    [Fact]
    public async Task WriteAsync_WritesManifestTranscriptAndRequestScript()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var transcript = new[]
            {
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
                    new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
                    new EmulatorHostExchange(
                        new EmulatorHostResponse(
                            RegisterValue: 0x41,
                            VisibleState: new FdcVisibleState(0, 0, 1, 0x41, 0, 0, false, true, true),
                            IrqAsserted: true,
                            DrqAsserted: true,
                            PendingAdvanceMicroseconds: null),
                        [])),
            };

            var report = EmulatorHostProofReportBuilder.Build(transcript, "OpenDiskPath", "observable");
            var requestScript = new[]
            {
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
            };

            await EmulatorHostBundleWriter.WriteAsync(outputDirectory, "proof", report, transcript, requestScript);

            var bundle = await EmulatorHostBundleReader.ReadAsync(outputDirectory, "proof");

            Assert.Equal("proof", bundle.Manifest.BaseName);
            Assert.Equal("OpenDiskPath", bundle.Manifest.OpenMode);
            Assert.Equal("observable", bundle.Manifest.ExchangeMode);
            Assert.Equal(2, bundle.Transcript.Count);
            Assert.Equal(2, bundle.RequestScript.Count);
            Assert.Contains("Host Proof Report", bundle.MarkdownReport);
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
