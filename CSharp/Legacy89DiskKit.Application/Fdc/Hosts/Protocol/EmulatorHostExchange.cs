namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public sealed record EmulatorHostExchange(
    EmulatorHostResponse Response,
    IReadOnlyList<EmulatorHostNotification> Notifications);
