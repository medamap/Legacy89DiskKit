namespace Legacy89DiskKit.Fdc.Domain.Model;

/// <summary>
/// Identifies a controller-visible register in the minimal FDC-facing contract.
/// </summary>
public enum FdcRegister
{
    CommandStatus = 0,
    Track = 1,
    Sector = 2,
    Data = 3
}
