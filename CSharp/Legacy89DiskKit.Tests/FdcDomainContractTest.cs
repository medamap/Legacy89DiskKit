using Legacy89DiskKit.Domain.Drive.Model;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Fdc.Model;
using Legacy89DiskKit.Domain.Timing.Interface;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class FdcDomainContractTest
{
    [Fact]
    public void DriveState_CanRepresentMountedMediumState()
    {
        var state = new DriveState(0, true, 12, 1, true, true, false, "d88");

        Assert.Equal(0, state.DriveNumber);
        Assert.True(state.HasMountedMedium);
        Assert.Equal(12, state.CurrentTrack);
        Assert.Equal("d88", state.MountedMediumKind);
    }

    [Fact]
    public void FakeController_CanExposeMinimalVisibleState()
    {
        var controller = new FakeFdcController();

        controller.WriteRegister(FdcRegister.Track, 0x12);
        controller.WriteRegister(FdcRegister.Sector, 0x34);
        controller.WriteRegister(FdcRegister.Data, 0x56);

        var visible = controller.GetVisibleState();

        Assert.Equal(0x12, visible.Track);
        Assert.Equal(0x34, visible.Sector);
        Assert.Equal(0x56, visible.Data);
        Assert.False(visible.Irq);
        Assert.False(visible.Drq);
    }

    [Fact]
    public void ControllerClock_CanAdvanceElapsedTime()
    {
        IControllerClock clock = new FakeControllerClock();

        clock.Advance(TimeSpan.FromMilliseconds(5));
        clock.Advance(TimeSpan.FromMilliseconds(7));

        Assert.Equal(TimeSpan.FromMilliseconds(12), clock.Elapsed);
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
