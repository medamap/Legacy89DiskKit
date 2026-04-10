namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public sealed record EmulatorHostNotification(
    EmulatorHostNotificationKind Kind,
    bool? SignalState = null,
    long? DelayMicroseconds = null);
