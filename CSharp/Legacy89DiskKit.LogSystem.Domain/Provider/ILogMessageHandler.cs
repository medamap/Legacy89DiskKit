using Legacy89DiskKit.LogSystem.Domain;

namespace Legacy89DiskKit.LogSystem.Domain.Provider;

public interface ILogMessageHandler
{
    bool IsEnabled { get; set; }
    LogType MinimumLevel { get; set; }
    void Handle(LogMessage message);
}