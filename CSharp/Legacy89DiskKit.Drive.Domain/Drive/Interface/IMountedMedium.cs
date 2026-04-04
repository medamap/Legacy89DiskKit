namespace Legacy89DiskKit.Domain.Drive.Interface;

/// <summary>
/// Represents a mounted medium that can back a drive-oriented workflow.
/// </summary>
public interface IMountedMedium
{
    /// <summary>
    /// Gets the medium kind identifier such as a container family or raw representation kind.
    /// </summary>
    string MediumKind { get; }

    /// <summary>
    /// Gets whether the mounted medium supports direct image-style access.
    /// </summary>
    bool SupportsDirectImageAccess { get; }

    /// <summary>
    /// Gets whether the mounted medium supports controller-facing access.
    /// </summary>
    bool SupportsControllerFacingAccess { get; }
}
