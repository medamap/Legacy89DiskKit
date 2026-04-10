namespace Legacy89DiskKit.DiskOperation.Domain.Model;

public class FileCopyOptions
{
    public bool OverwriteExisting { get; init; }
    public bool PreserveAttributes { get; init; } = true;
    public bool ValidateAfterCopy { get; init; } = true;
    public ConflictResolution ConflictResolution { get; init; } = ConflictResolution.AutoRename;
    public IProgress<FileCopyProgress>? Progress { get; init; }
}

public enum ConflictResolution
{
    Skip,
    Overwrite,
    AutoRename,
    Error
}

public class FileCopyProgress
{
    public string FileName { get; init; } = string.Empty;
    public long BytesTransferred { get; init; }
    public long TotalBytes { get; init; }
    public double PercentComplete => TotalBytes > 0 ? (double)BytesTransferred / TotalBytes * 100 : 0;
}