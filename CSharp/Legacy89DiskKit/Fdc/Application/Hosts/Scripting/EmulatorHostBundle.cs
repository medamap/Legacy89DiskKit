using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public sealed record EmulatorHostBundle(
    EmulatorHostBundleManifest Manifest,
    string MarkdownReport,
    IReadOnlyList<EmulatorHostTranscriptEntry> Transcript,
    IReadOnlyList<EmulatorHostRequest> RequestScript
);
