using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal sealed record HostProofTranscriptEntry(
    EmulatorHostRequest Request,
    EmulatorHostExchange Exchange);
