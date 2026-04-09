using System.Text;

namespace Legacy89DiskKit.Cli.Logging;

public sealed class ConsoleLogMessageHandler : ILogMessageHandler
{
    private readonly bool _useErrorStream;

    public ConsoleLogMessageHandler(bool useErrorStream = false)
    {
        _useErrorStream = useErrorStream;
    }

    public void Handle(LogMessage message)
    {
        if (message.Level == LogLevel.Debug) return;

        var text = FormatMessage(message);
        var writer = _useErrorStream || message.Level == LogLevel.Error
            ? Console.Error
            : Console.Out;

        writer.WriteLine(text);
    }

    public void Flush()
    {
        Console.Out.Flush();
        Console.Error.Flush();
    }

    private static string FormatMessage(LogMessage message)
    {
        var timestamp = message.Timestamp.ToString("HH:mm:ss");
        var level = message.Level switch
        {
            LogLevel.Warning => "WARN ",
            LogLevel.Error => "ERROR",
            _ => "INFO "
        };
        var source = message.Source != null ? $" [{message.Source}]" : "";
        return $"{timestamp} {level}{source} {message.Message}";
    }
}
