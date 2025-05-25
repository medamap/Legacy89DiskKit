using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

public class ReadOnlyFileSystemWrapper : IFileSystem
{
    private readonly IFileSystem _innerFileSystem;

    public ReadOnlyFileSystemWrapper(IFileSystem innerFileSystem)
    {
        _innerFileSystem = innerFileSystem ?? throw new ArgumentNullException(nameof(innerFileSystem));
    }

    // 読み取り専用操作：そのまま委譲
    public IEnumerable<FileEntry> ListFiles() => _innerFileSystem.ListFiles();
    
    public byte[] ReadFile(string fileName, bool allowPartialRead = false) => 
        _innerFileSystem.ReadFile(fileName, allowPartialRead);
    
    public BootSectorInfo ReadBootSector() => _innerFileSystem.ReadBootSector();

    // 書き込み操作：すべて禁止
    public void Format()
    {
        throw new InvalidOperationException(
            "Write operations not allowed on read-only filesystem. " +
            "Use OpenFileSystem() with explicit filesystem type for write access.");
    }

    public void WriteFile(string fileName, byte[] data, bool isText = false, ushort loadAddress = 0, ushort execAddress = 0)
    {
        throw new InvalidOperationException(
            "Write operations not allowed on read-only filesystem. " +
            "Use OpenFileSystem() with explicit filesystem type for write access.");
    }

    public void DeleteFile(string fileName)
    {
        throw new InvalidOperationException(
            "Write operations not allowed on read-only filesystem. " +
            "Use OpenFileSystem() with explicit filesystem type for write access.");
    }

    public void WriteBootSector(string label, byte[] bootCode)
    {
        throw new InvalidOperationException(
            "Write operations not allowed on read-only filesystem. " +
            "Use OpenFileSystem() with explicit filesystem type for write access.");
    }

    public void Dispose()
    {
        _innerFileSystem?.Dispose();
    }
}