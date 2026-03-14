using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostCliProcessBridgeTest
{
    [Fact]
    public async Task CliHostStdioObservable_CanServeReadOnlyD88Flow()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        await File.WriteAllBytesAsync(imagePath, container.ToImageData());

        await using var process = new CliHostProcessSession();
        var transcript = new List<HostProofTranscriptEntry>();

        try
        {
            var sequence = HostProofSequence.CreateReadOnlyD88ByPathSequence(imagePath);

            var capabilities = await process.SendExchangeAsync(sequence[0], transcript);
            Assert.NotNull(capabilities.Response.Capabilities);
            Assert.True(capabilities.Response.Capabilities!.SupportsObservableStdio);
            Assert.True(capabilities.Response.Capabilities.SupportsPathOpen);

            var openExchange = await process.SendExchangeAsync(sequence[1], transcript);
            Assert.NotNull(openExchange.Response.VisibleState);

            await process.SendExchangeAsync(sequence[2], transcript);
            await process.SendExchangeAsync(sequence[3], transcript);
            await process.SendExchangeAsync(sequence[4], transcript);

            var commandExchange = await process.SendExchangeAsync(sequence[5], transcript);
            Assert.Contains(commandExchange.Notifications, x => x.Kind == EmulatorHostNotificationKind.AdvanceRequested);

            var advanceExchange = await process.SendExchangeAsync(sequence[6], transcript);
            Assert.True(advanceExchange.Response.IrqAsserted);
            Assert.True(advanceExchange.Response.DrqAsserted);

            var firstByte = await process.SendExchangeAsync(sequence[7], transcript);
            var secondByte = await process.SendExchangeAsync(sequence[8], transcript);

            Assert.Equal((byte?)0x41, firstByte.Response.RegisterValue);
            Assert.Equal((byte?)0x42, secondByte.Response.RegisterValue);

            var closeExchange = await process.SendExchangeAsync(sequence[9], transcript);
            Assert.Null(closeExchange.Response.VisibleState);

            var transcriptPayload = HostProofTranscriptCodec.SerializeLines(transcript);
            var roundTrip = HostProofTranscriptCodec.DeserializeLines(transcriptPayload);
            Assert.Equal(transcriptPayload, HostProofTranscriptCodec.SerializeLines(roundTrip));
            Assert.Equal(10, roundTrip.Count);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }
}
