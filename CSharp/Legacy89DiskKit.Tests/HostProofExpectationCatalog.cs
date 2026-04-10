namespace Legacy89DiskKit.Tests;

internal static class HostProofExpectationCatalog
{
    public static HostProofExpectation EventDrivenFirstProofD88()
    {
        return new HostProofExpectation(
            RequirePathOpen: true,
            RequireBufferOpen: false,
            RequireNotificationExchange: true,
            RequireDiskOpen: true,
            RequireBusyObserved: true,
            RequireIrqObserved: true,
            RequireDrqObserved: true,
            RequireDataRead: true,
            RequireClose: true);
    }

    public static HostProofExpectation EventDrivenSecondProofRaw()
    {
        return new HostProofExpectation(
            RequirePathOpen: false,
            RequireBufferOpen: true,
            RequireNotificationExchange: true,
            RequireDiskOpen: true,
            RequireBusyObserved: true,
            RequireIrqObserved: true,
            RequireDrqObserved: true,
            RequireDataRead: true,
            RequireClose: false);
    }
}
