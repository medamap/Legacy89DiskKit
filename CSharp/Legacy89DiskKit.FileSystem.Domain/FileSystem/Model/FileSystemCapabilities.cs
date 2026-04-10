namespace Legacy89DiskKit.Domain.FileSystem.Model;

[Flags]
public enum FileSystemCapabilities
{
    None = 0,
    SupportsSubdirectories = 1 << 0,
    SupportsBootArea = 1 << 1,
    SupportsAttributes = 1 << 2,
    SupportsInternalCopy = 1 << 3,
    SupportsRename = 1 << 4,
    FixedFileNameLength = 1 << 5
}
