using Legacy89DiskKit.Domain.Fdc.Model;

namespace Legacy89DiskKit.Domain.Fdc.Interface;

/// <summary>
/// Represents the minimal controller-facing contract for future emulator-oriented access.
/// </summary>
public interface IFdcController
{
    /// <summary>
    /// Resets the controller-visible state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Writes a controller-visible register value.
    /// </summary>
    void WriteRegister(FdcRegister register, byte value);

    /// <summary>
    /// Reads a controller-visible register value.
    /// </summary>
    byte ReadRegister(FdcRegister register);

    /// <summary>
    /// Returns the current visible controller state.
    /// </summary>
    FdcVisibleState GetVisibleState();
}
