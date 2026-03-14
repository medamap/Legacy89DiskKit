namespace Legacy89DiskKit.Tests;

internal sealed record HostProofExpectation(
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
