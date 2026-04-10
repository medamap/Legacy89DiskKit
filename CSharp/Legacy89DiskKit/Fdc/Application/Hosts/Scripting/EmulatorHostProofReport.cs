namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public sealed record EmulatorHostProofReport(
    string OpenMode,
    string ExchangeMode,
    bool CapabilityHandshakeSucceeded,
    bool SupportsPathOpen,
    bool SupportsBufferOpen,
    bool SupportsNotificationExchange,
    bool SupportsPlainStdio,
    bool SupportsObservableStdio,
    bool DiskOpenSucceeded,
    bool BusyObserved,
    bool IrqObserved,
    bool DrqObserved,
    bool DataReadSucceeded,
    bool CloseSucceeded
);
