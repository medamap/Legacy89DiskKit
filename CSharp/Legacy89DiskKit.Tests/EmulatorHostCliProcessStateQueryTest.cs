using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostCliProcessStateQueryTest
{
    [Fact]
    public async Task CliHostStdioObservable_CanReportSelectedDriveAndBusyState()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 1, 1, new byte[] { 0x91 });

        await using var process = new CliHostProcessSession();
        var transcript = new List<HostProofTranscriptEntry>();

        await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskImage, DriveNumber: 1, ImageFormat: "d88", ImageDataBase64: Convert.ToBase64String(container.ToImageData()), ReadOnly: true), transcript);
        await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 1), transcript);
        await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.SelectSide, Side: 1), transcript);
        await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0), transcript);
        await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1), transcript);
        await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80), transcript);

        var busyState = await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.QueryState), transcript);
        Assert.NotNull(busyState.Response.VisibleState);
        Assert.True(busyState.Response.VisibleState!.Busy);
        Assert.Equal(1, busyState.Response.VisibleState.SelectedDrive);
        Assert.Equal(1, busyState.Response.VisibleState.SelectedSide);

        var completed = await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000), transcript);
        Assert.True(completed.Response.IrqAsserted);

        var readyState = await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.QueryState), transcript);
        Assert.NotNull(readyState.Response.VisibleState);
        Assert.False(readyState.Response.VisibleState!.Busy);
        Assert.True(readyState.Response.IrqAsserted);

        var data = await process.SendExchangeAsync(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3), transcript);
        Assert.Equal((byte?)0x91, data.Response.RegisterValue);
    }
}
