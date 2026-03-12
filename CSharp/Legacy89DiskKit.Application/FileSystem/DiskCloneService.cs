using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Exception;

namespace Legacy89DiskKit.Application.FileSystem;

public class DiskCloneService
{
    private readonly FileTransferService _transferService;

    public DiskCloneService(FileTransferService transferService)
    {
        _transferService = transferService;
    }

    /// <summary>
    /// Transfers the boot area (IPL) from source to target.
    /// </summary>
    public void TransferBootArea(IFileSystem source, IFileSystem target)
    {
        var bootData = source.ReadBootArea();
        target.WriteBootArea(bootData);
    }

    /// <summary>
    /// Transfers multiple files from source to target.
    /// </summary>
    public void TransferFiles(IFileSystem source, IFileSystem target, IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            try
            {
                var data = source.ReadFile(fileName);
                var sourceFiles = source.GetFiles();
                var sourceFile = sourceFiles.FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                
                if (sourceFile == null) throw new FileSystemException($"Source file not found: {fileName}");

                target.WriteFile(fileName, data, sourceFile.Attributes, sourceFile.LoadAddress, sourceFile.ExecutionAddress);
            }
            catch (Exception ex)
            {
                throw new FileSystemException($"Failed to transfer file '{fileName}': {ex.Message}", ex);
            }
        }
    }
}
