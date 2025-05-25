using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

public interface IFileSystem
{
    IDiskContainer DiskContainer { get; }
    bool IsFormatted { get; }
    
    void Format();
    
    IEnumerable<FileEntry> GetFiles();
    FileEntry? GetFile(string fileName);
    
    byte[] ReadFile(string fileName);
    byte[] ReadFile(string fileName, bool allowPartialRead);
    void WriteFile(string fileName, byte[] data, bool isText = false, ushort loadAddress = 0, ushort execAddress = 0);
    void DeleteFile(string fileName);
    
    BootSector GetBootSector();
    void WriteBootSector(BootSector bootSector);
    
    HuBasicFileSystemInfo GetFileSystemInfo();
}

public record FileEntry(
    string FileName,
    string Extension,
    HuBasicFileMode Mode,
    HuBasicFileAttributes Attributes,
    int Size,
    ushort LoadAddress,
    ushort ExecuteAddress,
    DateTime ModifiedDate,
    bool IsProtected);

public record HuBasicFileAttributes(
    bool IsDirectory,
    bool IsReadOnly,
    bool IsVerify,
    bool IsHidden,
    bool IsBinary,
    bool IsBasic,
    bool IsAscii);

public record BootSector(
    bool IsBootable,
    string Label,
    string Extension,
    int Size,
    ushort LoadAddress,
    ushort ExecuteAddress,
    DateTime ModifiedDate,
    ushort StartSector);

public record HuBasicFileSystemInfo(
    int TotalClusters,
    int FreeClusters,
    int ClusterSize,
    int SectorSize);

public enum HuBasicFileMode : byte
{
    Deleted = 0x00,
    EndOfDirectory = 0xFF,
    Binary = 0x01,
    Basic = 0x02,
    Ascii = 0x04
}