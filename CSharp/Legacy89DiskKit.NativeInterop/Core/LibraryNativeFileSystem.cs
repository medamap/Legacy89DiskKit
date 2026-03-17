using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class LibraryNativeFileSystem : IFileSystem
{
    private readonly int _handle;
    private readonly string _fileSystemName;

    public LibraryNativeFileSystem(int handle)
    {
        _handle = handle;
        
        if (NativeLibraryImports.GetFileSystemInfo(_handle, out var info) == 0)
        {
            _fileSystemName = info.FileSystemName;
        }
        else
        {
            _fileSystemName = "Unknown";
        }
    }

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        if (NativeLibraryImports.GetFileSystemInfo(_handle, out var info) == 0)
        {
            return new DiskFileSystemInfo(
                info.FileSystemName,
                info.TotalCapacity,
                info.FreeSpace,
                info.ClusterSize,
                info.ReservedSectors,
                info.PlatformId
            );
        }
        throw new Exception("Failed to get file system info.");
    }

    public FileSystemCapabilities Capabilities 
    {
        get
        {
            var caps = FileSystemCapabilities.None;
            if (_fileSystemName == "Hu-BASIC" || _fileSystemName == "N88-BASIC")
                caps |= FileSystemCapabilities.SupportsBootArea | FileSystemCapabilities.SupportsAttributes | FileSystemCapabilities.SupportsRename;
            if (_fileSystemName == "MSX-DOS")
                caps |= FileSystemCapabilities.SupportsBootArea | FileSystemCapabilities.SupportsAttributes | FileSystemCapabilities.SupportsRename | FileSystemCapabilities.SupportsSubdirectories;
            return caps;
        }
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        int result = NativeLibraryImports.GetFilesCount(_handle, out int count);
        if (result != 0)
        {
            return Enumerable.Empty<FileEntry>();
        }

        var buffer = new NativeFileEntry[count];
        if (count > 0)
        {
            int getFilesResult = NativeLibraryImports.GetFiles(_handle, buffer, count);
            if (getFilesResult > 0)
            {
                return buffer.Take(getFilesResult).Select(e => {
                    bool isReadOnly = false;
                    bool isAscii = false;
                    
                    if (_fileSystemName == "Hu-BASIC")
                    {
                        isReadOnly = (e.Attributes & 0x40) != 0;
                        isAscii = (e.Attributes & 0x0C) != 0;
                    }
                    else if (_fileSystemName == "N88-BASIC")
                    {
                        isReadOnly = (e.Attributes & 0x40) != 0;
                        isAscii = (e.Attributes & 0x0C) != 0;
                    }
                    else if (_fileSystemName == "MSX-DOS")
                    {
                        isReadOnly = (e.Attributes & 0x01) != 0;
                    }
                    
                    return new FileEntry(
                        e.FileName,
                        e.Extension,
                        e.Size,
                        null, // CreatedAt
                        null, // LastModifiedAt
                        new ExtendedFileAttributes(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.None, (byte)e.Attributes, isAscii),
                        0, // StartCluster
                        e.LoadAddress,
                        null, // EndAddress
                        e.ExecutionAddress
                    );
                });
            }
        }
        return Enumerable.Empty<FileEntry>();
    }

    public bool FileExists(string fileName)
    {
        return GetFiles().Any(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    public byte[] ReadFile(string fileName)
    {
        var files = GetFiles().ToList();
        var file = files.FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (file == null) throw new FileNotFoundException(fileName);

        byte[] buffer = new byte[file.Size];
        int result = NativeLibraryImports.ReadFile(_handle, fileName, buffer, buffer.Length);
        if (result < 0) throw new Exception($"Failed to read file: {fileName} (Error: {result})");
        return buffer;
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
    {
        int result = NativeLibraryImports.WriteFile(_handle, fileName, data, data.Length, attributes.RawAttributes, loadAddress ?? 0, executionAddress ?? 0);
        if (result < 0) throw new Exception($"Failed to write file: {fileName} (Error: {result})");
    }

    public void CopyFile(string sourceName, string targetName)
    {
        var data = ReadFile(sourceName);
        var files = GetFiles().ToList();
        var file = files.First(f => f.FullName.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
        WriteFile(targetName, data, file.Attributes, file.LoadAddress, file.ExecutionAddress);
    }

    public void DeleteFile(string fileName)
    {
        int result = NativeLibraryImports.DeleteFile(_handle, fileName);
        if (result < 0) throw new Exception($"Failed to delete file: {fileName} (Error: {result})");
    }

    public void RenameFile(string oldName, string newName)
    {
        int result = NativeLibraryImports.RenameFile(_handle, oldName, newName);
        if (result < 0) throw new Exception($"Failed to rename file: {oldName} to {newName} (Error: {result})");
    }

    public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes)
    {
        int result = NativeLibraryImports.UpdateAttributes(_handle, fileName, attributes.RawAttributes);
        if (result < 0) throw new Exception($"Failed to update attributes: {fileName} (Error: {result})");
    }

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii)
    {
        byte raw = 0x00;
        if (_fileSystemName == "Hu-BASIC") raw = (byte)(isAscii ? 0x04 : 0x01);
        else if (_fileSystemName == "N88-BASIC") raw = (byte)(isAscii ? 0x04 : 0x01);
        else if (_fileSystemName == "MSX-DOS") raw = 0x00;
        
        return new ExtendedFileAttributes(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.None, raw, isAscii);
    }

    public void Format()
    {
        int result = NativeLibraryImports.Format(_handle);
        if (result < 0) throw new Exception($"Failed to format (Error: {result})");
    }

    private const int MaxBootAreaSize = 8192; // Large enough for any known supported boot area

    public byte[] ReadBootArea()
    {
        byte[] buffer = new byte[MaxBootAreaSize];
        int result = NativeLibraryImports.ReadBootArea(_handle, buffer, buffer.Length);
        if (result < 0) throw new Exception($"Failed to read boot area (Error: {result})");
        return buffer.Take(result).ToArray();
    }

    public void WriteBootArea(byte[] data)
    {
        int result = NativeLibraryImports.WriteBootArea(_handle, data, data.Length);
        if (result < 0) throw new Exception($"Failed to write boot area (Error: {result})");
    }

    public void Dispose()
    {
    }
}
