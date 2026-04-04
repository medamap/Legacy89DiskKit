using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class CliHostProofRunnerTest
{
    [Fact]
    public async Task Runner_CanExecuteObservableSequence()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        await File.WriteAllBytesAsync(imagePath, container.ToImageData());

        try
        {
            await using var process = new CliHostProcessSession();
            var transcript = await CliHostProofRunner.RunObservableAsync(
                process,
                HostProofSequence.CreateReadOnlyD88ByPathSequence(imagePath));

            HostProofAssert.AssertReadRegisterValues(transcript, 0x41, 0x42);
            HostProofAssert.AssertTranscriptRoundTrip(transcript, 10);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }
}
