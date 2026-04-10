namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public sealed class EmulatorHostObservableProtocolStdioRunner
{
    private readonly EmulatorHostObservableProtocolSession _session;
    private readonly TextReader _reader;
    private readonly TextWriter _writer;

    public EmulatorHostObservableProtocolStdioRunner(
        EmulatorHostObservableProtocolSession session,
        TextReader? reader = null,
        TextWriter? writer = null)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _reader = reader ?? Console.In;
        _writer = writer ?? Console.Out;
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return _session.RunAsync(_reader, _writer, cancellationToken);
    }
}
