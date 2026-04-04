using Legacy89DiskKit.Fdc.Application.Hosts;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class XmilWebStyleFdcHostAdapterTest
{
    [Fact]
    public void Adapter_CanExposeGlobalStateSnapshot()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        var adapter = CreateXmilWebStyleFdcHostAdapter();

        adapter.MountDisk(0, container);
        adapter.SetDrive(0);
        adapter.SetSide(1);

        var state = adapter.GetState();

        Assert.True(state.DiskInserted);
        Assert.True(state.DriveReady);
        Assert.Equal(0, state.SelectedDrive);
        Assert.Equal(1, state.SelectedSide);
    }

    [Fact]
    public void Adapter_CanReadSectorThroughX1StyleEntrypoints()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x31, 0x32 });

        var adapter = CreateXmilWebStyleFdcHostAdapter();
        adapter.MountDisk(0, container);
        adapter.SetDrive(0);
        adapter.X1FdcW(1, 0);
        adapter.X1FdcW(2, 1);
        adapter.X1FdcW(0, 0x80);

        Assert.True(adapter.GetState().Busy);

        adapter.Advance(TimeSpan.FromMilliseconds(1));

        Assert.True(adapter.GetState().Irq);
        Assert.True(adapter.GetState().Drq);
        Assert.Equal((byte)0x31, adapter.X1FdcR(3));
        Assert.Equal((byte)0x32, adapter.X1FdcR(3));
    }

    [Fact]
    public void Adapter_CanRaiseGlobalSignalEvents()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x77 });

        var adapter = CreateXmilWebStyleFdcHostAdapter();
        var irqStates = new List<bool>();
        var drqStates = new List<bool>();
        adapter.IrqChanged += value => irqStates.Add(value);
        adapter.DrqChanged += value => drqStates.Add(value);

        adapter.MountDisk(0, container);
        adapter.SetDrive(0);
        adapter.X1FdcW(1, 0);
        adapter.X1FdcW(2, 1);
        adapter.X1FdcW(0, 0x80);
        adapter.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Contains(true, irqStates);
        Assert.Contains(true, drqStates);
    }

    [Fact]
    public void Adapter_CanScheduleAndRunEventObjects()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x40 });

        var adapter = CreateXmilWebStyleFdcHostAdapter();
        var scheduled = new List<(XmilWebFdcEventKind Kind, TimeSpan Delay)>();
        adapter.EventScheduled += (kind, delay) => scheduled.Add((kind, delay));

        adapter.MountDisk(0, container);
        adapter.SetDrive(0);
        adapter.X1FdcW(1, 0);
        adapter.X1FdcW(2, 1);
        adapter.X1FdcW(0, 0x80);

        Assert.Contains((XmilWebFdcEventKind.BusyCompletion, TimeSpan.FromMilliseconds(1)), scheduled);
        Assert.True(adapter.RunEvent(XmilWebFdcEventKind.BusyCompletion));
        Assert.True(adapter.GetState().Irq);
        Assert.True(adapter.GetState().Drq);
    }

    [Fact]
    public void Adapter_CanProveRawSectorImageBackedIntegration()
    {
        using var container = RawDiskContainer.CreateNewInMemory(Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        var sectorData = new byte[256];
        sectorData[0] = 0x51;
        sectorData[1] = 0x52;
        container.WriteSector(0, 0, 1, sectorData);

        var adapter = CreateXmilWebStyleFdcHostAdapter();
        adapter.MountDisk(0, container);
        adapter.SetDrive(0);
        adapter.X1FdcW(1, 0);
        adapter.X1FdcW(2, 1);
        adapter.X1FdcW(0, 0x80);
        adapter.RunEvent(XmilWebFdcEventKind.BusyCompletion);

        Assert.True(adapter.GetState().Irq);
        Assert.True(adapter.GetState().Drq);
        Assert.Equal((byte)0x51, adapter.X1FdcR(3));
        Assert.Equal((byte)0x52, adapter.X1FdcR(3));
    }

    private static XmilWebStyleFdcHostAdapter CreateXmilWebStyleFdcHostAdapter()
    {
        return new XmilWebStyleFdcHostAdapter(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    private static Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter CreateEventDrivenEmulatorFdcHostAdapter()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter(
            new Legacy89DiskKit.Drive.Application.DriveMountService(),
            new Legacy89DiskKit.Drive.Application.MountedMediumBindingService(),
            new Legacy89DiskKit.DiskImage.Infrastructure.Factory.DiskContainerFactory());
    }
}
