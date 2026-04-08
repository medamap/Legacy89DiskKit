using Legacy89DiskKit.LogSystem.Domain;
using Legacy89DiskKit.LogSystem.Domain.Provider;

namespace Legacy89DiskKit.LogSystem.Infrastructure.Provider.Common;

public class ConsoleLogMessageHandler : ILogMessageHandler
{
    public bool IsEnabled { get; set; } = true;
    public LogType MinimumLevel { get; set; } = LogType.Info;

    public void Handle(LogMessage message)
    {
        if (!IsEnabled || message.Type < MinimumLevel) return;

        var formatted = FormatMessage(message);
        
        switch (message.Type)
        {
            case LogType.Debug:
            case LogType.Info:
                Console.WriteLine(formatted);
                break;
            case LogType.Warning:
                Console.Error.WriteLine($"[WARN] {formatted}");
                break;
            case LogType.Error:
                Console.Error.WriteLine($"[ERROR] {formatted}");
                break;
        }
    }

    private static string FormatMessage(LogMessage message)
    {
        return $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] [{message.Type}] {message.Message}";
    }
}