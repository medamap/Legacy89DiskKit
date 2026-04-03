namespace Legacy89DiskKit.DiskOperation.Domain.Model;

public class FileCopyResult
{
    public string SourceFileName { get; init; } = string.Empty;
    public string DestinationFileName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public long BytesCopied { get; init; }
    public FileNameConversionType ConversionType { get; init; }
    public TimeSpan Duration { get; init; }
}

public class BatchCopyResult
{
    public int TotalFiles { get; init; }
    public int SuccessfulFiles { get; init; }
    public int FailedFiles { get; init; }
    public long TotalBytesCopied { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public List<FileCopyResult> FileResults { get; init; } = new();
}

public enum FileNameConversionType
{
    None,
    Truncated,
    Renamed,
    TruncatedAndRenamed
}