namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public enum EmulatorHostRequestKind
{
    QueryCapabilities,
    OpenDiskPath,
    OpenDiskImage,
    CloseDisk,
    SelectDrive,
    SelectSide,
    Reset,
    WriteRegister,
    ReadRegister,
    Advance,
    QueryState
}
