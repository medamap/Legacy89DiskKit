using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public sealed record EmulatorHostTranscriptEntry(
    EmulatorHostRequest Request,
    EmulatorHostExchange Exchange);
