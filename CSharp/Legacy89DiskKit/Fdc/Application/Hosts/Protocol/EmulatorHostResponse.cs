using Legacy89DiskKit.Fdc.Domain.Model;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public sealed record EmulatorHostResponse(
    byte? RegisterValue,
    FdcVisibleState? VisibleState,
    bool IrqAsserted,
    bool DrqAsserted,
    long? PendingAdvanceMicroseconds,
    EmulatorHostCapabilities? Capabilities = null,
    string? ErrorMessage = null);
