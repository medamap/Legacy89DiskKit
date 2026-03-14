using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Application.Fdc.Hosts;

public class XmilWebStyleFdcHostAdapter
{
    private readonly EventDrivenEmulatorFdcHostAdapter _adapter;
    private int _selectedDrive;
    private int _selectedSide;

    public XmilWebStyleFdcHostAdapter(EventDrivenEmulatorFdcHostAdapter adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _adapter.IrqChanged += value => IrqChanged?.Invoke(value);
        _adapter.DrqChanged += value => DrqChanged?.Invoke(value);
        _adapter.AdvanceRequested += delay => AdvanceRequested?.Invoke(delay);
    }

    public event Action<bool>? IrqChanged;

    public event Action<bool>? DrqChanged;

    public event Action<TimeSpan>? AdvanceRequested;

    public void MountDisk(int driveNumber, IDiskContainer container)
    {
        _adapter.OpenDisk(driveNumber, container);
    }

    public bool EjectDisk(int driveNumber)
    {
        return _adapter.CloseDisk(driveNumber);
    }

    public void SetDrive(int driveNumber)
    {
        _adapter.SelectDrive(driveNumber);
        _selectedDrive = driveNumber;
    }

    public void SetSide(int side)
    {
        _adapter.SelectSide(side);
        _selectedSide = side;
    }

    public void X1FdcW(uint address, byte value)
    {
        _adapter.WriteIo8(address, value);
    }

    public byte X1FdcR(uint address)
    {
        return _adapter.ReadIo8(address);
    }

    public void Reset()
    {
        _adapter.Reset();
    }

    public void Advance(TimeSpan delta)
    {
        _adapter.Advance(delta);
    }

    public XmilWebFdcState GetState()
    {
        var visible = _adapter.GetVisibleState();

        return new XmilWebFdcState(
            _adapter.IsDiskInserted(_selectedDrive),
            _adapter.IsDriveReady(_selectedDrive),
            visible.Busy,
            visible.Irq,
            visible.Drq,
            _selectedDrive,
            _selectedSide);
    }
}
