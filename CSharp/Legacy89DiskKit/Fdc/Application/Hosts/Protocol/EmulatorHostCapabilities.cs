namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public sealed record EmulatorHostCapabilities(
    int ProtocolVersion,
    bool SupportsPathOpen,
    bool SupportsBufferOpen,
    bool SupportsNotificationExchange,
    bool SupportsPlainStdio,
    bool SupportsObservableStdio);
