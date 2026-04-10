using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

internal static class HostProofAssert
{
    public static void AssertCapabilityHandshake(
        EmulatorHostExchange exchange,
        bool expectObservable,
        bool expectPathOpen,
        bool expectBufferOpen)
    {
        Assert.NotNull(exchange.Response.Capabilities);
        Assert.Equal(expectObservable, exchange.Response.Capabilities!.SupportsObservableStdio);
        Assert.Equal(expectPathOpen, exchange.Response.Capabilities.SupportsPathOpen);
        Assert.Equal(expectBufferOpen, exchange.Response.Capabilities.SupportsBufferOpen);
    }

    public static void AssertAdvanceRequested(EmulatorHostExchange exchange)
    {
        Assert.Contains(exchange.Notifications, x => x.Kind == EmulatorHostNotificationKind.AdvanceRequested);
    }

    public static void AssertReadRegisterValues(
        IReadOnlyList<HostProofTranscriptEntry> transcript,
        params byte[] expectedValues)
    {
        var actual = transcript
            .Where(x => x.Request.Kind == EmulatorHostRequestKind.ReadRegister)
            .Select(x => x.Exchange.Response.RegisterValue)
            .Take(expectedValues.Length)
            .ToArray();

        Assert.Equal(expectedValues.Select(x => (byte?)x).ToArray(), actual);
    }

    public static void AssertTranscriptRoundTrip(IReadOnlyList<HostProofTranscriptEntry> transcript, int expectedCount)
    {
        var payload = HostProofTranscriptCodec.SerializeLines(transcript);
        var roundTrip = HostProofTranscriptCodec.DeserializeLines(payload);
        Assert.Equal(payload, HostProofTranscriptCodec.SerializeLines(roundTrip));
        Assert.Equal(expectedCount, roundTrip.Count);
    }
}
