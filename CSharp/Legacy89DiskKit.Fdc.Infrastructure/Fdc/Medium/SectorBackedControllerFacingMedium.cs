using Legacy89DiskKit.Fdc.Domain.Interface;
using Legacy89DiskKit.Fdc.Domain.Model;

namespace Legacy89DiskKit.Fdc.Infrastructure.Medium;

public abstract class SectorBackedControllerFacingMedium : IControllerFacingMedium
{
    private enum PendingOperation
    {
        None,
        Restore,
        Seek,
        ReadSector
    }

    private static readonly TimeSpan CommandDelay = TimeSpan.FromMilliseconds(1);

    private byte _status;
    private byte _track;
    private byte _sector = 1;
    private byte _data;
    private int _selectedSide;
    private bool _irq;
    private bool _drq;
    private byte[] _transferBuffer = [];
    private int _transferIndex;
    private PendingOperation _pendingOperation;
    private TimeSpan _remainingDelay;
    private byte _pendingSeekTrack;

    public abstract string MediumKind { get; }

    public bool IsReady => true;

    public abstract bool IsWriteProtected { get; }

    public int SelectedSide => _selectedSide;

    public bool IsBusy => _pendingOperation != PendingOperation.None;

    public bool IsIrqAsserted => _irq;

    public bool IsDrqAsserted => _drq;

    public TimeSpan? GetPendingDelayHint()
    {
        return _pendingOperation == PendingOperation.None ? null : _remainingDelay;
    }

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
        _pendingOperation = PendingOperation.None;
        _remainingDelay = TimeSpan.Zero;
        _pendingSeekTrack = 0;
    }

    public void Advance(TimeSpan delta)
    {
        if (_pendingOperation == PendingOperation.None || delta <= TimeSpan.Zero)
        {
            return;
        }

        _remainingDelay -= delta;
        if (_remainingDelay > TimeSpan.Zero)
        {
            return;
        }

        CompletePendingOperation();
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
            StartPendingOperation(PendingOperation.ReadSector);
            return;
        }

        switch (value)
        {
            case <= 0x0F:
                StartPendingOperation(PendingOperation.Restore);
                break;
            case >= 0x10 and <= 0x1F:
                _pendingSeekTrack = _data;
                StartPendingOperation(PendingOperation.Seek);
                break;
            case 0xD0:
                ExecuteForceInterrupt();
                break;
            default:
                _status = (byte)FdcStatusFlags.UnsupportedCommand;
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

    private void StartPendingOperation(PendingOperation operation)
    {
        _pendingOperation = operation;
        _remainingDelay = CommandDelay;
        _status = (byte)FdcStatusFlags.Busy;
        _irq = false;
        _drq = false;
        ClearTransfer();
    }

    private void CompletePendingOperation()
    {
        var operation = _pendingOperation;
        _pendingOperation = PendingOperation.None;
        _remainingDelay = TimeSpan.Zero;

        switch (operation)
        {
            case PendingOperation.Restore:
                _track = 0;
                _status = 0;
                _irq = true;
                _drq = false;
                ClearTransfer();
                break;
            case PendingOperation.Seek:
                _track = _pendingSeekTrack;
                _status = 0;
                _irq = true;
                _drq = false;
                ClearTransfer();
                break;
            case PendingOperation.ReadSector:
                CompleteReadSector();
                break;
        }
    }

    private void ExecuteForceInterrupt()
    {
        _status = 0;
        _irq = false;
        _drq = false;
        ClearTransfer();
        _pendingOperation = PendingOperation.None;
        _remainingDelay = TimeSpan.Zero;
    }

    private void CompleteReadSector()
    {
        if (!SectorExistsCore(_track, _selectedSide, _sector))
        {
            _status = (byte)FdcStatusFlags.RecordNotFound;
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
