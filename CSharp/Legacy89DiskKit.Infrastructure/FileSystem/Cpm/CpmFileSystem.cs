using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using DomainAttr = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.Cpm;

public class CpmFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;

    public CpmFileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer;
    }

    public FileSystemCapabilities Capabilities => 
        FileSystemCapabilities.SupportsAttributes | 
        FileSystemCapabilities.SupportsRename | 
        FileSystemCapabilities.FixedFileNameLength;

    public DiskFileSystemInfo GetFileSystemInfo() => new DiskFileSystemInfo("CP/M", 0, 0, 0, 0);

    public IEnumerable<FileEntry> GetFiles() => Enumerable.Empty<FileEntry>();

    public bool FileExists(string fileName) => false;

    public byte[] ReadFile(string fileName) => Array.Empty<byte>();

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null) { }

    public void DeleteFile(string fileName) { }

    public void RenameFile(string oldName, string newName) { }

    public void CopyFile(string sourceName, string targetName) { }

    public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes) { }
    
    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii) => new ExtendedFileAttributes(DomainAttr.None, 0, isAscii, "CPM");

    public void Format() { }

    public byte[] ReadBootArea() => Array.Empty<byte>();

    public void WriteBootArea(byte[] data) { }

    public void Dispose() { }
}
