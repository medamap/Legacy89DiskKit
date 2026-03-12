namespace Legacy89DiskKit.DiskOperation.Domain.Exception;

public class DiskOperationException : System.Exception
{
    public DiskOperationException(string message) : base(message)
    {
    }

    public DiskOperationException(string message, System.Exception innerException) 
        : base(message, innerException)
    {
    }
}

public class InsufficientDiskSpaceException : DiskOperationException
{
    public long RequiredBytes { get; }
    public long AvailableBytes { get; }

    public InsufficientDiskSpaceException(long requiredBytes, long availableBytes)
        : base($"Insufficient disk space. Required: {requiredBytes} bytes, Available: {availableBytes} bytes")
    {
        RequiredBytes = requiredBytes;
        AvailableBytes = availableBytes;
    }
}

public class FileNameConversionException : DiskOperationException
{
    public string OriginalFileName { get; }

    public FileNameConversionException(string originalFileName, string message)
        : base($"Failed to convert file name '{originalFileName}': {message}")
    {
        OriginalFileName = originalFileName;
    }
}