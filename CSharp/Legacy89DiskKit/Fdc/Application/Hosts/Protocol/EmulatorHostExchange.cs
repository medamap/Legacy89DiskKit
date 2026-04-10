namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public sealed record EmulatorHostExchange(
    EmulatorHostResponse Response,
    IReadOnlyList<EmulatorHostNotification> Notifications);
