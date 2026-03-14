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
    }

    public bool CloseDisk(int driveNumber)
    {
        _bindings.Remove(driveNumber);
        _controllers.Remove(driveNumber);
        return _driveMountService.Unmount(driveNumber);
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
    }

    public void SelectSide(int side)
    {
        if (!_bindings.TryGetValue(_selectedDrive, out var binding) || binding.ControllerFacingMedium is null)
        {
            throw new InvalidOperationException("No controller-facing medium is mounted for the selected drive.");
        }

        binding.ControllerFacingMedium.SelectSide(side);
    }

    public void Reset()
    {
        CurrentController.Controller.Reset();
    }

    public void WriteIo8(uint address, byte value)
    {
        CurrentController.Controller.WriteRegister(MapRegister(address), value);
    }

    public byte ReadIo8(uint address)
    {
        return CurrentController.Controller.ReadRegister(MapRegister(address));
    }

    public void Advance(TimeSpan delta)
    {
        CurrentController.TimedController.Advance(delta);
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
