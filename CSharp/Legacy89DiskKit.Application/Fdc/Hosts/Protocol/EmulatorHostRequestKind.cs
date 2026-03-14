namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public enum EmulatorHostRequestKind
{
    OpenDiskPath,
    CloseDisk,
    SelectDrive,
    SelectSide,
    Reset,
    WriteRegister,
    ReadRegister,
    Advance,
    QueryState
}
