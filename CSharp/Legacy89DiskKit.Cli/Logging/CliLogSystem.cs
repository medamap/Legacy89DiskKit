namespace Legacy89DiskKit.Cli.Logging;

public sealed class CliLogSystem : IDisposable
{
    private readonly List<ILogMessageHandler> _handlers = new();
    private readonly object _lock = new();
    private bool _disposed;

    public static CliLogSystem CreateWithDefaultFileLogger(string? logPath = null)
    {
        var system = new CliLogSystem();
        system.AddHandler(new FileLogMessageHandler(logPath));
        return system;
    }

    public static CliLogSystem CreateWithConsoleAndFileLogger(string? logPath = null)
    {
        var system = new CliLogSystem();
        system.AddHandler(new ConsoleLogMessageHandler());
        system.AddHandler(new FileLogMessageHandler(logPath));
        return system;
    }

    public void AddHandler(ILogMessageHandler handler)
    {
        lock (_lock)
        {
            _handlers.Add(handler);
        }
    }

    public void Debug(string message, string? source = null)
    {
        Log(LogLevel.Debug, message, source);
    }

    public void Info(string message, string? source = null)
    {
        Log(LogLevel.Info, message, source);
    }

    public void Warning(string message, string? source = null)
    {
        Log(LogLevel.Warning, message, source);
    }

    public void Error(string message, string? source = null)
    {
        Log(LogLevel.Error, message, source);
    }

    public void Log(LogLevel level, string message, string? source = null)
    {
        if (_disposed) return;

        var logMessage = new LogMessage(level, message, source);
        ILogMessageHandler[] handlers;

        lock (_lock)
        {
            if (_handlers.Count == 0) return;
            handlers = _handlers.ToArray();
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler.Handle(logMessage);
            }
            catch
            {
            }
        }
    }

    public void Flush()
    {
        lock (_lock)
        {
            foreach (var handler in _handlers)
            {
                try
                {
                    handler.Flush();
                }
                catch
                {
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Flush();

        lock (_lock)
        {
            foreach (var handler in _handlers)
            {
                if (handler is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            _handlers.Clear();
        }
    }
}
