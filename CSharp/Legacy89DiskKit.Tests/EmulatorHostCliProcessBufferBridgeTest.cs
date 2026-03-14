using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostCliProcessBufferBridgeTest
{
    [Fact]
    public async Task CliHostStdioObservable_CanServeReadOnlyD88BufferFlow()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x31, 0x32 });

        await using var process = new CliHostProcessSession();
        var transcript = new List<HostProofTranscriptEntry>();
        var sequence = HostProofSequence.CreateReadOnlyD88ByBufferSequence(container.ToImageData());

        var capabilities = await process.SendExchangeAsync(sequence[0], transcript);
        Assert.NotNull(capabilities.Response.Capabilities);
        Assert.True(capabilities.Response.Capabilities!.SupportsBufferOpen);
        Assert.True(capabilities.Response.Capabilities.SupportsObservableStdio);

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
        Assert.Equal((byte?)0x31, firstByte.Response.RegisterValue);

        var transcriptPayload = HostProofTranscriptCodec.SerializeLines(transcript);
        var roundTrip = HostProofTranscriptCodec.DeserializeLines(transcriptPayload);
        Assert.Equal(transcriptPayload, HostProofTranscriptCodec.SerializeLines(roundTrip));
        Assert.Equal(8, roundTrip.Count);
    }
}
