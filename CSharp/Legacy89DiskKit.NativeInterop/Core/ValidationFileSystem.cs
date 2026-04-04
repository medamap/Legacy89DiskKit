using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class ValidationFileSystem : IFileSystem
{
    private readonly IFileSystem _reference;
    private readonly IFileSystem _target;

    public ValidationFileSystem(IFileSystem reference, IFileSystem target)
    {
        _reference = reference;
        _target = target;
    }

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        var refInfo = _reference.GetFileSystemInfo();
        var targetInfo = _target.GetFileSystemInfo();

        if (refInfo.FileSystemName != targetInfo.FileSystemName)
        {
            throw new Exception($"Validation Error: FileSystemName mismatch. Ref: {refInfo.FileSystemName}, Target: {targetInfo.FileSystemName}");
        }

        return refInfo;
    }

    public FileSystemCapabilities Capabilities => _reference.Capabilities;

    public IEnumerable<FileEntry> GetFiles()
    {
        var refFiles = _reference.GetFiles().OrderBy(f => f.FullName).ToList();
        var targetFiles = _target.GetFiles().OrderBy(f => f.FullName).ToList();

        if (refFiles.Count != targetFiles.Count)
        {
            throw new Exception($"Validation Error: File count mismatch. Ref: {refFiles.Count}, Target: {targetFiles.Count}");
        }

        for (int i = 0; i < refFiles.Count; i++)
        {
            var r = refFiles[i];
            var t = targetFiles[i];

            if (r.FullName != t.FullName)
                throw new Exception($"Validation Error: File name mismatch at index {i}. Ref: {r.FullName}, Target: {t.FullName}");
            
            if (r.Size != t.Size)
                throw new Exception($"Validation Error: File size mismatch for {r.FullName}. Ref: {r.Size}, Target: {t.Size}");

            // Note: Attributes and Addresses might have subtle implementation differences, 
            // but we expect them to match for HuBasic/N88.
            if (r.Attributes.RawAttributes != t.Attributes.RawAttributes)
                Console.WriteLine($"Validation Warning: RawAttributes mismatch for {r.FullName}. Ref: {r.Attributes.RawAttributes}, Target: {t.Attributes.RawAttributes}");
        }

        return refFiles;
    }

    public bool FileExists(string fileName)
    {
        var refResult = _reference.FileExists(fileName);
        var targetResult = _target.FileExists(fileName);

        if (refResult != targetResult)
        {
            throw new Exception($"Validation Error: FileExists mismatch for {fileName}. Ref: {refResult}, Target: {targetResult}");
        }

        return refResult;
    }

    public byte[] ReadFile(string fileName)
    {
        var refData = _reference.ReadFile(fileName);
        var targetData = _target.ReadFile(fileName);

        if (!refData.SequenceEqual(targetData))
        {
            throw new Exception($"Validation Error: ReadFile content mismatch for {fileName}. Size Ref: {refData.Length}, Target: {targetData.Length}");
        }

        return refData;
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
    {
        _reference.WriteFile(fileName, data, attributes, loadAddress, executionAddress);
        _target.WriteFile(fileName, data, attributes, loadAddress, executionAddress);
    }

    public void CopyFile(string sourceName, string targetName)
    {
        _reference.CopyFile(sourceName, targetName);
        _target.CopyFile(sourceName, targetName);
    }

    public void DeleteFile(string fileName)
    {
        _reference.DeleteFile(fileName);
        _target.DeleteFile(fileName);
    }

    public void RenameFile(string oldName, string newName)
    {
        _reference.RenameFile(oldName, newName);
        _target.RenameFile(oldName, newName);
    }

    public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes)
    {
        _reference.UpdateAttributes(fileName, attributes);
        _target.UpdateAttributes(fileName, attributes);
    }

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii) => _reference.CreateDefaultAttributes(isAscii);

    public void Format()
    {
        _reference.Format();
        _target.Format();
    }

    public byte[] ReadBootArea()
    {
        var refData = _reference.ReadBootArea();
        var targetData = _target.ReadBootArea();

        if (!refData.SequenceEqual(targetData))
        {
            Console.WriteLine($"Validation Warning: BootArea mismatch.");
        }

        return refData;
    }

    public void WriteBootArea(byte[] data)
    {
        _reference.WriteBootArea(data);
        _target.WriteBootArea(data);
    }

    public void Dispose()
    {
        _reference.Dispose();
        _target.Dispose();
    }
}
