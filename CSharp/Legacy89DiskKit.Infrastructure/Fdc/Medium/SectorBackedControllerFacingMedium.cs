using Legacy89DiskKit.Domain.Fdc.Interface;

namespace Legacy89DiskKit.Infrastructure.Fdc.Medium;

public abstract class SectorBackedControllerFacingMedium : IControllerFacingMedium
{
    private byte _status;
    private byte _track;
    private byte _sector = 1;
    private byte _data;
    private int _selectedSide;
    private bool _irq;
    private bool _drq;
    private byte[] _transferBuffer = [];
    private int _transferIndex;

    public abstract string MediumKind { get; }

    public bool IsReady => true;

    public abstract bool IsWriteProtected { get; }

    public int SelectedSide => _selectedSide;

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
        ClearTransfer();
    }

    public void SelectSide(int side)
    {
        _selectedSide = side;
    }

    public void SeekTrack(int track)
    {
        _track = (byte)Math.Clamp(track, byte.MinValue, byte.MaxValue);
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

    public byte PeekDataRegister()
    {
        return _data;
    }

    public byte ReadDataRegister()
    {
        var value = _data;

        if (_drq)
        {
            AdvanceTransfer();
        }

        return value;
    }

    public void WriteCommand(byte value)
    {
        if ((value & 0xF0) == 0x80)
        {
            ExecuteReadSector();
            return;
        }

        switch (value)
        {
            case <= 0x0F:
                ExecuteRestore();
                break;
            case >= 0x10 and <= 0x1F:
                ExecuteSeek();
                break;
            case 0xD0:
                ExecuteForceInterrupt();
                break;
            default:
                _status = 0x40;
                _irq = true;
                _drq = false;
                ClearTransfer();
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

    protected abstract bool SectorExistsCore(int track, int side, int sector);

    protected abstract byte[] ReadSectorCore(int track, int side, int sector);

    private void ExecuteRestore()
    {
        _track = 0;
        _status = 0;
        _irq = true;
        _drq = false;
        ClearTransfer();
    }

    private void ExecuteSeek()
    {
        _track = _data;
        _status = 0;
        _irq = true;
        _drq = false;
        ClearTransfer();
    }

    private void ExecuteForceInterrupt()
    {
        _status = 0;
        _irq = false;
        _drq = false;
        ClearTransfer();
    }

    private void ExecuteReadSector()
    {
        if (!SectorExistsCore(_track, _selectedSide, _sector))
        {
            _status = 0x10;
            _irq = true;
            _drq = false;
            ClearTransfer();
            return;
        }

        var data = ReadSectorCore(_track, _selectedSide, _sector);
        _status = 0;
        _irq = true;
        BeginTransfer(data);
    }

    private void BeginTransfer(byte[] data)
    {
        _transferBuffer = data.Length == 0 ? [0] : data;
        _transferIndex = 0;
        _data = _transferBuffer[0];
        _drq = true;
    }

    private void AdvanceTransfer()
    {
        _transferIndex++;
        if (_transferIndex >= _transferBuffer.Length)
        {
            _drq = false;
            _data = 0;
            ClearTransfer();
            return;
        }

        _data = _transferBuffer[_transferIndex];
        _drq = true;
    }

    private void ClearTransfer()
    {
        _transferBuffer = [];
        _transferIndex = 0;
    }
}
