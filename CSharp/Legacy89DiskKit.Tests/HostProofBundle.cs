using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal sealed record HostProofBundle(
    HostProofBundleManifest Manifest,
    string MarkdownReport,
    IReadOnlyList<HostProofTranscriptEntry> Transcript,
    IReadOnlyList<EmulatorHostRequest> RequestScript
);
