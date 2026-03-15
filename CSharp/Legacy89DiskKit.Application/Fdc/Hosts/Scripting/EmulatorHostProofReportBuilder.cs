using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public static class EmulatorHostProofReportBuilder
{
    public static EmulatorHostProofReport Build(
        IReadOnlyList<EmulatorHostTranscriptEntry> transcript,
        string openMode,
        string exchangeMode)
    {
        ArgumentNullException.ThrowIfNull(transcript);
        ArgumentException.ThrowIfNullOrWhiteSpace(openMode);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeMode);

        var firstEntry = transcript.FirstOrDefault();
        var firstExchange = firstEntry?.Exchange;
        var capabilities = firstExchange?.Response.Capabilities;
        var openEntry = transcript.FirstOrDefault(x =>
            x.Request.Kind is EmulatorHostRequestKind.OpenDiskPath or EmulatorHostRequestKind.OpenDiskImage);
        var closeEntry = transcript.LastOrDefault(x => x.Request.Kind == EmulatorHostRequestKind.CloseDisk);

        var readValues = transcript
            .Where(x => x.Request.Kind == EmulatorHostRequestKind.ReadRegister && x.Request.RegisterAddress == 3)
            .Select(x => x.Exchange.Response.RegisterValue)
            .ToArray();

        return new EmulatorHostProofReport(
            OpenMode: openMode,
            ExchangeMode: exchangeMode,
            CapabilityHandshakeSucceeded: capabilities is not null,
            SupportsPathOpen: capabilities?.SupportsPathOpen ?? false,
            SupportsBufferOpen: capabilities?.SupportsBufferOpen ?? false,
            SupportsNotificationExchange: capabilities?.SupportsNotificationExchange ?? false,
            SupportsPlainStdio: capabilities?.SupportsPlainStdio ?? false,
            SupportsObservableStdio: capabilities?.SupportsObservableStdio ?? false,
            DiskOpenSucceeded: openEntry?.Exchange.Response.VisibleState is not null,
            BusyObserved: transcript.Any(x => x.Exchange.Response.VisibleState?.Busy == true),
            IrqObserved: transcript.Any(x => x.Exchange.Response.IrqAsserted),
            DrqObserved: transcript.Any(x => x.Exchange.Response.DrqAsserted),
            DataReadSucceeded: readValues.Any(x => x.HasValue),
            CloseSucceeded: closeEntry is null || closeEntry.Exchange.Response.VisibleState is null);
    }
}
