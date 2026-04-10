using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal sealed record HostProofTranscriptEntry(
    EmulatorHostRequest Request,
    EmulatorHostExchange Exchange);
