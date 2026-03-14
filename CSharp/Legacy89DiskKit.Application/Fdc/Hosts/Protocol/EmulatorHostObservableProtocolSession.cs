namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public sealed class EmulatorHostObservableProtocolSession
{
    private readonly EmulatorHostProtocolEndpoint _endpoint;
    private readonly Queue<EmulatorHostNotification> _notifications = new();

    public EmulatorHostObservableProtocolSession(EventDrivenEmulatorFdcHostAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);

        _endpoint = new EmulatorHostProtocolEndpoint(adapter);
        adapter.IrqChanged += value => _notifications.Enqueue(new EmulatorHostNotification(EmulatorHostNotificationKind.IrqChanged, SignalState: value));
        adapter.DrqChanged += value => _notifications.Enqueue(new EmulatorHostNotification(EmulatorHostNotificationKind.DrqChanged, SignalState: value));
        adapter.AdvanceRequested += delay => _notifications.Enqueue(
            new EmulatorHostNotification(
                EmulatorHostNotificationKind.AdvanceRequested,
                DelayMicroseconds: (long)(delay.TotalMilliseconds * 1000)));
    }

    public string HandleLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new ArgumentException("Request line must not be empty.", nameof(line));
        }

        var responsePayload = _endpoint.Handle(line);
        var response = PatchCapabilities(EmulatorHostProtocolCodec.DeserializeResponse(responsePayload));
        var notifications = DrainNotifications();
        return EmulatorHostProtocolCodec.SerializeExchange(new EmulatorHostExchange(response, notifications));
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

            var exchange = HandleLine(line);
            await writer.WriteLineAsync(exchange);
            await writer.FlushAsync();
        }
    }

    private IReadOnlyList<EmulatorHostNotification> DrainNotifications()
    {
        if (_notifications.Count == 0)
        {
            return [];
        }

        var notifications = new List<EmulatorHostNotification>(_notifications.Count);
        while (_notifications.Count > 0)
        {
            notifications.Add(_notifications.Dequeue());
        }

        return notifications;
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
                SupportsPlainStdio = false,
                SupportsObservableStdio = true
            }
        };
    }
}
