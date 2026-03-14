using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostCliProcessPlainBridgeTest
{
    [Fact]
    public async Task CliHostStdio_CanServeReadOnlyD88Flow()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x21, 0x22 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        await File.WriteAllBytesAsync(imagePath, container.ToImageData());

        await using var process = new CliHostProcessSession(observable: false);

        try
        {
            var sequence = HostProofSequence.CreateReadOnlyD88ByPathSequence(imagePath);

            var capabilities = await process.SendResponseAsync(sequence[0]);
            Assert.NotNull(capabilities.Capabilities);
            Assert.False(capabilities.Capabilities!.SupportsObservableStdio);
            Assert.True(capabilities.Capabilities.SupportsPlainStdio);
            Assert.True(capabilities.Capabilities.SupportsPathOpen);

            var openResponse = await process.SendResponseAsync(sequence[1]);
            Assert.NotNull(openResponse.VisibleState);

            await process.SendResponseAsync(sequence[2]);
            await process.SendResponseAsync(sequence[3]);
            await process.SendResponseAsync(sequence[4]);
            await process.SendResponseAsync(sequence[5]);

            var advanceResponse = await process.SendResponseAsync(sequence[6]);
            Assert.True(advanceResponse.IrqAsserted);
            Assert.True(advanceResponse.DrqAsserted);

            var firstByte = await process.SendResponseAsync(sequence[7]);
            var secondByte = await process.SendResponseAsync(sequence[8]);
            Assert.Equal((byte?)0x21, firstByte.RegisterValue);
            Assert.Equal((byte?)0x22, secondByte.RegisterValue);

            var closeResponse = await process.SendResponseAsync(sequence[9]);
            Assert.Null(closeResponse.VisibleState);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }
}
