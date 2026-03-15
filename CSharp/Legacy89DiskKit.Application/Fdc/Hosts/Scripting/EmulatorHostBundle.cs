using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public sealed record EmulatorHostBundle(
    EmulatorHostBundleManifest Manifest,
    string MarkdownReport,
    IReadOnlyList<EmulatorHostTranscriptEntry> Transcript,
    IReadOnlyList<EmulatorHostRequest> RequestScript
);
