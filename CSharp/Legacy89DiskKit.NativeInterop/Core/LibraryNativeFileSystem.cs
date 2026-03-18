using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Interface.Layout;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.NativeInterop.Types;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Application;
using System.Text;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class LibraryNativeFileSystem : IFileSystem, IDirectoryLayoutProvider
{
    private readonly int _handle;
    private readonly string _fileSystemName;
    private readonly ICharacterEncoder _encoder;

    public LibraryNativeFileSystem(int handle)
    {
        _handle = handle;
        
        var fsInfo = GetFileSystemInfo();
        _fileSystemName = fsInfo.FileSystemName;
        _encoder = Legacy89DiskKitApplication.ResolveEncoder(fsInfo);
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

                    var rawFileName = e.FileName.TakeWhile(b => b != 0).ToArray();
                    var rawExtension = e.Extension.TakeWhile(b => b != 0).ToArray();
                    var fileName = _encoder.DecodeText(rawFileName);
                    var extension = _encoder.DecodeText(rawExtension);
                    
                    return new FileEntry(
                        fileName,
                        extension,
                        e.Size,
                        null,
                        null,
                        new ExtendedFileAttributes(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.None, (byte)e.Attributes, isAscii),
                        0,
                        e.LoadAddress,
                        null,
                        e.ExecutionAddress,
                        rawFileName,
                        rawExtension
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
        var encodedName = _encoder.EncodeText(fileName).Append((byte)0).ToArray();
        int result = NativeLibraryImports.ReadFile(_handle, encodedName, buffer, buffer.Length);
        if (result < 0) throw new Exception($"Failed to read file: {fileName} (Error: {result})");
        return buffer;
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
    {
        var encodedName = _encoder.EncodeText(fileName).Append((byte)0).ToArray();
        int result = NativeLibraryImports.WriteFile(_handle, encodedName, data, data.Length, attributes.RawAttributes, loadAddress ?? 0, executionAddress ?? 0);
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
        var encodedName = _encoder.EncodeText(fileName).Append((byte)0).ToArray();
        int result = NativeLibraryImports.DeleteFile(_handle, encodedName);
        if (result < 0) throw new Exception($"Failed to delete file: {fileName} (Error: {result})");
    }

    public void RenameFile(string oldName, string newName)
    {
        var encodedOldName = _encoder.EncodeText(oldName).Append((byte)0).ToArray();
        var encodedNewName = _encoder.EncodeText(newName).Append((byte)0).ToArray();
        int result = NativeLibraryImports.RenameFile(_handle, encodedOldName, encodedNewName);
        if (result < 0) throw new Exception($"Failed to rename file: {oldName} to {newName} (Error: {result})");
    }

    public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes)
    {
        var encodedName = _encoder.EncodeText(fileName).Append((byte)0).ToArray();
        int result = NativeLibraryImports.UpdateAttributes(_handle, encodedName, attributes.RawAttributes);
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

    private const int MaxBootAreaSize = 8192;

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

    public DirectoryEntryLayout ReadDirectoryLayout()
    {
        var buffer = new NativeDirectoryLayoutItem[256]; // Assuming max 256 entries
        int count = NativeLibraryImports.ReadDirectoryLayout(_handle, buffer, buffer.Length);
        if (count < 0) throw new Exception($"Failed to read directory layout (Error: {count})");

        var files = GetFiles().ToList();
        var items = new List<DirectoryLayoutItem>(count);

        for (int i = 0; i < count; i++)
        {
            var e = buffer[i];
            string id = Encoding.ASCII.GetString(e.Id.TakeWhile(b => b != 0).ToArray());
            string displayName = _encoder.DecodeText(e.DisplayName.TakeWhile(b => b != 0).ToArray());
            var kind = (DirectoryLayoutItemKind)e.Kind;

            FileEntry? entry = null;
            if (kind == DirectoryLayoutItemKind.FileEntry)
            {
                entry = files.FirstOrDefault(f => f.FullName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            }

            items.Add(new DirectoryLayoutItem(id, e.Order, kind, displayName, entry, null));
        }

        return new DirectoryEntryLayout(_fileSystemName, items);
    }

    public void ApplyDirectoryLayout(DirectoryEntryLayout layout)
    {
        var items = layout.Items;
        var buffer = new NativeDirectoryLayoutItem[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            buffer[i] = new NativeDirectoryLayoutItem
            {
                Id = CreatePaddedBytes(Encoding.ASCII.GetBytes(item.Id), 64),
                StableId = new byte[64],
                DisplayName = CreatePaddedBytes(_encoder.EncodeText(item.DisplayName), 64),
                Order = item.Order,
                Kind = (int)item.Kind
            };
        }

        int result = NativeLibraryImports.ApplyDirectoryLayout(_handle, buffer, buffer.Length);
        if (result < 0) throw new Exception($"Failed to apply directory layout (Error: {result})");
    }

    private static byte[] CreatePaddedBytes(byte[] source, int length)
    {
        var result = new byte[length];
        Array.Copy(source, result, Math.Min(source.Length, length));
        return result;
    }

    public void Dispose()
    {
    }
}
