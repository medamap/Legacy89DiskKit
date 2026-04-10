using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofRealProcessRawComparisonTest
{
    [Fact]
    public async Task RealProcessRawProof_MatchesSecondBaseline()
    {
        using var container = RawDiskContainer.CreateNewInMemory(DiskType.TwoD);
        var sectorData = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();
        sectorData[0] = 0x51;
        sectorData[1] = 0x52;
        container.WriteSector(0, 0, 1, sectorData);

        var outputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var requests = HostProofSequence.CreateReadOnlyRawByBufferSequence(container.ToImageData());
            await using var process = new CliHostProcessSession();
            var transcript = await CliHostProofRunner.RunObservableAsync(process, requests);
            var report = HostProofReportBuilder.Build(transcript, "OpenDiskImage", "observable");

            await HostProofBundleWriter.WriteAsync(outputDirectory, "proof", report, transcript, requests);
            var bundle = await HostProofBundleReader.ReadAsync(outputDirectory, "proof");
            var mismatches = HostProofBundleComparer.Compare(bundle, HostProofExpectationCatalog.EventDrivenSecondProofRaw());

            Assert.Empty(mismatches);
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
