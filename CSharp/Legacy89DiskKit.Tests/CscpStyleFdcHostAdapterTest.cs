using Legacy89DiskKit.Application;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class CscpStyleFdcHostAdapterTest
{
    [Fact]
    public void Adapter_CanMountAndReportInsertedDisk()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        var adapter = Legacy89DiskKitApplication.CreateCscpStyleFdcHostAdapter();

        adapter.OpenDisk(0, container);

        Assert.True(adapter.IsDiskInserted(0));
        Assert.True(adapter.IsDriveReady(0));
    }

    [Fact]
    public void Adapter_CanReadSectorThroughRegisterBridge()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42, 0x43 });

        var adapter = Legacy89DiskKitApplication.CreateCscpStyleFdcHostAdapter();
        adapter.OpenDisk(0, container);
        adapter.SelectDrive(0);

        adapter.WriteIo8(1, 0);
        adapter.WriteIo8(2, 1);
        adapter.WriteIo8(0, 0x80);

        Assert.True(adapter.GetVisibleState().Busy);

        adapter.Advance(TimeSpan.FromMilliseconds(1));

        Assert.True(adapter.IsIrqAsserted());
        Assert.True(adapter.IsDrqAsserted());
        Assert.Equal(0x41, adapter.ReadIo8(3));
        Assert.Equal(0x42, adapter.ReadIo8(3));
        Assert.Equal(0x43, adapter.ReadIo8(3));
    }

    [Fact]
    public void Adapter_CanSwitchSideOnSelectedDrive()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 1, 1, new byte[] { 0x99 });

        var adapter = Legacy89DiskKitApplication.CreateCscpStyleFdcHostAdapter();
        adapter.OpenDisk(0, container);
        adapter.SelectDrive(0);
        adapter.SelectSide(1);
        adapter.WriteIo8(1, 0);
        adapter.WriteIo8(2, 1);
        adapter.WriteIo8(0, 0x80);
        adapter.Advance(TimeSpan.FromMilliseconds(1));

        var visible = adapter.GetVisibleState();

        Assert.Equal(0, visible.SelectedDrive);
        Assert.Equal(1, visible.SelectedSide);
        Assert.Equal(0x99, adapter.ReadIo8(3));
    }

    [Fact]
    public void Adapter_CanUnmountDisk()
    {
        using var container = RawDiskContainer.CreateNewInMemory(Domain.DiskImage.Model.DiskType.TwoD);
        var adapter = Legacy89DiskKitApplication.CreateCscpStyleFdcHostAdapter();
        adapter.OpenDisk(1, container);

        var closed = adapter.CloseDisk(1);

        Assert.True(closed);
        Assert.False(adapter.IsDiskInserted(1));
    }

    [Fact]
    public void Adapter_RaisesIrqAndDrqCallbacksWhenSignalStateChanges()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x55 });

        var adapter = Legacy89DiskKitApplication.CreateCscpStyleFdcHostAdapter();
        var irqStates = new List<bool>();
        var drqStates = new List<bool>();
        adapter.IrqChanged += value => irqStates.Add(value);
        adapter.DrqChanged += value => drqStates.Add(value);

        adapter.OpenDisk(0, container);
        adapter.SelectDrive(0);
        adapter.WriteIo8(1, 0);
        adapter.WriteIo8(2, 1);
        adapter.WriteIo8(0, 0x80);
        adapter.Advance(TimeSpan.FromMilliseconds(1));
        adapter.ReadIo8(3);

        Assert.Contains(true, irqStates);
        Assert.Contains(true, drqStates);
        Assert.Contains(false, drqStates);
    }
}
