using Legacy89DiskKit.Domain.Fdc.Model;

namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public sealed record EmulatorHostResponse(
    byte? RegisterValue,
    FdcVisibleState? VisibleState,
    bool IrqAsserted,
    bool DrqAsserted,
    long? PendingAdvanceMicroseconds,
    EmulatorHostCapabilities? Capabilities = null);
