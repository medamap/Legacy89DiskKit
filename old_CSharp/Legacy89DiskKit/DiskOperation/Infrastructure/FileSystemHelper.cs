using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.DiskOperation.Infrastructure;

public static class FileSystemHelper
{
    public static bool FileExists(this IFileSystem fileSystem, string fileName)
    {
        return fileSystem.GetFile(fileName) != null;
    }
    
    public static long GetFreeSpace(this IFileSystem fileSystem)
    {
        var info = fileSystem.GetFileSystemInfo();
        return (long)info.FreeClusters * info.ClusterSize;
    }
    
    public static FileSystemType GetFileSystemType(this IFileSystem fileSystem)
    {
        // Handle ReadOnlyFileSystemWrapper by getting the inner filesystem type
        if (fileSystem.GetType().Name == "ReadOnlyFileSystemWrapper")
        {
            // Use reflection to get the inner filesystem
            var field = fileSystem.GetType().GetField("_innerFileSystem", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var innerFileSystem = field.GetValue(fileSystem) as IFileSystem;
                if (innerFileSystem != null)
                {
                    return GetFileSystemType(innerFileSystem);
                }
            }
        }
        
        return fileSystem switch
        {
            _ when fileSystem.GetType().Name.Contains("HuBasic") => FileSystemType.HuBasic,
            _ when fileSystem.GetType().Name.Contains("N88Basic") => FileSystemType.N88Basic,
            _ when fileSystem.GetType().Name.Contains("Fat12") => FileSystemType.Fat12,
            _ when fileSystem.GetType().Name.Contains("MsxDos") => FileSystemType.MsxDos,
            _ when fileSystem.GetType().Name.Contains("Cpm") => FileSystemType.Cpm,
            _ when fileSystem.GetType().Name.Contains("Cdos") => FileSystemType.Cdos,
            _ => throw new NotSupportedException($"Unknown filesystem type: {fileSystem.GetType().Name}")
        };
    }
}