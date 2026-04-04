using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Exception;
using Legacy89DiskKit.Archive.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.FileSystem.Application;

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
    public void TransferFiles(
        IFileSystem source,
        IFileSystem target,
        IEnumerable<string> fileNames,
        IFileSystemTransferAdapter? sourceAdapter = null,
        IFileSystemTransferAdapter? targetAdapter = null)
    {
        if (sourceAdapter != null && targetAdapter != null)
        {
            var orchestrator = new FileSystemTransferOrchestrator();
            orchestrator.Register(source, sourceAdapter);
            orchestrator.Register(target, targetAdapter);

            foreach (var fileName in fileNames)
            {
                try
                {
                    orchestrator.Transfer(source, target, fileName, fileName);
                }
                catch (Exception ex)
                {
                    throw new FileSystemException($"Failed to transfer file '{fileName}': {ex.Message}", ex);
                }
            }

            return;
        }

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
    /// Clones an X-DOS disk to a new destination: formats the destination, copies the boot area,
    /// then transfers all top-level non-directory files using the provided adapters.
    /// </summary>
    public void CloneXDosBootable(
        IFileSystem srcFs, IFileSystemTransferAdapter srcAdapter,
        IFileSystem dstFs, IFileSystemTransferAdapter dstAdapter)
    {
        bool srcPrevMode = false;
        bool dstPrevMode = false;

        if (srcAdapter is Legacy89DiskKit.Infrastructure.FileSystem.XDos.XDosTransferAdapter srcX)
        {
            srcPrevMode = srcX.IsCloneMode;
            srcX.IsCloneMode = true;
        }
        if (dstAdapter is Legacy89DiskKit.Infrastructure.FileSystem.XDos.XDosTransferAdapter dstX)
        {
            dstPrevMode = dstX.IsCloneMode;
            dstX.IsCloneMode = true;
        }

        try
        {
            var bootArea = srcFs.ReadBootArea();
            dstFs.Format();
            dstFs.WriteBootArea(bootArea);

            var fileNames = srcFs.GetFiles()
                .Where(entry => !entry.Attributes.StandardAttributes.HasFlag(Domain.FileSystem.Model.FileAttributes.Directory))
                .Select(entry => entry.FullName);

            TransferFiles(srcFs, dstFs, fileNames, srcAdapter, dstAdapter);
        }
        finally
        {
            if (srcAdapter is Legacy89DiskKit.Infrastructure.FileSystem.XDos.XDosTransferAdapter srcFinal) srcFinal.IsCloneMode = srcPrevMode;
            if (dstAdapter is Legacy89DiskKit.Infrastructure.FileSystem.XDos.XDosTransferAdapter dstFinal) dstFinal.IsCloneMode = dstPrevMode;
        }
    }

    /// <summary>
    /// Clones a Hu-BASIC disk to a new destination: formats the destination, copies the boot area,
    /// then transfers all files using HuBasicTransferAdapter for high fidelity.
    /// </summary>
    public void CloneHuBasicBootable(
        IFileSystem srcFs, IFileSystemTransferAdapter srcAdapter,
        IFileSystem dstFs, IFileSystemTransferAdapter dstAdapter)
    {
        var bootArea = srcFs.ReadBootArea();
        dstFs.Format();
        dstFs.WriteBootArea(bootArea);

        var fileNames = srcFs.GetFiles()
            .Select(entry => entry.FullName);

        TransferFiles(srcFs, dstFs, fileNames, srcAdapter, dstAdapter);
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
