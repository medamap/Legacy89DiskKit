using Legacy89DiskKit.Fdc.Domain.Interface;
using Legacy89DiskKit.Fdc.Domain.Model;
using Legacy89DiskKit.Drive.Domain.Interface;
using Legacy89DiskKit.Fdc.Infrastructure;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.Drive.Infrastructure.Medium;
using Legacy89DiskKit.Fdc.Infrastructure.Medium;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class D88BackedMediumTest
{
    [Fact]
    public void D88BackedSectorAddressableMedium_CanReadDecodedSectorData()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0xAB, 0xCD, 0xEF });

        ISectorAddressableMedium medium = new D88BackedSectorAddressableMedium(container);

        var data = medium.ReadSector(0, 0, 1);

        Assert.Equal("d88-family", medium.MediumKind);
        Assert.True(medium.SectorExists(0, 0, 1));
        Assert.Equal(new byte[] { 0xAB, 0xCD, 0xEF }, data);
    }

    [Fact]
    public void D88BackedControllerFacingMedium_CanServeReadSectorLikeFlow()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x5A, 0x6B, 0x7C });

        IControllerFacingMedium medium = new D88BackedControllerFacingMedium(container);

        medium.Reset();
        medium.SelectSide(0);
        medium.WriteTrackRegister(0);
        medium.WriteSectorRegister(1);
        medium.WriteCommand(0x80);

        Assert.True(medium.IsBusy);
        Assert.Equal((byte)FdcStatusFlags.Busy, medium.ReadStatus());
        Assert.False(medium.IsIrqAsserted);
        Assert.False(medium.IsDrqAsserted);

        medium.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Equal("d88-family", medium.MediumKind);
        Assert.True(medium.IsReady);
        Assert.False(medium.IsWriteProtected);
        Assert.False(medium.IsBusy);
        Assert.True(medium.IsIrqAsserted);
        Assert.True(medium.IsDrqAsserted);
        Assert.Equal(0x5A, medium.ReadDataRegister());
        Assert.True(medium.IsDrqAsserted);
        Assert.Equal(0x6B, medium.ReadDataRegister());
        Assert.True(medium.IsDrqAsserted);
        Assert.Equal(0x7C, medium.ReadDataRegister());
        Assert.False(medium.IsDrqAsserted);
    }

    [Fact]
    public void D88BackedControllerFacingMedium_ReturnsRecordNotFoundStatusForMissingSector()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        IControllerFacingMedium medium = new D88BackedControllerFacingMedium(container);

        medium.Reset();
        medium.SelectSide(0);
        medium.WriteTrackRegister(0);
        medium.WriteSectorRegister(99);
        medium.WriteCommand(0x80);

        Assert.True(medium.IsBusy);
        medium.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Equal((byte)FdcStatusFlags.RecordNotFound, medium.ReadStatus());
        Assert.False(medium.IsBusy);
        Assert.True(medium.IsIrqAsserted);
        Assert.False(medium.IsDrqAsserted);
    }

    [Fact]
    public void D88BackedControllerFacingMedium_CanRestoreSeekAndForceInterrupt()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        IControllerFacingMedium medium = new D88BackedControllerFacingMedium(container);

        medium.Reset();
        medium.WriteTrackRegister(7);
        medium.WriteDataRegister(3);
        medium.WriteCommand(0x1F);

        Assert.True(medium.IsBusy);
        medium.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Equal(3, medium.ReadTrackRegister());
        Assert.False(medium.IsBusy);
        Assert.True(medium.IsIrqAsserted);

        medium.WriteCommand(0xD0);
        Assert.False(medium.IsIrqAsserted);

        medium.WriteTrackRegister(9);
        medium.WriteCommand(0x00);
        Assert.True(medium.IsBusy);
        medium.Advance(TimeSpan.FromMilliseconds(1));
        Assert.Equal(0, medium.ReadTrackRegister());
        Assert.False(medium.IsBusy);
        Assert.True(medium.IsIrqAsserted);
    }

    [Fact]
    public void FdcMediumController_GetVisibleState_DoesNotConsumeTransferData()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x5A, 0x6B, 0x7C });

        var controller = new FdcMediumController(new D88BackedControllerFacingMedium(container));

        controller.WriteRegister(Legacy89DiskKit.Fdc.Domain.Model.FdcRegister.Track, 0);
        controller.WriteRegister(Legacy89DiskKit.Fdc.Domain.Model.FdcRegister.Sector, 1);
        controller.WriteRegister(Legacy89DiskKit.Fdc.Domain.Model.FdcRegister.CommandStatus, 0x80);

        Assert.True(controller.GetVisibleState().Busy);
        controller.Advance(TimeSpan.FromMilliseconds(1));

        var visible = controller.GetVisibleState();

        Assert.Equal(0x5A, visible.Data);
        Assert.Equal(0, visible.SelectedDrive);
        Assert.Equal(0, visible.SelectedSide);
        Assert.True(visible.Drq);
        Assert.Equal(0x5A, controller.ReadRegister(Legacy89DiskKit.Fdc.Domain.Model.FdcRegister.Data));
        Assert.Equal(0x6B, controller.ReadRegister(Legacy89DiskKit.Fdc.Domain.Model.FdcRegister.Data));
    }

    [Fact]
    public void FdcMediumController_ReflectsSelectedDriveAndSide()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        var medium = new D88BackedControllerFacingMedium(container);
        medium.SelectSide(1);
        var controller = new FdcMediumController(medium, selectedDrive: 2);

        var visible = controller.GetVisibleState();

        Assert.Equal(2, visible.SelectedDrive);
        Assert.Equal(1, visible.SelectedSide);
    }

    [Fact]
    public void D88BackedControllerFacingMedium_ReturnsUnsupportedCommandStatus()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        IControllerFacingMedium medium = new D88BackedControllerFacingMedium(container);

        medium.Reset();
        medium.WriteCommand(0xFF);

        Assert.Equal((byte)FdcStatusFlags.UnsupportedCommand, medium.ReadStatus());
        Assert.True(medium.IsIrqAsserted);
        Assert.False(medium.IsDrqAsserted);
        Assert.False(medium.IsBusy);
    }
}
