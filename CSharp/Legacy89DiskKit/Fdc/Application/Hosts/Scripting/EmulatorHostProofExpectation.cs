namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public sealed record EmulatorHostProofExpectation(
    bool RequirePathOpen,
    bool RequireBufferOpen,
    bool RequireNotificationExchange,
    bool RequireDiskOpen,
    bool RequireBusyObserved,
    bool RequireIrqObserved,
    bool RequireDrqObserved,
    bool RequireDataRead,
    bool RequireClose
);
