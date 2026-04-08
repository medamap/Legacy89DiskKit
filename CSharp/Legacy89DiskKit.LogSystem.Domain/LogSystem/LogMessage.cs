namespace Legacy89DiskKit.LogSystem.Domain;

public enum LogType
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public record LogMessage(
    LogType Type,
    string Message,
    DateTime Timestamp,
    string? Source = null
);