using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using DomainAttr = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Cpm;

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

    public byte[] ReadBootArea()
    {
        try
        {
            // For CP/M on PC-8801 etc., the boot area is typically Track 0.
            // We return Sector 1 as a representative boot sector.
            return _diskContainer.ReadSector(0, 0, 1);
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    public void WriteBootArea(byte[] data) { }

    public void Dispose() { }
}
