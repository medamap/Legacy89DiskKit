using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal sealed record HostProofBundle(
    HostProofBundleManifest Manifest,
    string MarkdownReport,
    IReadOnlyList<HostProofTranscriptEntry> Transcript,
    IReadOnlyList<EmulatorHostRequest> RequestScript
);
