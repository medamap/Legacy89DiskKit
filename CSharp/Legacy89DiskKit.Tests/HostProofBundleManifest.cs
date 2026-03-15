namespace Legacy89DiskKit.Tests;

internal sealed record HostProofBundleManifest(
    string BaseName,
    string ReportFileName,
    string TranscriptFileName,
    string? RequestScriptFileName,
    string OpenMode,
    string ExchangeMode
);
