using Legacy89DiskKit.LogSystem.Domain;
using Legacy89DiskKit.LogSystem.Domain.Provider;
using MessagePipe;

namespace Legacy89DiskKit.LogSystem.Application.LogSystem;

public class LogMessagePublisher : IDisposable
{
    private readonly IPublisher<LogMessage> _publisher;
    private readonly List<ILogMessageHandler> _handlers = new();

    public LogMessagePublisher(IPublisher<LogMessage> publisher, params ILogMessageHandler[] handlers)
    {
        _publisher = publisher;
        foreach (var handler in handlers)
        {
            _handlers.Add(handler);
        }
    }

    public void Publish(LogType type, string message, string? source = null)
    {
        var logMessage = new LogMessage(type, message, DateTime.Now, source);
        _publisher.Publish(logMessage);
        
        foreach (var handler in _handlers)
        {
            handler.Handle(logMessage);
        }
    }

    public void PublishDebug(string message, string? source = null) => Publish(LogType.Debug, message, source);
    public void PublishInfo(string message, string? source = null) => Publish(LogType.Info, message, source);
    public void PublishWarning(string message, string? source = null) => Publish(LogType.Warning, message, source);
    public void PublishError(string message, string? source = null) => Publish(LogType.Error, message, source);

    public void Dispose()
    {
        (_publisher as IDisposable)?.Dispose();
    }
}