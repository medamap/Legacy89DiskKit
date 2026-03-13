namespace Legacy89DiskKit.Domain.Fdc.Model;

/// <summary>
/// Represents the controller-visible state exposed through the minimal FDC-facing contract.
/// </summary>
public sealed record FdcVisibleState(
    byte Status,
    byte Track,
    byte Sector,
    byte Data,
    int SelectedDrive,
    int SelectedSide,
    bool Busy,
    bool Irq,
    bool Drq
);
