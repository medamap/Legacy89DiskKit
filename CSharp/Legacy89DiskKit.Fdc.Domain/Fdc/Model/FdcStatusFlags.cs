namespace Legacy89DiskKit.Fdc.Domain.Model;

[Flags]
public enum FdcStatusFlags : byte
{
    None = 0x00,
    Busy = 0x01,
    RecordNotFound = 0x10,
    UnsupportedCommand = 0x40
}
