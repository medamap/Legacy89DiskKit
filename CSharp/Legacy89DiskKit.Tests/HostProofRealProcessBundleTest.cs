using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofRealProcessBundleTest
{
    [Fact]
    public async Task RealProcessProof_CanWritePortableBundle()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        var outputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        await File.WriteAllBytesAsync(imagePath, container.ToImageData());

        try
        {
            var requests = HostProofSequence.CreateReadOnlyD88ByPathSequence(imagePath);
            await using var process = new CliHostProcessSession();
            var transcript = await CliHostProofRunner.RunObservableAsync(process, requests);
            var report = HostProofReportBuilder.Build(transcript, "OpenDiskPath", "observable");

            await HostProofBundleWriter.WriteAsync(outputDirectory, "proof", report, transcript, requests);

            Assert.True(File.Exists(Path.Combine(outputDirectory, "proof.md")));
            Assert.True(File.Exists(Path.Combine(outputDirectory, "proof.jsonl")));
            Assert.True(File.Exists(Path.Combine(outputDirectory, "proof.requests.jsonl")));
        }
        finally
        {
            File.Delete(imagePath);
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}
