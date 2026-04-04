using Legacy89DiskKit.FileSystem.Domain.Model.XDos;

namespace Legacy89DiskKit.FileSystem.Domain.Model;

public sealed record XDosFileMetadata(
    XDosFileType FileType,
    ushort RawFileType,
    byte RawAttribute,
    uint TimestampRaw
) : IFileSystemEntryMetadata;
