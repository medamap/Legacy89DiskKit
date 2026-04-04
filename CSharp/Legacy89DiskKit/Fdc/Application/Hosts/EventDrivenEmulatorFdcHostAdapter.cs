using Legacy89DiskKit.Drive.Application;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.Fdc.Domain.Interface;
using Legacy89DiskKit.Fdc.Domain.Model;
using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Fdc.Application.Hosts;

public class EventDrivenEmulatorFdcHostAdapter
{
    private static readonly EmulatorHostCapabilities HostCapabilities = new(
        ProtocolVersion: 1,
        SupportsPathOpen: true,
        SupportsBufferOpen: true,
        SupportsNotificationExchange: true,
        SupportsPlainStdio: true,
        SupportsObservableStdio: true);

    private readonly DriveMountService _driveMountService;
    private readonly MountedMediumBindingService _bindingService;
    private readonly IDiskContainerFactory _containerFactory;
    private readonly Dictionary<int, MountedMediumBinding> _bindings = new();
    private readonly Dictionary<int, ControllerBinding> _controllers = new();
    private readonly Dictionary<int, IDiskContainer> _ownedContainers = new();
    private int _selectedDrive;
    private bool _lastIrq;
    private bool _lastDrq;
    private TimeSpan? _lastAdvanceHint;

    public event Action<bool>? IrqChanged;

    public event Action<bool>? DrqChanged;

    public event Action<TimeSpan>? AdvanceRequested;

    public EventDrivenEmulatorFdcHostAdapter(
        DriveMountService driveMountService,
        MountedMediumBindingService bindingService,
        IDiskContainerFactory containerFactory)
    {
        _driveMountService = driveMountService ?? throw new ArgumentNullException(nameof(driveMountService));
        _bindingService = bindingService ?? throw new ArgumentNullException(nameof(bindingService));
        _containerFactory = containerFactory ?? throw new ArgumentNullException(nameof(containerFactory));
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

        ReleaseOwnedContainer(driveNumber);
        _bindings[driveNumber] = binding;
        _controllers[driveNumber] = new ControllerBinding(controller, timedController);
        SyncSignals();
    }

    public void OpenDiskPath(int driveNumber, string imagePath, bool readOnly = true)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new ArgumentException("ImagePath is required.", nameof(imagePath));
        }

        var container = _containerFactory.Open(imagePath, readOnly);
        try
        {
            OpenDisk(driveNumber, container);
            _ownedContainers[driveNumber] = container;
        }
        catch
        {
            container.Dispose();
            throw;
        }
    }

    public void OpenDiskImage(int driveNumber, byte[] imageData, string imageFormat, bool readOnly = true)
    {
        ArgumentNullException.ThrowIfNull(imageData);
        if (string.IsNullOrWhiteSpace(imageFormat))
        {
            throw new ArgumentException("ImageFormat is required.", nameof(imageFormat));
        }

        var container = _containerFactory.Open(imageData, imageFormat, readOnly);
        try
        {
            OpenDisk(driveNumber, container);
            _ownedContainers[driveNumber] = container;
        }
        catch
        {
            container.Dispose();
            throw;
        }
    }

    public bool CloseDisk(int driveNumber)
    {
        _bindings.Remove(driveNumber);
        _controllers.Remove(driveNumber);
        var result = _driveMountService.Unmount(driveNumber);
        ReleaseOwnedContainer(driveNumber);
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

    public EmulatorHostResponse Handle(EmulatorHostRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte? registerValue = null;

        switch (request.Kind)
        {
            case EmulatorHostRequestKind.QueryCapabilities:
                break;
            case EmulatorHostRequestKind.OpenDiskPath:
                OpenDiskPath(
                    request.DriveNumber ?? throw new ArgumentException("DriveNumber is required.", nameof(request)),
                    request.ImagePath ?? throw new ArgumentException("ImagePath is required.", nameof(request)),
                    request.ReadOnly ?? true);
                break;
            case EmulatorHostRequestKind.OpenDiskImage:
                OpenDiskImage(
                    request.DriveNumber ?? throw new ArgumentException("DriveNumber is required.", nameof(request)),
                    Convert.FromBase64String(request.ImageDataBase64 ?? throw new ArgumentException("ImageDataBase64 is required.", nameof(request))),
                    request.ImageFormat ?? throw new ArgumentException("ImageFormat is required.", nameof(request)),
                    request.ReadOnly ?? true);
                break;
            case EmulatorHostRequestKind.CloseDisk:
                CloseDisk(request.DriveNumber ?? throw new ArgumentException("DriveNumber is required.", nameof(request)));
                break;
            case EmulatorHostRequestKind.SelectDrive:
                SelectDrive(request.DriveNumber ?? throw new ArgumentException("DriveNumber is required.", nameof(request)));
                break;
            case EmulatorHostRequestKind.SelectSide:
                SelectSide(request.Side ?? throw new ArgumentException("Side is required.", nameof(request)));
                break;
            case EmulatorHostRequestKind.Reset:
                Reset();
                break;
            case EmulatorHostRequestKind.WriteRegister:
                WriteIo8(
                    request.RegisterAddress ?? throw new ArgumentException("RegisterAddress is required.", nameof(request)),
                    request.RegisterValue ?? throw new ArgumentException("RegisterValue is required.", nameof(request)));
                break;
            case EmulatorHostRequestKind.ReadRegister:
                registerValue = ReadIo8(request.RegisterAddress ?? throw new ArgumentException("RegisterAddress is required.", nameof(request)));
                break;
            case EmulatorHostRequestKind.Advance:
                Advance(TimeSpan.FromMilliseconds((request.AdvanceMicroseconds ?? throw new ArgumentException("AdvanceMicroseconds is required.", nameof(request))) / 1000.0));
                break;
            case EmulatorHostRequestKind.QueryState:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.Kind, "Unsupported host request kind.");
        }

        var visibleState = TryGetVisibleState();
        var pendingAdvance = TryGetPendingAdvanceHint();

        return new EmulatorHostResponse(
            registerValue,
            visibleState,
            visibleState?.Irq ?? false,
            visibleState?.Drq ?? false,
            pendingAdvance is null ? null : (long)pendingAdvance.Value.TotalMilliseconds * 1000,
            request.Kind == EmulatorHostRequestKind.QueryCapabilities ? HostCapabilities : null);
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

    private void ReleaseOwnedContainer(int driveNumber)
    {
        if (_ownedContainers.Remove(driveNumber, out var ownedContainer))
        {
            ownedContainer.Dispose();
        }
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
