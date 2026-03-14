namespace Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

public sealed class EmulatorHostProtocolStdioRunner
{
    private readonly EmulatorHostProtocolTextSession _session;
    private readonly TextReader _reader;
    private readonly TextWriter _writer;

    public EmulatorHostProtocolStdioRunner(
        EmulatorHostProtocolTextSession session,
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
