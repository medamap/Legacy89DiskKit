using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskOperation.Domain.Exception;
using Legacy89DiskKit.DiskOperation.Domain.Interface;
using Legacy89DiskKit.DiskOperation.Domain.Model;
using Legacy89DiskKit.DiskOperation.Infrastructure;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using System.Diagnostics;

namespace Legacy89DiskKit.DiskOperation.Application;

public class FileCopyService : IFileCopyService
{
    private readonly IDiskContainerFactory _diskContainerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly IFileNameConverter _fileNameConverter;

    public FileCopyService(
        IDiskContainerFactory diskContainerFactory,
        IFileSystemFactory fileSystemFactory,
        IFileNameConverter fileNameConverter)
    {
        _diskContainerFactory = diskContainerFactory;
        _fileSystemFactory = fileSystemFactory;
        _fileNameConverter = fileNameConverter;
    }

    public Task<FileCopyResult> CopyFileAsync(
        string sourceDiskPath,
        string destinationDiskPath,
        string fileName,
        FileCopyOptions? options = null)
    {
        options ??= new FileCopyOptions();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var sourceContainer = _diskContainerFactory.OpenDiskImage(sourceDiskPath, true);
            using var destContainer = _diskContainerFactory.OpenDiskImage(destinationDiskPath, false);

            var sourceFs = _fileSystemFactory.OpenFileSystemReadOnly(sourceContainer);
            var destFs = _fileSystemFactory.OpenFileSystem(destContainer, _fileSystemFactory.GuessFileSystemType(destContainer));

            var sourceFile = sourceFs.GetFiles().FirstOrDefault(f =>
                string.Equals($"{f.FileName}.{f.Extension}".TrimEnd('.'), fileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
                
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file '{fileName}' not found");
            }
            
            // Use the actual filename from the file system
            var actualFileName = string.IsNullOrEmpty(sourceFile.Extension) 
                ? sourceFile.FileName 
                : $"{sourceFile.FileName}.{sourceFile.Extension}";

            if (!CanCopyFile(sourceFs, destFs, fileName))
            {
                throw new DiskOperationException($"Cannot copy file '{fileName}' between these file systems");
            }

            var fileData = sourceFs.ReadFile(actualFileName);
            var fileSize = fileData.Length;
            var requiredSpace = CalculateRequiredSpace(destFs, actualFileName, fileSize);
            var availableSpace = destFs.GetFreeSpace();

            if (availableSpace < requiredSpace)
            {
                throw new InsufficientDiskSpaceException(requiredSpace, availableSpace);
            }

            var existingFiles = destFs.GetFiles().Select(f => f.FileName);
            var destFileName = _fileNameConverter.ConvertFileName(
                actualFileName,
                sourceFs.GetFileSystemType(),
                destFs.GetFileSystemType(),
                existingFiles);

            var conversionResult = _fileNameConverter.AnalyzeConversion(
                actualFileName,
                sourceFs.GetFileSystemType(),
                destFs.GetFileSystemType());

            if (destFs.FileExists(destFileName))
            {
                switch (options.ConflictResolution)
                {
                    case ConflictResolution.Skip:
                        return Task.FromResult(new FileCopyResult
                        {
                            SourceFileName = fileName,
                            DestinationFileName = destFileName,
                            Success = false,
                            ErrorMessage = "File already exists",
                            Duration = stopwatch.Elapsed
                        });

                    case ConflictResolution.Error:
                        throw new DiskOperationException($"Destination file '{destFileName}' already exists");

                    case ConflictResolution.Overwrite:
                        destFs.DeleteFile(destFileName);
                        break;

                    case ConflictResolution.AutoRename:
                        existingFiles = destFs.GetFiles().Select(f => f.FileName);
                        destFileName = _fileNameConverter.ConvertFileName(
                            actualFileName,
                            sourceFs.GetFileSystemType(),
                            destFs.GetFileSystemType(),
                            existingFiles);
                        break;
                }
            }

            long bytesCopied = fileData.Length;

            options.Progress?.Report(new FileCopyProgress
            {
                FileName = actualFileName,
                BytesTransferred = 0,
                TotalBytes = bytesCopied
            });

            destFs.WriteFile(destFileName, fileData);

            if (options.PreserveAttributes)
            {
                var sourceEntry = sourceFile;
                if (sourceEntry.Attributes.IsReadOnly && destFs.GetFileSystemType() != FileSystemType.N88Basic)
                {
                    // Note: FileEntry is immutable, we cannot modify attributes after creation
                    // This would need to be handled at the filesystem level
                }
            }

            options.Progress?.Report(new FileCopyProgress
            {
                FileName = actualFileName,
                BytesTransferred = bytesCopied,
                TotalBytes = bytesCopied
            });

            if (options.ValidateAfterCopy)
            {
                var copiedData = destFs.ReadFile(destFileName);
                if (!fileData.SequenceEqual(copiedData))
                {
                    throw new DiskOperationException("File validation failed after copy");
                }
            }

            stopwatch.Stop();
            return Task.FromResult(new FileCopyResult
            {
                SourceFileName = fileName,
                DestinationFileName = destFileName,
                Success = true,
                BytesCopied = bytesCopied,
                ConversionType = conversionResult.ConversionType,
                Duration = stopwatch.Elapsed
            });
        }
        catch (Exception ex) when (ex is not DiskOperationException)
        {
            stopwatch.Stop();
            return Task.FromResult(new FileCopyResult
            {
                SourceFileName = fileName,
                DestinationFileName = string.Empty,
                Success = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed
            });
        }
    }

    public async Task<BatchCopyResult> CopyFilesAsync(
        string sourceDiskPath,
        string destinationDiskPath,
        IEnumerable<string> fileNames,
        FileCopyOptions? options = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<FileCopyResult>();
        long totalBytes = 0;

        foreach (var fileName in fileNames)
        {
            var result = await CopyFileAsync(sourceDiskPath, destinationDiskPath, fileName, options);
            results.Add(result);
            if (result.Success)
            {
                totalBytes += result.BytesCopied;
            }
        }

        stopwatch.Stop();
        return new BatchCopyResult
        {
            TotalFiles = results.Count,
            SuccessfulFiles = results.Count(r => r.Success),
            FailedFiles = results.Count(r => !r.Success),
            TotalBytesCopied = totalBytes,
            TotalDuration = stopwatch.Elapsed,
            FileResults = results
        };
    }

    public bool CanCopyFile(IFileSystem sourceFileSystem, IFileSystem destinationFileSystem, string fileName)
    {
        var sourceEntry = sourceFileSystem.GetFiles().FirstOrDefault(f => 
            string.Equals($"{f.FileName}.{f.Extension}".TrimEnd('.'), fileName, StringComparison.OrdinalIgnoreCase) || 
            string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
        if (sourceEntry == null)
            return false;

        return sourceFileSystem.GetFileSystemType() switch
        {
            FileSystemType.N88Basic when destinationFileSystem.GetFileSystemType() == FileSystemType.N88Basic => true,
            FileSystemType.N88Basic => false,
            _ when destinationFileSystem.GetFileSystemType() == FileSystemType.N88Basic => false,
            _ => true
        };
    }

    public long CalculateRequiredSpace(IFileSystem destinationFileSystem, string fileName, long fileSize)
    {
        var info = destinationFileSystem.GetFileSystemInfo();
        var clusterSize = info.ClusterSize;
        var clusters = (fileSize + clusterSize - 1) / clusterSize;
        return clusters * clusterSize;
    }
    
    public long CalculateRequiredSpace(IFileSystem sourceFileSystem, string fileName)
    {
        var fileEntry = sourceFileSystem.GetFiles().FirstOrDefault(f => 
            string.Equals($"{f.FileName}.{f.Extension}".TrimEnd('.'), fileName, StringComparison.OrdinalIgnoreCase) || 
            string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
        if (fileEntry == null)
            return 0;

        var actualFileName = string.IsNullOrEmpty(fileEntry.Extension) 
            ? fileEntry.FileName 
            : $"{fileEntry.FileName}.{fileEntry.Extension}";
            
        var fileData = sourceFileSystem.ReadFile(actualFileName);
        return fileData.Length;
    }
}