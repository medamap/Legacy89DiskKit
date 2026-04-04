namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public sealed class EmulatorHostProtocolEndpoint
{
    private readonly EventDrivenEmulatorFdcHostAdapter _adapter;

    public EmulatorHostProtocolEndpoint(EventDrivenEmulatorFdcHostAdapter adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public string Handle(string requestPayload)
    {
        var request = EmulatorHostProtocolCodec.DeserializeRequest(requestPayload);
        var response = _adapter.Handle(request);
        return EmulatorHostProtocolCodec.SerializeResponse(response);
    }
}
