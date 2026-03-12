namespace Legacy89DiskKit.Domain.FileSystem.Model;

public enum HuBasicFileType
{
    Unknown,
    Binary,
    Basic,
    Ascii
}

public sealed record HuBasicFileMetadata(
    HuBasicFileType FileType,
    bool HasPassword,
    bool IsHidden,
    bool IsVerify,
    bool IsWriteProtected,
    bool IsDirectory,
    ushort RecordedSize,
    ushort? LoadAddress,
    ushort? ExecutionAddress,
    int StartCluster,
    byte RawModeByte,
    byte PasswordByte = 0x20
) : IFileSystemEntryMetadata;

public sealed record HuBasicBootRecordInfo(
    byte BootFlag,
    string Name,
    string Extension,
    bool HasPassword,
    ushort Size,
    ushort LoadAddress,
    ushort ExecutionAddress,
    ushort StartRecord
);
