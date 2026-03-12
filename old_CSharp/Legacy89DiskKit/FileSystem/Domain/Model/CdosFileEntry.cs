namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// Represents a CDOS directory entry (32 bytes)
/// </summary>
public record CdosFileEntry
{
    /// <summary>
    /// Directory entry size in bytes
    /// </summary>
    public const int EntrySize = 32;

    /// <summary>
    /// Deleted file marker
    /// </summary>
    public const byte DeletedMarker = 0xE5;

    /// <summary>
    /// Empty entry marker
    /// </summary>
    public const byte EmptyMarker = 0x00;

    /// <summary>
    /// Gets or sets the file name (8 characters, space padded)
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension (3 characters, space padded)
    /// </summary>
    public string Extension { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the file attributes
    /// </summary>
    public CdosFileAttributes Attributes { get; init; }

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public uint FileSize { get; init; }

    /// <summary>
    /// Gets or sets the starting track number
    /// </summary>
    public byte StartTrack { get; init; }

    /// <summary>
    /// Gets or sets the starting sector number
    /// </summary>
    public byte StartSector { get; init; }

    /// <summary>
    /// Gets or sets the load address for executable files
    /// </summary>
    public ushort LoadAddress { get; init; }

    /// <summary>
    /// Gets or sets the execution address for executable files
    /// </summary>
    public ushort ExecutionAddress { get; init; }

    /// <summary>
    /// Gets whether this entry represents a deleted file
    /// </summary>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets whether this entry is empty
    /// </summary>
    public bool IsEmpty { get; init; }

    /// <summary>
    /// Gets whether this is a valid file entry
    /// </summary>
    public bool IsValid => !IsDeleted && !IsEmpty && !string.IsNullOrWhiteSpace(FileName);

    /// <summary>
    /// Gets the full filename with extension
    /// </summary>
    public string FullFileName => string.IsNullOrWhiteSpace(Extension) 
        ? FileName.Trim() 
        : $"{FileName.Trim()}.{Extension.Trim()}";
}

/// <summary>
/// CDOS file attributes
/// </summary>
[Flags]
public enum CdosFileAttributes : byte
{
    /// <summary>
    /// No attributes
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Read-only file
    /// </summary>
    ReadOnly = 0x01,

    /// <summary>
    /// Archive flag
    /// </summary>
    Archive = 0x02,

    /// <summary>
    /// Hidden file
    /// </summary>
    Hidden = 0x08,

    /// <summary>
    /// System file
    /// </summary>
    System = 0x10
}