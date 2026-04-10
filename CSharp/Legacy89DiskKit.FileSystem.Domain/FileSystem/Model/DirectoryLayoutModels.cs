namespace Legacy89DiskKit.Domain.FileSystem.Model;

public enum DirectoryLayoutItemKind
{
    FileEntry,
    VirtualLabel
}

public enum DirectorySortBy
{
    Name,
    Extension,
    Type
}

public enum DirectoryLayoutOperationType
{
    Move,
    InsertLabel,
    Sort,
    Delete
}

public sealed record VirtualDirectoryLabelEntry(
    string FileName,
    string Extension,
    byte RawModeByte,
    byte PasswordByte,
    ushort Size,
    ushort LoadAddress,
    ushort EndAddress,
    ushort ExecutionAddress,
    int StartCluster
);

public sealed record DirectoryLayoutItem(
    string Id,
    int Order,
    DirectoryLayoutItemKind Kind,
    string DisplayName,
    FileEntry? Entry = null,
    VirtualDirectoryLabelEntry? VirtualLabel = null
);

public sealed record DirectoryEntryLayout(
    string FileSystemName,
    IReadOnlyList<DirectoryLayoutItem> Items
);

public sealed record DirectoryLayoutOperation(
    DirectoryLayoutOperationType OperationType,
    string SourceId,
    string? TargetId = null,
    DirectorySortBy? SortBy = null,
    VirtualDirectoryLabelEntry? Label = null
);
