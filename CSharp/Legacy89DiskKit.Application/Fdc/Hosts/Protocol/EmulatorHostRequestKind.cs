namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public enum EmulatorHostRequestKind
{
    SelectDrive,
    SelectSide,
    Reset,
    WriteRegister,
    ReadRegister,
    Advance,
    QueryState
}
