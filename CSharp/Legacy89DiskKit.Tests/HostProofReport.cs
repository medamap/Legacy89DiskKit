namespace Legacy89DiskKit.Tests;

internal sealed record HostProofReport(
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
    bool CloseSucceeded);
