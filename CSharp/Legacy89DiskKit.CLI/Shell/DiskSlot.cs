using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.CLI.Shell;

public class DiskSlot
{
    public int SlotNumber { get; }
    public string? DiskPath { get; private set; }
    public IDiskContainer? Container { get; private set; }
    public IFileSystem? FileSystem { get; private set; }
    public string? FileSystemType { get; private set; }
    public bool IsEmpty => Container == null;

    public DiskSlot(int slotNumber)
    {
        SlotNumber = slotNumber;
    }

    public void Mount(string diskPath, IDiskContainer container, IFileSystem fileSystem, string fileSystemType)
    {
        Unmount();
        
        DiskPath = diskPath;
        Container = container;
        FileSystem = fileSystem;
        FileSystemType = fileSystemType;
    }

    public void Unmount()
    {
        Container?.Dispose();
        Container = null;
        FileSystem = null;
        DiskPath = null;
        FileSystemType = null;
    }

    public string GetDisplayName()
    {
        if (IsEmpty)
            return "[Empty]";
        
        var diskName = Path.GetFileName(DiskPath);
        return $"{diskName}/{FileSystemType}";
    }

    public string GetStatusInfo()
    {
        if (IsEmpty)
            return "[Empty]";
            
        var files = FileSystem?.GetFiles()?.Count() ?? 0;
        var diskType = Container?.DiskType.ToString() ?? "Unknown";
        return $"{GetDisplayName()} ({diskType}) - {files} files";
    }
}