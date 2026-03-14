using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofTranscriptTest
{
    [Fact]
    public void TranscriptCodec_CanRoundTripJsonLines()
    {
        var entries = new[]
        {
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null,
                        Capabilities: new EmulatorHostCapabilities(1, true, true, true, true, true)),
                    [])),
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(0x34, null, true, true, null),
                    [new EmulatorHostNotification(EmulatorHostNotificationKind.IrqChanged, SignalState: true)]))
        };

        var payload = HostProofTranscriptCodec.SerializeLines(entries);
        var roundTrip = HostProofTranscriptCodec.DeserializeLines(payload);

        Assert.Equal(payload, HostProofTranscriptCodec.SerializeLines(roundTrip));
        Assert.Equal(2, roundTrip.Count);
        Assert.Equal(EmulatorHostRequestKind.QueryCapabilities, roundTrip[0].Request.Kind);
        Assert.Equal(EmulatorHostRequestKind.Advance, roundTrip[1].Request.Kind);
    }
}
