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
        HostProofAssert.AssertCapabilityHandshake(capabilities, expectObservable: true, expectPathOpen: true, expectBufferOpen: true);

        await process.SendExchangeAsync(sequence[1], transcript);
        await process.SendExchangeAsync(sequence[2], transcript);
        await process.SendExchangeAsync(sequence[3], transcript);
        await process.SendExchangeAsync(sequence[4], transcript);

        var commandExchange = await process.SendExchangeAsync(sequence[5], transcript);
        HostProofAssert.AssertAdvanceRequested(commandExchange);

        var advanceExchange = await process.SendExchangeAsync(sequence[6], transcript);
        Assert.True(advanceExchange.Response.IrqAsserted);
        Assert.True(advanceExchange.Response.DrqAsserted);

        await process.SendExchangeAsync(sequence[7], transcript);
        await process.SendExchangeAsync(sequence[8], transcript);
        HostProofAssert.AssertReadRegisterValues(transcript, 0x51, 0x52);
        HostProofAssert.AssertTranscriptRoundTrip(transcript, 9);
        var report = HostProofReportBuilder.Build(transcript, "OpenDiskImage", "observable");
        Assert.True(report.SupportsBufferOpen);
        Assert.True(report.DiskOpenSucceeded);
        Assert.True(report.DataReadSucceeded);
    }
}
