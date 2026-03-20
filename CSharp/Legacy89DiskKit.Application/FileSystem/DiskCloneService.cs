using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Exception;
using Legacy89DiskKit.Application.Services;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Application.FileSystem;

public class DiskCloneService
{
    private readonly FileTransferService _transferService;
    private readonly FileNameNormalizationService _normalizationService;

    public DiskCloneService(FileTransferService transferService, FileNameNormalizationService normalizationService)
    {
        _transferService = transferService;
        _normalizationService = normalizationService;
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
        var targetInfo = target.GetFileSystemInfo();
        var existingNames = new HashSet<string>(target.GetFiles().Select(f => f.FullName.ToUpperInvariant()));

        foreach (var fileName in fileNames)
        {
            try
            {
                var sourceFiles = source.GetFiles();
                var sourceFile = sourceFiles.FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                
                if (sourceFile == null) throw new FileSystemException($"Source file not found: {fileName}");

                var data = source.ReadFile(fileName);

                string normalizedName = _normalizationService.Normalize(
                    fileName,
                    targetInfo.DefaultEncodingId,
                    targetInfo.MaxBaseNameLength,
                    targetInfo.MaxExtensionLength,
                    existingNames);

                target.WriteFile(normalizedName, data, sourceFile.Attributes, sourceFile.LoadAddress, sourceFile.ExecutionAddress);
                existingNames.Add(normalizedName.ToUpperInvariant());
            }
            catch (Exception ex)
            {
                throw new FileSystemException($"Failed to transfer file '{fileName}': {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Performs a sector-by-sector physical copy from source to destination.
    /// </summary>
    public (int tracksCopied, int sectorsSkipped) CopySectors(IDiskContainer source, IDiskContainer destination, bool allowPartialRead = true)
    {
        if (source.DiskType != destination.DiskType)
        {
            throw new ArgumentException("Disk types do not match.");
        }

        int tracksCopied = 0;
        int sectorsSkipped = 0;
        var copiedTracks = new HashSet<(int, int)>();

        foreach (var sectorInfo in source.GetAllSectors())
        {
            try
            {
                var data = source.ReadSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector);
                destination.WriteSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector, data);

                if (copiedTracks.Add((sectorInfo.Cylinder, sectorInfo.Head)))
                {
                    tracksCopied++;
                }
            }
            catch
            {
                if (!allowPartialRead) throw;
                sectorsSkipped++;
            }
        }

        return (tracksCopied, sectorsSkipped);
    }
}
