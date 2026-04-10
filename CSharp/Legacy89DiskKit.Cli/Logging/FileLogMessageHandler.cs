using System.Text;

namespace Legacy89DiskKit.Cli.Logging;

public sealed class FileLogMessageHandler : ILogMessageHandler, IDisposable
{
    private readonly string? _customPath;
    private readonly bool _useDefaultPath;
    private readonly int _retentionDays;
    private StreamWriter? _writer;
    private readonly object _lock = new();
    private readonly DateTime _createdDate;
    private bool _disposed;

    public FileLogMessageHandler(string? customPath = null, int retentionDays = 7)
    {
        _customPath = customPath;
        _useDefaultPath = string.IsNullOrWhiteSpace(customPath);
        _retentionDays = retentionDays;
        _createdDate = DateTime.UtcNow.Date;
    }

    public void Handle(LogMessage message)
    {
        if (_disposed) return;

        try
        {
            EnsureWriter();
            if (_writer == null) return;

            lock (_lock)
            {
                _writer.WriteLine(FormatMessage(message));
                _writer.Flush();
            }
        }
        catch
        {
        }
    }

    public void Flush()
    {
        try
        {
            lock (_lock)
            {
                _writer?.Flush();
            }
        }
        catch
        {
        }
    }

    private void EnsureWriter()
    {
        if (_writer != null)
        {
            if (!_useDefaultPath || DateTime.UtcNow.Date == _createdDate)
                return;

            RotateIfExpired();
            return;
        }

        var path = GetCurrentLogPath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(path, append: true, encoding: Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    private string GetCurrentLogPath()
    {
        if (!_useDefaultPath)
        {
            return _customPath!;
        }

        return GetDatedLogFilePath();
    }

    private string GetDefaultLogDirectory()
    {
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(home, "log", "Legacy89DiskKit");
        }

        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "Legacy89DiskKit", "log");
        }

        return Path.Combine(Path.GetTempPath(), "Legacy89DiskKit", "log");
    }

    private string GetDatedLogFilePath()
    {
        var directory = GetDefaultLogDirectory();
        var datePrefix = DateTime.UtcNow.ToString("yyyy-MM-dd");
        return Path.Combine(directory, $"{datePrefix}.log");
    }

    private void RotateIfExpired()
    {
        try
        {
            _writer?.Dispose();
            _writer = null;

            CleanupOldLogs();

            var path = GetDatedLogFilePath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(path, append: true, encoding: Encoding.UTF8)
            {
                AutoFlush = true
            };
        }
        catch
        {
        }
    }

    private void CleanupOldLogs()
    {
        try
        {
            if (_useDefaultPath)
            {
                var directory = GetDefaultLogDirectory();
                if (!Directory.Exists(directory)) return;

                var cutoffDate = DateTime.UtcNow.Date.AddDays(-_retentionDays);
                var files = Directory.GetFiles(directory, "*.log");
                foreach (var file in files)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        if (DateTime.TryParseExact(fileName, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var fileDate))
                        {
                            if (fileDate < cutoffDate)
                            {
                                File.Delete(file);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }
    }

    private static string FormatMessage(LogMessage message)
    {
        var timestamp = message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var level = message.Level.ToString().ToUpperInvariant().PadRight(7);
        var source = message.Source != null ? $"[{message.Source}] " : "";
        return $"{timestamp} {level} {source}{message.Message}";
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }
}
