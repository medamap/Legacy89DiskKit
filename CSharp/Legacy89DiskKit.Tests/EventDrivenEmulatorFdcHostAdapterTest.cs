using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EventDrivenEmulatorFdcHostAdapterTest
{
    [Fact]
    public void Adapter_CanMountAndReportInsertedDisk()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();

        adapter.OpenDisk(0, container);

        Assert.True(adapter.IsDiskInserted(0));
        Assert.True(adapter.IsDriveReady(0));
    }

    [Fact]
    public void Adapter_CanReadSectorThroughRegisterBridge()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42, 0x43 });

        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
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
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 1, 1, new byte[] { 0x99 });

        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
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
        using var container = RawDiskContainer.CreateNewInMemory(Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(1, container);

        var closed = adapter.CloseDisk(1);

        Assert.True(closed);
        Assert.False(adapter.IsDiskInserted(1));
    }

    [Fact]
    public void Adapter_RaisesIrqAndDrqCallbacksWhenSignalStateChanges()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x55 });

        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
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

    [Fact]
    public void Adapter_RaisesAdvanceRequestedWhenCommandStartsPendingWork()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x10 });

        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
        var requestedDelays = new List<TimeSpan>();
        adapter.AdvanceRequested += delay => requestedDelays.Add(delay);

        adapter.OpenDisk(0, container);
        adapter.SelectDrive(0);
        adapter.WriteIo8(1, 0);
        adapter.WriteIo8(2, 1);
        adapter.WriteIo8(0, 0x80);

        Assert.Contains(TimeSpan.FromMilliseconds(1), requestedDelays);
    }

    [Fact]
    public void Adapter_CanHandleTransportNeutralRequests()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x61, 0x62 });

        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);

        adapter.Handle(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0));
        adapter.Handle(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0));
        adapter.Handle(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1));
        var commandResponse = adapter.Handle(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80));

        Assert.Equal(1000, commandResponse.PendingAdvanceMicroseconds);
        Assert.True(commandResponse.VisibleState?.Busy);

        var advanced = adapter.Handle(new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000));

        Assert.True(advanced.IrqAsserted);
        Assert.True(advanced.DrqAsserted);

        var firstByte = adapter.Handle(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3));
        var secondByte = adapter.Handle(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3));

        Assert.Equal((byte?)0x61, firstByte.RegisterValue);
        Assert.Equal((byte?)0x62, secondByte.RegisterValue);
    }

    [Fact]
    public void Adapter_CanOpenDiskFromPath()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x71, 0x72 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        File.WriteAllBytes(imagePath, container.ToImageData());

        try
        {
            var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
            adapter.OpenDiskPath(0, imagePath);
            adapter.SelectDrive(0);
            adapter.WriteIo8(1, 0);
            adapter.WriteIo8(2, 1);
            adapter.WriteIo8(0, 0x80);
            adapter.Advance(TimeSpan.FromMilliseconds(1));

            Assert.Equal(0x71, adapter.ReadIo8(3));
            Assert.Equal(0x72, adapter.ReadIo8(3));
        }
        finally
        {
            File.Delete(imagePath);
        }
    }

    [Fact]
    public void Adapter_CanOpenDiskFromBuffer()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x81, 0x82 });

        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDiskImage(0, container.ToImageData(), "d88");
        adapter.SelectDrive(0);
        adapter.WriteIo8(1, 0);
        adapter.WriteIo8(2, 1);
        adapter.WriteIo8(0, 0x80);
        adapter.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Equal(0x81, adapter.ReadIo8(3));
        Assert.Equal(0x82, adapter.ReadIo8(3));
    }

    private static Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter CreateEventDrivenEmulatorFdcHostAdapter()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter(
            new Legacy89DiskKit.Drive.Application.DriveMountService(),
            new Legacy89DiskKit.Drive.Application.MountedMediumBindingService(),
            new Legacy89DiskKit.DiskImage.Infrastructure.Factory.DiskContainerFactory());
    }
}
