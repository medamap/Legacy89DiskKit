using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Fdc.Model;
using Legacy89DiskKit.Domain.Timing.Interface;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class FdcApplicationServiceTest
{
    [Fact]
    public void DriveMountService_CanMountAndExposeDriveState()
    {
        var service = Legacy89DiskKitApplication.CreateDriveMountService();
        var medium = new FakeMountedMedium("d88", supportsDirectImageAccess: true, supportsControllerFacingAccess: true);

        service.Mount(0, medium);

        var state = service.GetState(0, currentTrack: 12, selectedSide: 1, motorOn: true);

        Assert.True(service.IsMounted(0));
        Assert.Same(medium, service.GetMountedMedium(0));
        Assert.Equal(12, state.CurrentTrack);
        Assert.Equal(1, state.SelectedSide);
        Assert.True(state.MotorOn);
        Assert.True(state.IsReady);
        Assert.Equal("d88", state.MountedMediumKind);
    }

    [Fact]
    public void FdcAccessService_CanUseControllerAndClockFromBootstrap()
    {
        var controller = new FakeFdcController();
        var clock = new FakeControllerClock();
        var service = Legacy89DiskKitApplication.CreateFdcAccessService(controller, clock);

        service.WriteRegister(FdcRegister.Track, 0x22);
        service.WriteRegister(FdcRegister.Sector, 0x33);
        service.Advance(TimeSpan.FromMilliseconds(4));

        var visible = service.GetVisibleState();

        Assert.True(service.SupportsTimingAdvance);
        Assert.Equal(0x22, visible.Track);
        Assert.Equal(0x33, visible.Sector);
        Assert.Equal(TimeSpan.FromMilliseconds(4), clock.Elapsed);
    }

    [Fact]
    public void FdcAccessService_ThrowsWhenTimingAdvanceIsUnavailable()
    {
        var controller = new FakeFdcController();
        var service = Legacy89DiskKitApplication.CreateFdcAccessService(controller);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Advance(TimeSpan.FromMilliseconds(1)));

        Assert.Equal("Timing advance is not available without a controller clock.", ex.Message);
    }

    private sealed class FakeMountedMedium(string mediumKind, bool supportsDirectImageAccess, bool supportsControllerFacingAccess) : IMountedMedium
    {
        public string MediumKind { get; } = mediumKind;

        public bool SupportsDirectImageAccess { get; } = supportsDirectImageAccess;

        public bool SupportsControllerFacingAccess { get; } = supportsControllerFacingAccess;
    }

    private sealed class FakeFdcController : IFdcController
    {
        private readonly Dictionary<FdcRegister, byte> _registers = new()
        {
            [FdcRegister.CommandStatus] = 0,
            [FdcRegister.Track] = 0,
            [FdcRegister.Sector] = 0,
            [FdcRegister.Data] = 0
        };

        public void Reset()
        {
            _registers[FdcRegister.CommandStatus] = 0;
            _registers[FdcRegister.Track] = 0;
            _registers[FdcRegister.Sector] = 0;
            _registers[FdcRegister.Data] = 0;
        }

        public void WriteRegister(FdcRegister register, byte value)
        {
            _registers[register] = value;
        }

        public byte ReadRegister(FdcRegister register)
        {
            return _registers[register];
        }

        public FdcVisibleState GetVisibleState()
        {
            return new FdcVisibleState(
                _registers[FdcRegister.CommandStatus],
                _registers[FdcRegister.Track],
                _registers[FdcRegister.Sector],
                _registers[FdcRegister.Data],
                0,
                0,
                false,
                false
            );
        }
    }

    private sealed class FakeControllerClock : IControllerClock
    {
        public TimeSpan Elapsed { get; private set; }

        public void Advance(TimeSpan delta)
        {
            Elapsed += delta;
        }
    }
}
