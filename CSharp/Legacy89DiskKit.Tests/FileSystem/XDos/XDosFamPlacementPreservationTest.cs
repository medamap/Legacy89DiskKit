using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosFamPlacementPreservationTest
{
    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private (IDiskContainer container, XDosFileSystem fs) CreateFormattedXDos(string name, DiskType diskType = DiskType.TwoD)
    {
        var path = GetRepoPath($"images/test/{name}.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (File.Exists(path)) File.Delete(path);

        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var container = svc.CreateDisk(path, diskType);
        var fs = new XDosFileSystem(container);
        fs.Format();
        return (container, fs);
    }

    [Fact]
    public void CloneXDosBootable_VerifyFamPlacementPreservation()
    {
        var xdosSysPath = GetRepoPath("images/disk_org/x1/XDOS_SYS.D88");
        if (!File.Exists(xdosSysPath)) return;

        var (dstContainer, dstFs) = CreateFormattedXDos("WF_PRESERVE_FAM_DST");

        using var srcSvc = Legacy89DiskKitApplication.CreateDiskService();
        var srcContainer = srcSvc.OpenDisk(xdosSysPath, readOnly: true);
        var srcFs = new XDosFileSystem(srcContainer);

        var srcFiles = srcFs.GetFilesWithMetadata().Where(e => !e.IsEmpty).ToList();

        var cloneService = Legacy89DiskKitApplication.CreateDiskCloneService(srcFs.GetFileSystemInfo());
        cloneService.CloneXDosBootable(srcFs, new XDosTransferAdapter(srcFs), dstFs, new XDosTransferAdapter(dstFs));

        dstContainer.Save();

        var dstFiles = dstFs.GetFilesWithMetadata().Where(e => !e.IsEmpty).ToList();

        foreach (var srcEntry in srcFiles)
        {
            var dstEntry = dstFiles.FirstOrDefault(e => e.FileName == srcEntry.FileName && e.RawFileType == srcEntry.RawFileType);
            Assert.NotNull(dstEntry);

            // This is what we want to preserve
            Assert.Equal(srcEntry.FamPointer.Track, dstEntry.FamPointer.Track);
            Assert.Equal(srcEntry.FamPointer.Sector, dstEntry.FamPointer.Sector);
        }
    }

    [Fact]
    public void TransferFiles_Ordinary_DoesNotPreserveFamPlacement()
    {
        var xdosSysPath = GetRepoPath("images/disk_org/x1/XDOS_SYS.D88");
        if (!File.Exists(xdosSysPath)) return;

        var (dstContainer, dstFs) = CreateFormattedXDos("WF_ORDINARY_XFER_DST");
        
        // Occupy the beginning of the disk to force different placement for ordinary transfer
        dstFs.WriteFile("DUMMY.BIN", new byte[1024], dstFs.CreateDefaultAttributes(false));

        using var srcSvc = Legacy89DiskKitApplication.CreateDiskService();
        var srcContainer = srcSvc.OpenDisk(xdosSysPath, readOnly: true);
        var srcFs = new XDosFileSystem(srcContainer);

        var srcFiles = srcFs.GetFilesWithMetadata().Where(e => !e.IsEmpty).Take(3).ToList();
        var fileNames = srcFiles.Select(e => e.FileName).ToList();

        var cloneService = Legacy89DiskKitApplication.CreateDiskCloneService(srcFs.GetFileSystemInfo());
        // Use default adapters (IsCloneMode = false)
        cloneService.TransferFiles(srcFs, dstFs, fileNames, new XDosTransferAdapter(srcFs), new XDosTransferAdapter(dstFs));

        dstContainer.Save();

        var dstFiles = dstFs.GetFilesWithMetadata().Where(e => !e.IsEmpty).ToList();

        bool anyDifference = false;
        foreach (var srcEntry in srcFiles)
        {
            var dstEntry = dstFiles.FirstOrDefault(e => e.FileName == srcEntry.FileName);
            if (dstEntry != null)
            {
                if (srcEntry.FamPointer.Track != dstEntry.FamPointer.Track ||
                    srcEntry.FamPointer.Sector != dstEntry.FamPointer.Sector)
                {
                    anyDifference = true;
                    break;
                }
            }
        }

        // Ordinary allocator should result in different placement than source system disk
        // (because source system disk often has optimized or specific placement)
        Assert.True(anyDifference, "Ordinary transfer should use normal allocator, resulting in different placement.");
    }

    [Fact]
    public void ReuseAdapter_StateLeak_VerifyStateLeak()
    {
        var xdosSysPath = GetRepoPath("images/disk_org/x1/XDOS_SYS.D88");
        if (!File.Exists(xdosSysPath)) return;

        var (dstContainer1, dstFs1) = CreateFormattedXDos("WF_REUSE_ADAPTER_DST1");
        var (dstContainer2, dstFs2) = CreateFormattedXDos("WF_REUSE_ADAPTER_DST2");

        using var srcSvc = Legacy89DiskKitApplication.CreateDiskService();
        var srcContainer = srcSvc.OpenDisk(xdosSysPath, readOnly: true);
        var srcFs = new XDosFileSystem(srcContainer);

        var srcAdapter = new XDosTransferAdapter(srcFs);
        var dstAdapter = new XDosTransferAdapter(dstFs1);

        var cloneService = Legacy89DiskKitApplication.CreateDiskCloneService(srcFs.GetFileSystemInfo());

        // 1. First call: Clone (sets IsCloneMode = true)
        cloneService.CloneXDosBootable(srcFs, srcAdapter, dstFs1, dstAdapter);
        
        // After call, it should be reset to previous value (false)
        Assert.False(srcAdapter.IsCloneMode, "srcAdapter.IsCloneMode should be reset to false.");
        Assert.False(dstAdapter.IsCloneMode, "dstAdapter.IsCloneMode should be reset to false.");
    }
}
