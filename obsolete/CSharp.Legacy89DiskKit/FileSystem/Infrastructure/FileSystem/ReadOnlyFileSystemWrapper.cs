using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

public class ReadOnlyFileSystemWrapper : IFileSystem
{
    private readonly IFileSystem _innerFileSystem;

    public ReadOnlyFileSystemWrapper(IFileSystem innerFileSystem)
    {
        _innerFileSystem = innerFileSystem ?? throw new ArgumentNullException(nameof(innerFileSystem));
    }

    // IFileSystem プロパティ
    public IDiskContainer DiskContainer => _innerFileSystem.DiskContainer;
    public bool IsFormatted => _innerFileSystem.IsFormatted;

    // 読み取り専用操作：そのまま委譲
    public IEnumerable<FileEntry> GetFiles() => _innerFileSystem.GetFiles();
    
    public FileEntry? GetFile(string fileName) => _innerFileSystem.GetFile(fileName);
    
    public byte[] ReadFile(string fileName) => _innerFileSystem.ReadFile(fileName);
    
    public byte[] ReadFile(string fileName, bool allowPartialRead) => 
        _innerFileSystem.ReadFile(fileName, allowPartialRead);
    
    public BootSector GetBootSector() => _innerFileSystem.GetBootSector();
    
    public HuBasicFileSystemInfo GetFileSystemInfo() => _innerFileSystem.GetFileSystemInfo();

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

    public void WriteBootSector(BootSector bootSector)
    {
        throw new InvalidOperationException(
            "Write operations not allowed on read-only filesystem. " +
            "Use OpenFileSystem() with explicit filesystem type for write access.");
    }

    public void Dispose()
    {
        // IFileSystemインターフェースにDisposeメソッドがないため、何もしない
        // 必要に応じてIDiskContainerのDisposeを呼び出し
        _innerFileSystem?.DiskContainer?.Dispose();
    }
}