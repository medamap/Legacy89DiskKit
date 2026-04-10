using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Domain.FileSystem.Model;

public sealed record XDosFileMetadata(
    XDosFileType FileType,
    ushort RawFileType,
    byte RawAttribute,
    uint TimestampRaw
) : IFileSystemEntryMetadata;
