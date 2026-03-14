namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public sealed record EmulatorHostNotification(
    EmulatorHostNotificationKind Kind,
    bool? SignalState = null,
    long? DelayMicroseconds = null);
