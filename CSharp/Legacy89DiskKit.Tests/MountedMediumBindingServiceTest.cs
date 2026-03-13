using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Fdc.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.Fdc;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class MountedMediumBindingServiceTest
{
    [Fact]
    public void BindingService_CanCreateAndMountD88Binding()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        var mountService = Legacy89DiskKitApplication.CreateDriveMountService();
        var bindingService = Legacy89DiskKitApplication.CreateMountedMediumBindingService();

        var binding = bindingService.MountContainer(mountService, 0, container);
        var state = mountService.GetState(0);

        Assert.Equal("d88-family", binding.MountedMedium.MediumKind);
        Assert.NotNull(binding.SectorMedium);
        Assert.NotNull(binding.ControllerFacingMedium);
        Assert.True(state.HasMountedMedium);
        Assert.Equal("d88-family", state.MountedMediumKind);
    }

    [Fact]
    public void BindingService_CanCreateAndMountRawBinding()
    {
        using var container = RawDiskContainer.CreateNewInMemory(Domain.DiskImage.Model.DiskType.TwoD);
        var mountService = Legacy89DiskKitApplication.CreateDriveMountService();
        var bindingService = Legacy89DiskKitApplication.CreateMountedMediumBindingService();

        var binding = bindingService.MountContainer(mountService, 1, container);
        var state = mountService.GetState(1);

        Assert.Equal("raw-sector-image", binding.MountedMedium.MediumKind);
        Assert.NotNull(binding.SectorMedium);
        Assert.NotNull(binding.ControllerFacingMedium);
        Assert.True(state.HasMountedMedium);
        Assert.Equal("raw-sector-image", state.MountedMediumKind);
    }

    [Fact]
    public void BoundControllerMedium_CanBeUsedThroughFdcAccessService()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x6C, 0x00, 0x00 });

        var bindingService = Legacy89DiskKitApplication.CreateMountedMediumBindingService();
        var binding = bindingService.CreateFromContainer(container);
        var controller = new FdcMediumController(binding.ControllerFacingMedium!);
        var accessService = Legacy89DiskKitApplication.CreateFdcAccessService(controller);

        accessService.Reset();
        accessService.WriteRegister(FdcRegister.Track, 0);
        accessService.WriteRegister(FdcRegister.Sector, 1);
        accessService.WriteRegister(FdcRegister.CommandStatus, 0x80);
        controller.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Equal(0x6C, accessService.ReadRegister(FdcRegister.Data));
    }

    [Fact]
    public void BindingService_CanCreateDriveAwareController()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 1, 1, new byte[] { 0x6C, 0x7D });

        var bindingService = Legacy89DiskKitApplication.CreateMountedMediumBindingService();
        var binding = bindingService.CreateFromContainer(container);
        binding.ControllerFacingMedium!.SelectSide(1);
        var controller = bindingService.CreateController(binding, 3);
        var timedController = Assert.IsAssignableFrom<ITimedFdcController>(controller);

        controller.WriteRegister(FdcRegister.Track, 0);
        controller.WriteRegister(FdcRegister.Sector, 1);
        controller.WriteRegister(FdcRegister.CommandStatus, 0x80);
        timedController.Advance(TimeSpan.FromMilliseconds(1));

        var visible = controller.GetVisibleState();

        Assert.Equal(3, visible.SelectedDrive);
        Assert.Equal(1, visible.SelectedSide);
        Assert.Equal(0x6C, visible.Data);
    }
}
