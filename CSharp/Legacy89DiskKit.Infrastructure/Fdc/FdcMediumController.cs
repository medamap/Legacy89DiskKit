using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Fdc.Model;

namespace Legacy89DiskKit.Infrastructure.Fdc;

public class FdcMediumController : IFdcController
{
    private readonly IControllerFacingMedium _medium;

    public FdcMediumController(IControllerFacingMedium medium)
    {
        _medium = medium ?? throw new ArgumentNullException(nameof(medium));
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
            0,
            0,
            _medium.IsIrqAsserted,
            _medium.IsDrqAsserted
        );
    }
}
