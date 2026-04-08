using Legacy89DiskKit.LogSystem.Domain;
using Legacy89DiskKit.LogSystem.Domain.Provider;
using MessagePipe;

namespace Legacy89DiskKit.LogSystem.Application.LogSystem;

public class LogSystemService : IDisposable
{
    private readonly IPublisher<LogMessage> _publisher;
    private readonly List<ILogMessageHandler> _handlers = new();
    private readonly LogType _minimumLevel;

    public LogSystemService(IPublisher<LogMessage> publisher, IEnumerable<ILogMessageHandler> handlers, LogType minimumLevel = LogType.Info)
    {
        _publisher = publisher;
        _minimumLevel = minimumLevel;
        
        foreach (var handler in handlers)
        {
            handler.MinimumLevel = minimumLevel;
            _handlers.Add(handler);
        }
    }

    public void Log(LogType type, string message, string? source = null)
    {
        if (type < _minimumLevel) return;

        var logMessage = new LogMessage(type, message, DateTime.Now, source);
        _publisher.Publish(logMessage);
        
        foreach (var handler in _handlers)
        {
            handler.Handle(logMessage);
        }
    }

    public void LogDebug(string message, string? source = null) => Log(LogType.Debug, message, source);
    public void LogInfo(string message, string? source = null) => Log(LogType.Info, message, source);
    public void LogWarning(string message, string? source = null) => Log(LogType.Warning, message, source);
    public void LogError(string message, string? source = null) => Log(LogType.Error, message, source);

    public void Dispose()
    {
        (_publisher as IDisposable)?.Dispose();
    }
}