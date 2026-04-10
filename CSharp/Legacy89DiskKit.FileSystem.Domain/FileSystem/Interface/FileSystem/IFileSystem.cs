using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

public interface IFileSystem : IDisposable
{
    DiskFileSystemInfo GetFileSystemInfo();
    FileSystemCapabilities Capabilities { get; }
    
    // File Operations
    IEnumerable<FileEntry> GetFiles();
    bool FileExists(string fileName);
    byte[] ReadFile(string fileName);
    void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null);
    void DeleteFile(string fileName);
    void RenameFile(string oldName, string newName);
    void CopyFile(string sourceName, string targetName);
    void UpdateAttributes(string fileName, ExtendedFileAttributes attributes);

    /// <summary>
    /// Creates a default attribute set for a new file based on the file system's conventions.
    /// </summary>
    ExtendedFileAttributes CreateDefaultAttributes(bool isAscii);
    
    // Format / System
    void Format();
    byte[] ReadBootArea();
    void WriteBootArea(byte[] data);
}
