namespace Legacy89DiskKit.Tests;

internal static class HostProofBundleComparer
{
    public static IReadOnlyList<string> Compare(HostProofBundle bundle, HostProofExpectation expectation)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(expectation);

        var report = HostProofReportBuilder.Build(
            bundle.Transcript,
            bundle.Manifest.OpenMode,
            bundle.Manifest.ExchangeMode);

        var mismatches = new List<string>();

        AddIfRequired(expectation.RequirePathOpen && !report.SupportsPathOpen, "SupportsPathOpen was required.");
        AddIfRequired(expectation.RequireBufferOpen && !report.SupportsBufferOpen, "SupportsBufferOpen was required.");
        AddIfRequired(expectation.RequireNotificationExchange && !report.SupportsNotificationExchange, "SupportsNotificationExchange was required.");
        AddIfRequired(expectation.RequireDiskOpen && !report.DiskOpenSucceeded, "Disk open was required.");
        AddIfRequired(expectation.RequireBusyObserved && !report.BusyObserved, "Busy observation was required.");
        AddIfRequired(expectation.RequireIrqObserved && !report.IrqObserved, "IRQ observation was required.");
        AddIfRequired(expectation.RequireDrqObserved && !report.DrqObserved, "DRQ observation was required.");
        AddIfRequired(expectation.RequireDataRead && !report.DataReadSucceeded, "Data read was required.");
        AddIfRequired(expectation.RequireClose && !report.CloseSucceeded, "Close was required.");

        return mismatches;

        void AddIfRequired(bool condition, string message)
        {
            if (condition)
            {
                mismatches.Add(message);
            }
        }
    }
}
