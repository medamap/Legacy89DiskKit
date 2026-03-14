namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public sealed record EmulatorHostRequest(
    EmulatorHostRequestKind Kind,
    int? DriveNumber = null,
    int? Side = null,
    uint? RegisterAddress = null,
    byte? RegisterValue = null,
    long? AdvanceMicroseconds = null);
