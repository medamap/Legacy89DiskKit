using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public sealed record EmulatorHostTranscriptEntry(
    EmulatorHostRequest Request,
    EmulatorHostExchange Exchange);
