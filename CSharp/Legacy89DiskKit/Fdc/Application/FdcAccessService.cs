using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Fdc.Model;
using Legacy89DiskKit.Domain.Timing.Interface;

namespace Legacy89DiskKit.Fdc.Application;

public class FdcAccessService
{
    private readonly IFdcController _controller;
    private readonly IControllerClock? _clock;

    public FdcAccessService(IFdcController controller, IControllerClock? clock = null)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _clock = clock;
    }

    public bool SupportsTimingAdvance => _clock is not null;

    public void Reset()
    {
        _controller.Reset();
    }

    public void WriteRegister(FdcRegister register, byte value)
    {
        _controller.WriteRegister(register, value);
    }

    public byte ReadRegister(FdcRegister register)
    {
        return _controller.ReadRegister(register);
    }

    public FdcVisibleState GetVisibleState()
    {
        return _controller.GetVisibleState();
    }

    public void Advance(TimeSpan delta)
    {
        if (_clock is null)
        {
            throw new InvalidOperationException("Timing advance is not available without a controller clock.");
        }

        _clock.Advance(delta);

        if (_controller is ITimedFdcController timedController)
        {
            timedController.Advance(delta);
        }
    }
}
