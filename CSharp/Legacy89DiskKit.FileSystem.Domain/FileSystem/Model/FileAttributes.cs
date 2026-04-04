namespace Legacy89DiskKit.FileSystem.Domain.Model;

[Flags]
public enum FileAttributes
{
    None = 0,
    ReadOnly = 1 << 0,
    Hidden = 1 << 1,
    System = 1 << 2,
    Directory = 1 << 3,
    Archive = 1 << 4,
    WriteProtect = 1 << 5, // Vintage systems specific
}

public record ExtendedFileAttributes(
    FileAttributes StandardAttributes,
    byte RawAttributes, // OS-specific raw byte
    bool IsAscii,       // Specific to Hu-BASIC/N88-BASIC types
    string OsSpecificInfo = ""
);
