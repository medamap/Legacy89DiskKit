using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostCliProcessRawBridgeTest
{
    [Fact]
    public async Task CliHostStdioObservable_CanServeReadOnlyRawBufferFlow()
    {
        using var container = RawDiskContainer.CreateNewInMemory(DiskType.TwoD);
        var sectorData = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();
        sectorData[0] = 0x51;
        sectorData[1] = 0x52;
        container.WriteSector(0, 0, 1, sectorData);

        await using var process = new CliHostProcessSession();
        var transcript = new List<HostProofTranscriptEntry>();
        var sequence = HostProofSequence.CreateReadOnlyRawByBufferSequence(container.ToImageData());

        var capabilities = await process.SendExchangeAsync(sequence[0], transcript);
        Assert.NotNull(capabilities.Response.Capabilities);
        Assert.True(capabilities.Response.Capabilities!.SupportsBufferOpen);

        await process.SendExchangeAsync(sequence[1], transcript);
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

        Assert.Equal((byte?)0x51, firstByte.Response.RegisterValue);
        Assert.Equal((byte?)0x52, secondByte.Response.RegisterValue);

        var transcriptPayload = HostProofTranscriptCodec.SerializeLines(transcript);
        var roundTrip = HostProofTranscriptCodec.DeserializeLines(transcriptPayload);
        Assert.Equal(transcriptPayload, HostProofTranscriptCodec.SerializeLines(roundTrip));
        Assert.Equal(9, roundTrip.Count);
    }
}
