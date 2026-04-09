namespace Legacy89DiskKit.Cli.Logging;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public sealed class LogMessage
{
    public DateTime Timestamp { get; }
    public LogLevel Level { get; }
    public string Message { get; }
    public string? Source { get; }

    public LogMessage(LogLevel level, string message, string? source = null)
    {
        Timestamp = DateTime.UtcNow;
        Level = level;
        Message = message;
        Source = source;
    }
}
