using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofRealProcessReportTest
{
    [Fact]
    public async Task RealProcessTranscript_CanProduceMarkdownReport()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        await File.WriteAllBytesAsync(imagePath, container.ToImageData());

        await using var process = new CliHostProcessSession();
        var transcript = new List<HostProofTranscriptEntry>();

        try
        {
            foreach (var request in HostProofSequence.CreateReadOnlyD88ByPathSequence(imagePath))
            {
                await process.SendExchangeAsync(request, transcript);
            }

            var report = HostProofReportBuilder.Build(transcript, "OpenDiskPath", "observable");
            var markdown = HostProofReportMarkdownRenderer.Render(report);

            Assert.Contains("- Disk open succeeded: True", markdown);
            Assert.Contains("- Data read succeeded: True", markdown);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }
}
