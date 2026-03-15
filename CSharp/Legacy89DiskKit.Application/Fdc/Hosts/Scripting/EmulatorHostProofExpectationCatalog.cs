namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public static class EmulatorHostProofExpectationCatalog
{
    public static EmulatorHostProofExpectation EventDrivenFirstProofD88()
    {
        return new EmulatorHostProofExpectation(
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

    public static EmulatorHostProofExpectation EventDrivenSecondProofRaw()
    {
        return new EmulatorHostProofExpectation(
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
