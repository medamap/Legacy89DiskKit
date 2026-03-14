using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostCliProcessProtocolErrorTest
{
    [Fact]
    public async Task CliHostStdioObservable_CanRecoverAfterMalformedRequestLine()
    {
        await using var process = new CliHostProcessSession();

        var errorPayload = await process.SendRawLineAsync("{not-json");
        var errorExchange = EmulatorHostProtocolCodec.DeserializeExchange(errorPayload);
        Assert.False(string.IsNullOrWhiteSpace(errorExchange.Response.ErrorMessage));

        var capabilitiesPayload = await process.SendRawLineAsync(
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities)));
        var capabilitiesExchange = EmulatorHostProtocolCodec.DeserializeExchange(capabilitiesPayload);

        Assert.NotNull(capabilitiesExchange.Response.Capabilities);
        Assert.True(capabilitiesExchange.Response.Capabilities!.SupportsObservableStdio);
        Assert.Null(capabilitiesExchange.Response.ErrorMessage);
    }

    [Fact]
    public async Task CliHostStdio_CanRecoverAfterMalformedRequestLine()
    {
        await using var process = new CliHostProcessSession(observable: false);

        var errorPayload = await process.SendRawLineAsync("{not-json");
        var errorResponse = EmulatorHostProtocolCodec.DeserializeResponse(errorPayload);
        Assert.False(string.IsNullOrWhiteSpace(errorResponse.ErrorMessage));

        var capabilitiesPayload = await process.SendRawLineAsync(
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities)));
        var capabilitiesResponse = EmulatorHostProtocolCodec.DeserializeResponse(capabilitiesPayload);

        Assert.NotNull(capabilitiesResponse.Capabilities);
        Assert.True(capabilitiesResponse.Capabilities!.SupportsPlainStdio);
        Assert.Null(capabilitiesResponse.ErrorMessage);
    }
}
