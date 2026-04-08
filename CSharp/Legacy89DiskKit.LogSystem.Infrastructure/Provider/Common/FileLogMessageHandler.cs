using Legacy89DiskKit.LogSystem.Domain;
using Legacy89DiskKit.LogSystem.Domain.Provider;

namespace Legacy89DiskKit.LogSystem.Infrastructure.Provider.Common;

public class FileLogMessageHandler : ILogMessageHandler, IDisposable
{
    private readonly string _logFilePath;
    private readonly object _lock = new();
    private long _currentFileSize;
    private readonly long _maxFileSize;
    private readonly int _maxRetainDays;
    private readonly bool _rotationEnabled;

    public bool IsEnabled { get; set; } = true;
    public LogType MinimumLevel { get; set; } = LogType.Info;

    public FileLogMessageHandler(string logFilePath, bool rotationEnabled = false, long maxFileSize = 10 * 1024 * 1024, int maxRetainDays = 7)
    {
        _logFilePath = logFilePath;
        _rotationEnabled = rotationEnabled;
        _maxFileSize = maxFileSize;
        _maxRetainDays = maxRetainDays;
    }

    public void Handle(LogMessage message)
    {
        if (!IsEnabled || message.Type < MinimumLevel) return;

        var formatted = FormatMessage(message);
        
        lock (_lock)
        {
            try
            {
                var actualLogPath = _logFilePath;
                if (_rotationEnabled)
                {
                    actualLogPath = GetDatedLogFilePath();
                    RotateIfExpired(actualLogPath);
                }

                var dir = Path.GetDirectoryName(actualLogPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.AppendAllText(actualLogPath, formatted + Environment.NewLine);
            }
            catch
            {
            }
        }
    }

    private string GetDatedLogFilePath()
    {
        var dir = Path.GetDirectoryName(_logFilePath) ?? ".";
        var name = Path.GetFileNameWithoutExtension(_logFilePath);
        var ext = Path.GetExtension(_logFilePath);
        var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
        var currentDatePath = Path.Combine(dir, $"{name}-{dateStr}{ext}");
        
        if (File.Exists(_logFilePath) && _logFilePath != currentDatePath)
        {
            try { File.Move(_logFilePath, currentDatePath); } catch { }
        }
        
        return currentDatePath;
    }

    private void RotateIfExpired(string actualLogPath)
    {
        if (File.Exists(actualLogPath))
        {
            var fileInfo = new FileInfo(actualLogPath);
            _currentFileSize = fileInfo.Length;
        }

        if (_currentFileSize >= _maxFileSize)
        {
            RotateLogFile(actualLogPath);
            _currentFileSize = 0;
        }

        CleanupOldFiles();
    }

    private void RotateLogFile(string actualLogPath)
    {
        try
        {
            if (!File.Exists(actualLogPath)) return;
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var dir = Path.GetDirectoryName(actualLogPath) ?? ".";
            var name = Path.GetFileNameWithoutExtension(actualLogPath);
            var ext = Path.GetExtension(actualLogPath);
            var rotated = Path.Combine(dir, $"{name}_{timestamp}{ext}");
            
            File.Move(actualLogPath, rotated);
        }
        catch
        {
        }
    }

    private void CleanupOldFiles()
    {
        if (!_rotationEnabled) return;
        
        try
        {
            var dir = Path.GetDirectoryName(_logFilePath);
            if (string.IsNullOrEmpty(dir)) return;
            
            var name = Path.GetFileNameWithoutExtension(_logFilePath);
            var ext = Path.GetExtension(_logFilePath);
            var cutoff = DateTime.Now.AddDays(-_maxRetainDays);
            
            foreach (var file in Directory.GetFiles(dir, $"{name}*{ext}"))
            {
                var info = new FileInfo(file);
                if (info.CreationTime < cutoff)
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
        catch
        {
        }
    }

    private static string FormatMessage(LogMessage message)
    {
        return $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] [{message.Type}] {message.Message}";
    }

    public void Dispose()
    {
    }
}