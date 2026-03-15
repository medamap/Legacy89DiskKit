namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public static class EmulatorHostBundleComparer
{
    public static IReadOnlyList<string> Compare(EmulatorHostBundle bundle, EmulatorHostProofExpectation expectation)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(expectation);

        var report = EmulatorHostProofReportBuilder.Build(
            bundle.Transcript,
            bundle.Manifest.OpenMode,
            bundle.Manifest.ExchangeMode);
        return EmulatorHostProofReportComparer.Compare(report, expectation);
    }
}
