using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;

namespace Legacy89DiskKit.Infrastructure.Fdc.Medium;

public class D88BackedControllerFacingMedium : IControllerFacingMedium
{
    private readonly D88DiskContainer _container;
    private byte _status;
    private byte _track;
    private byte _sector = 1;
    private byte _data;
    private int _selectedSide;
    private bool _irq;
    private bool _drq;

    public D88BackedControllerFacingMedium(D88DiskContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public string MediumKind => "d88-family";

    public bool IsReady => true;

    public bool IsWriteProtected => _container.IsReadOnly;

    public bool IsIrqAsserted => _irq;

    public bool IsDrqAsserted => _drq;

    public void Reset()
    {
        _status = 0;
        _track = 0;
        _sector = 1;
        _data = 0;
        _selectedSide = 0;
        _irq = false;
        _drq = false;
    }

    public void SelectSide(int side)
    {
        _selectedSide = side;
    }

    public void SeekTrack(int track)
    {
        _track = (byte)track;
    }

    public byte ReadStatus()
    {
        return _status;
    }

    public byte ReadTrackRegister()
    {
        return _track;
    }

    public byte ReadSectorRegister()
    {
        return _sector;
    }

    public byte ReadDataRegister()
    {
        _drq = false;
        return _data;
    }

    public void WriteCommand(byte value)
    {
        switch (value)
        {
            case 0x80:
                ExecuteReadSector();
                break;
            case 0xD0:
                _status = 0;
                _irq = false;
                _drq = false;
                break;
            default:
                _status = 0x40;
                _irq = true;
                _drq = false;
                break;
        }
    }

    public void WriteTrackRegister(byte value)
    {
        _track = value;
    }

    public void WriteSectorRegister(byte value)
    {
        _sector = value;
    }

    public void WriteDataRegister(byte value)
    {
        _data = value;
    }

    private void ExecuteReadSector()
    {
        if (!_container.SectorExists(_track, _selectedSide, _sector))
        {
            _status = 0x10;
            _irq = true;
            _drq = false;
            return;
        }

        var data = _container.ReadSector(_track, _selectedSide, _sector);
        _data = data.Length > 0 ? data[0] : (byte)0;
        _status = 0;
        _irq = true;
        _drq = true;
    }
}
