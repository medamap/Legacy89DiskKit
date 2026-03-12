using Legacy89DiskKit.DiskOperation.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.DiskOperation.Domain.Interface;

public interface IFileCopyService
{
    Task<FileCopyResult> CopyFileAsync(
        string sourceDiskPath,
        string destinationDiskPath,
        string fileName,
        FileCopyOptions? options = null);
    
    Task<BatchCopyResult> CopyFilesAsync(
        string sourceDiskPath,
        string destinationDiskPath,
        IEnumerable<string> fileNames,
        FileCopyOptions? options = null);
    
    bool CanCopyFile(
        IFileSystem sourceFileSystem,
        IFileSystem destinationFileSystem,
        string fileName);
    
    long CalculateRequiredSpace(
        IFileSystem sourceFileSystem,
        string fileName);
}