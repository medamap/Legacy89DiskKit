namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public sealed record EmulatorHostCapabilities(
    int ProtocolVersion,
    bool SupportsPathOpen,
    bool SupportsBufferOpen,
    bool SupportsNotificationExchange,
    bool SupportsPlainStdio,
    bool SupportsObservableStdio);
