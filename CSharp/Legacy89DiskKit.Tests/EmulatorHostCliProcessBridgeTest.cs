using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
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
            HostProofAssert.AssertCapabilityHandshake(capabilities, expectObservable: true, expectPathOpen: true, expectBufferOpen: true);

            var openExchange = await process.SendExchangeAsync(sequence[1], transcript);
            Assert.NotNull(openExchange.Response.VisibleState);

            await process.SendExchangeAsync(sequence[2], transcript);
            await process.SendExchangeAsync(sequence[3], transcript);
            await process.SendExchangeAsync(sequence[4], transcript);

            var commandExchange = await process.SendExchangeAsync(sequence[5], transcript);
            HostProofAssert.AssertAdvanceRequested(commandExchange);

            var advanceExchange = await process.SendExchangeAsync(sequence[6], transcript);
            Assert.True(advanceExchange.Response.IrqAsserted);
            Assert.True(advanceExchange.Response.DrqAsserted);

            var firstByte = await process.SendExchangeAsync(sequence[7], transcript);
            var secondByte = await process.SendExchangeAsync(sequence[8], transcript);

            HostProofAssert.AssertReadRegisterValues(transcript, 0x41, 0x42);

            var closeExchange = await process.SendExchangeAsync(sequence[9], transcript);
            Assert.Null(closeExchange.Response.VisibleState);

            HostProofAssert.AssertTranscriptRoundTrip(transcript, 10);
            var report = HostProofReportBuilder.Build(transcript, "OpenDiskPath", "observable");
            Assert.True(report.CapabilityHandshakeSucceeded);
            Assert.True(report.DiskOpenSucceeded);
            Assert.True(report.DataReadSucceeded);
            Assert.True(report.CloseSucceeded);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }
}
