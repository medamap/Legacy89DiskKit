namespace Legacy89DiskKit.Domain.Timing.Interface;

/// <summary>
/// Represents the minimal timing progression contract for controller-oriented execution.
/// </summary>
public interface IControllerClock
{
    /// <summary>
    /// Gets the current elapsed controller time.
    /// </summary>
    TimeSpan Elapsed { get; }

    /// <summary>
    /// Advances the controller time by the specified duration.
    /// </summary>
    void Advance(TimeSpan delta);
}
