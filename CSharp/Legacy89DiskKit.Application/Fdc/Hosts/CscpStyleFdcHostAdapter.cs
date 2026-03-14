using Legacy89DiskKit.Application.Drive;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Fdc.Model;

namespace Legacy89DiskKit.Application.Fdc.Hosts;

public class CscpStyleFdcHostAdapter
{
    private readonly DriveMountService _driveMountService;
    private readonly MountedMediumBindingService _bindingService;
    private readonly Dictionary<int, MountedMediumBinding> _bindings = new();
    private readonly Dictionary<int, ControllerBinding> _controllers = new();
    private int _selectedDrive;
    private bool _lastIrq;
    private bool _lastDrq;
    private TimeSpan? _lastAdvanceHint;

    public event Action<bool>? IrqChanged;

    public event Action<bool>? DrqChanged;

    public event Action<TimeSpan>? AdvanceRequested;

    public CscpStyleFdcHostAdapter(DriveMountService driveMountService, MountedMediumBindingService bindingService)
    {
        _driveMountService = driveMountService ?? throw new ArgumentNullException(nameof(driveMountService));
        _bindingService = bindingService ?? throw new ArgumentNullException(nameof(bindingService));
    }

    public void OpenDisk(int driveNumber, IDiskContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        var binding = _bindingService.MountContainer(_driveMountService, driveNumber, container);
        var controller = _bindingService.CreateController(binding, driveNumber);
        if (controller is not ITimedFdcController timedController)
        {
            throw new NotSupportedException("The mounted medium controller does not support timing advancement.");
        }

        _bindings[driveNumber] = binding;
        _controllers[driveNumber] = new ControllerBinding(controller, timedController);
        SyncSignals();
    }

    public bool CloseDisk(int driveNumber)
    {
        _bindings.Remove(driveNumber);
        _controllers.Remove(driveNumber);
        var result = _driveMountService.Unmount(driveNumber);
        SyncSignals();
        return result;
    }

    public bool IsDiskInserted(int driveNumber)
    {
        return _driveMountService.IsMounted(driveNumber);
    }

    public bool IsDriveReady(int driveNumber)
    {
        return _driveMountService.GetState(driveNumber).IsReady;
    }

    public void SelectDrive(int driveNumber)
    {
        EnsureDriveMounted(driveNumber);
        _selectedDrive = driveNumber;
        SyncSignals();
    }

    public void SelectSide(int side)
    {
        if (!_bindings.TryGetValue(_selectedDrive, out var binding) || binding.ControllerFacingMedium is null)
        {
            throw new InvalidOperationException("No controller-facing medium is mounted for the selected drive.");
        }

        binding.ControllerFacingMedium.SelectSide(side);
        SyncSignals();
    }

    public void Reset()
    {
        CurrentController.Controller.Reset();
        SyncSignals();
    }

    public void WriteIo8(uint address, byte value)
    {
        CurrentController.Controller.WriteRegister(MapRegister(address), value);
        SyncSignals();
    }

    public byte ReadIo8(uint address)
    {
        var value = CurrentController.Controller.ReadRegister(MapRegister(address));
        SyncSignals();
        return value;
    }

    public void Advance(TimeSpan delta)
    {
        CurrentController.TimedController.Advance(delta);
        SyncSignals();
    }

    public FdcVisibleState GetVisibleState()
    {
        return CurrentController.Controller.GetVisibleState();
    }

    public bool IsIrqAsserted()
    {
        return GetVisibleState().Irq;
    }

    public bool IsDrqAsserted()
    {
        return GetVisibleState().Drq;
    }

    private void SyncSignals()
    {
        var visible = TryGetVisibleState();
        var irq = visible?.Irq ?? false;
        var drq = visible?.Drq ?? false;
        var hint = TryGetPendingAdvanceHint();

        if (irq != _lastIrq)
        {
            _lastIrq = irq;
            IrqChanged?.Invoke(irq);
        }

        if (drq != _lastDrq)
        {
            _lastDrq = drq;
            DrqChanged?.Invoke(drq);
        }

        if (hint is null)
        {
            _lastAdvanceHint = null;
            return;
        }

        if (_lastAdvanceHint != hint)
        {
            _lastAdvanceHint = hint;
            AdvanceRequested?.Invoke(hint.Value);
        }
    }

    private FdcVisibleState? TryGetVisibleState()
    {
        if (_controllers.TryGetValue(_selectedDrive, out var controller))
        {
            return controller.Controller.GetVisibleState();
        }

        return null;
    }

    private TimeSpan? TryGetPendingAdvanceHint()
    {
        if (_controllers.TryGetValue(_selectedDrive, out var controller))
        {
            return controller.TimedController.GetPendingAdvanceHint();
        }

        return null;
    }

    private ControllerBinding CurrentController
    {
        get
        {
            if (_controllers.TryGetValue(_selectedDrive, out var controller))
            {
                return controller;
            }

            throw new InvalidOperationException("No disk is mounted for the selected drive.");
        }
    }

    private void EnsureDriveMounted(int driveNumber)
    {
        if (!_controllers.ContainsKey(driveNumber))
        {
            throw new InvalidOperationException("No disk is mounted for the requested drive.");
        }
    }

    private static FdcRegister MapRegister(uint address)
    {
        return address switch
        {
            0 => FdcRegister.CommandStatus,
            1 => FdcRegister.Track,
            2 => FdcRegister.Sector,
            3 => FdcRegister.Data,
            _ => throw new ArgumentOutOfRangeException(nameof(address), address, "Unsupported FDC register address.")
        };
    }

    private sealed record ControllerBinding(IFdcController Controller, ITimedFdcController TimedController);
}
