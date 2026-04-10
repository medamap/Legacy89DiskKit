namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public sealed class EmulatorHostProtocolTextSession
{
    private readonly EmulatorHostProtocolEndpoint _endpoint;

    public EmulatorHostProtocolTextSession(EmulatorHostProtocolEndpoint endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    public string HandleLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new ArgumentException("Request line must not be empty.", nameof(line));
        }

        try
        {
            var responsePayload = _endpoint.Handle(line);
            var response = EmulatorHostProtocolCodec.DeserializeResponse(responsePayload);
            return EmulatorHostProtocolCodec.SerializeResponse(PatchCapabilities(response));
        }
        catch (Exception ex)
        {
            return EmulatorHostProtocolCodec.SerializeResponse(CreateErrorResponse(ex.Message));
        }
    }

    public async Task RunAsync(TextReader reader, TextWriter writer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(writer);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var response = HandleLine(line);
            await writer.WriteLineAsync(response);
            await writer.FlushAsync();
        }
    }

    private static EmulatorHostResponse PatchCapabilities(EmulatorHostResponse response)
    {
        if (response.Capabilities is null)
        {
            return response;
        }

        return response with
        {
            Capabilities = response.Capabilities with
            {
                SupportsPlainStdio = true,
                SupportsObservableStdio = false
            }
        };
    }

    private static EmulatorHostResponse CreateErrorResponse(string message)
    {
        return new EmulatorHostResponse(
            RegisterValue: null,
            VisibleState: null,
            IrqAsserted: false,
            DrqAsserted: false,
            PendingAdvanceMicroseconds: null,
            ErrorMessage: message);
    }
}
