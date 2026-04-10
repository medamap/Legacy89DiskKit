namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

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
