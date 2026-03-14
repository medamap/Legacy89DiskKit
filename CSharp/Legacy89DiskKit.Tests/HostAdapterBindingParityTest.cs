using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.Fdc.Hosts;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostAdapterBindingParityTest
{
    [Fact]
    public void Adapters_CanShareD88MountedMediumBindingPath()
    {
        using var eventDrivenContainer = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        eventDrivenContainer.WriteSector(0, 0, 1, new byte[] { 0x21, 0x22 });

        var eventDrivenAdapter = Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter();
        eventDrivenAdapter.OpenDisk(0, eventDrivenContainer);
        eventDrivenAdapter.SelectDrive(0);
        eventDrivenAdapter.WriteIo8(1, 0);
        eventDrivenAdapter.WriteIo8(2, 1);
        eventDrivenAdapter.WriteIo8(0, 0x80);
        eventDrivenAdapter.Advance(TimeSpan.FromMilliseconds(1));

        using var xmilContainer = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        xmilContainer.WriteSector(0, 0, 1, new byte[] { 0x21, 0x22 });

        var xmilAdapter = Legacy89DiskKitApplication.CreateXmilWebStyleFdcHostAdapter();
        xmilAdapter.MountDisk(0, xmilContainer);
        xmilAdapter.SetDrive(0);
        xmilAdapter.X1FdcW(1, 0);
        xmilAdapter.X1FdcW(2, 1);
        xmilAdapter.X1FdcW(0, 0x80);
        xmilAdapter.RunEvent(XmilWebFdcEventKind.BusyCompletion);

        Assert.Equal((byte)0x21, eventDrivenAdapter.ReadIo8(3));
        Assert.Equal((byte)0x21, xmilAdapter.X1FdcR(3));
    }
}
