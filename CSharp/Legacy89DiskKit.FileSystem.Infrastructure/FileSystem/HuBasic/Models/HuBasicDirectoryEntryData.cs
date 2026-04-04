namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;

public sealed record HuBasicDirectoryEntryData(
    byte ModeByte,
    byte PasswordByte,
    byte[] RawFileName,
    byte[] RawExtension,
    string FileName,
    string Extension,
    ushort RecordedSize,
    ushort LoadAddress,
    ushort ExecutionAddress,
    DateTime ModifiedDate,
    int StartCluster
);
