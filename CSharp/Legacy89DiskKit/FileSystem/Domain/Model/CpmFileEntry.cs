namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// Represents a CP/M directory entry
/// </summary>
public record CpmFileEntry
{
    /// <summary>
    /// User number (0-15, 0xE5 = deleted)
    /// </summary>
    public byte UserNumber { get; init; }

    /// <summary>
    /// File name (8 characters, space padded)
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// File extension (3 characters, space padded)
    /// </summary>
    public string Extension { get; init; } = string.Empty;

    /// <summary>
    /// Extent number (low byte)
    /// </summary>
    public byte ExtentLow { get; init; }

    /// <summary>
    /// Extent number (high byte)
    /// </summary>
    public byte ExtentHigh { get; init; }

    /// <summary>
    /// Number of 128-byte records in this extent
    /// </summary>
    public byte RecordCount { get; init; }

    /// <summary>
    /// Allocation block numbers (8 or 16 entries depending on DPB)
    /// </summary>
    public ushort[] AllocationBlocks { get; init; } = Array.Empty<ushort>();

    /// <summary>
    /// Read-only file attribute
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// System file attribute
    /// </summary>
    public bool IsSystem { get; init; }

    /// <summary>
    /// Archive file attribute
    /// </summary>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Gets whether this entry represents a deleted file
    /// </summary>
    public bool IsDeleted => UserNumber == 0xE5;

    /// <summary>
    /// Gets whether this entry is empty (never used)
    /// </summary>
    public bool IsEmpty => UserNumber == 0xFF;

    /// <summary>
    /// Gets whether this is a valid file entry
    /// </summary>
    public bool IsValid => !IsDeleted && !IsEmpty && UserNumber <= 15 && !string.IsNullOrWhiteSpace(FileName.Trim());

    /// <summary>
    /// Gets the full filename with extension
    /// </summary>
    public string FullFileName => $"{FileName.Trim()}.{Extension.Trim()}".TrimEnd('.');

    /// <summary>
    /// Gets the extent number as a 16-bit value
    /// </summary>
    public int ExtentNumber => (ExtentHigh << 8) | ExtentLow;

    /// <summary>
    /// Directory entry size in bytes
    /// </summary>
    public const int EntrySize = 32;

    /// <summary>
    /// Maximum user number
    /// </summary>
    public const byte MaxUserNumber = 15;

    /// <summary>
    /// Deleted file marker
    /// </summary>
    public const byte DeletedMarker = 0xE5;

    /// <summary>
    /// Empty entry marker
    /// </summary>
    public const byte EmptyMarker = 0xFF;
}