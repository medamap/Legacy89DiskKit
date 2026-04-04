using Legacy89DiskKit.Fdc.Domain.Interface;
using Legacy89DiskKit.Fdc.Domain.Model;

namespace Legacy89DiskKit.Fdc.Infrastructure;

public class FdcMediumController : IFdcController, ITimedFdcController
{
    private readonly IControllerFacingMedium _medium;
    private readonly int _selectedDrive;

    public FdcMediumController(IControllerFacingMedium medium, int selectedDrive = 0)
    {
        _medium = medium ?? throw new ArgumentNullException(nameof(medium));
        _selectedDrive = selectedDrive;
    }

    public void Reset()
    {
        _medium.Reset();
    }

    public void WriteRegister(FdcRegister register, byte value)
    {
        switch (register)
        {
            case FdcRegister.CommandStatus:
                _medium.WriteCommand(value);
                break;
            case FdcRegister.Track:
                _medium.WriteTrackRegister(value);
                break;
            case FdcRegister.Sector:
                _medium.WriteSectorRegister(value);
                break;
            case FdcRegister.Data:
                _medium.WriteDataRegister(value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(register), register, "Unsupported FDC register.");
        }
    }

    public byte ReadRegister(FdcRegister register)
    {
        return register switch
        {
            FdcRegister.CommandStatus => _medium.ReadStatus(),
            FdcRegister.Track => _medium.ReadTrackRegister(),
            FdcRegister.Sector => _medium.ReadSectorRegister(),
            FdcRegister.Data => _medium.ReadDataRegister(),
            _ => throw new ArgumentOutOfRangeException(nameof(register), register, "Unsupported FDC register.")
        };
    }

    public FdcVisibleState GetVisibleState()
    {
        return new FdcVisibleState(
            _medium.ReadStatus(),
            _medium.ReadTrackRegister(),
            _medium.ReadSectorRegister(),
            _medium.PeekDataRegister(),
            _selectedDrive,
            _medium.SelectedSide,
            _medium.IsBusy,
            _medium.IsIrqAsserted,
            _medium.IsDrqAsserted
        );
    }

    public void Advance(TimeSpan delta)
    {
        _medium.Advance(delta);
    }

    public TimeSpan? GetPendingAdvanceHint()
    {
        return _medium.GetPendingDelayHint();
    }
}
