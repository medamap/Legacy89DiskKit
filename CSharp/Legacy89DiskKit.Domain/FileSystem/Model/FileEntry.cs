namespace Legacy89DiskKit.Domain.FileSystem.Model;

public record FileEntry(
    string FileName,
    string Extension,
    long Size,
    DateTime? CreatedAt,
    DateTime? LastModifiedAt,
    ExtendedFileAttributes Attributes,
    int StartCluster = 0,
    ushort? LoadAddress = null,
    ushort? EndAddress = null,
    ushort? ExecutionAddress = null
)
{
    public string FullName => string.IsNullOrEmpty(Extension) ? FileName : $"{FileName}.{Extension}";
}
