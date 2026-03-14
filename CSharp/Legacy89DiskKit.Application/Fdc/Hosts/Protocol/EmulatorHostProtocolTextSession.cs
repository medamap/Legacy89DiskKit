namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

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

        return _endpoint.Handle(line);
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

            var response = _endpoint.Handle(line);
            await writer.WriteLineAsync(response);
            await writer.FlushAsync();
        }
    }
}
