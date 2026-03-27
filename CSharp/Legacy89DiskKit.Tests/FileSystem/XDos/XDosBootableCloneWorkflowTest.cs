using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosBootableCloneWorkflowTest
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
    public void BootAreaInstall_FormattedSource_BootDataTransfersToTarget()
    {
        var (srcContainer, srcFs) = CreateFormattedXDos("WF_BOOT_SRC");
        var (dstContainer, dstFs) = CreateFormattedXDos("WF_BOOT_DST");

        var cloneService = Legacy89DiskKitApplication.CreateDiskCloneService(srcFs.GetFileSystemInfo());
        cloneService.TransferBootArea(srcFs, dstFs);

        var srcBoot = srcFs.ReadBootArea();
        var dstBoot = dstFs.ReadBootArea();
        Assert.True(srcBoot.SequenceEqual(dstBoot));
    }

    [Fact]
    public void CloneWorkflow_SmallFixture_AllTopLevelFilesTransferred()
    {
        var (srcContainer, srcFs) = CreateFormattedXDos("WF_XFER_SRC");
        var (dstContainer, dstFs) = CreateFormattedXDos("WF_XFER_DST");

        byte[] data1 = new byte[256]; data1[0] = 0x11;
        byte[] data2 = new byte[512]; data2[0] = 0x22;
        byte[] data3 = System.Text.Encoding.ASCII.GetBytes("HELLO XDOS\r\n");

        srcFs.WriteFileInternal("PROG.CMD", data1, srcFs.CreateDefaultAttributes(false),
            loadAddress: 0x8000, executionAddress: 0x8100,
            forcedRawType: (ushort)XDosFileType.Cmd);
        srcFs.WriteFileInternal("DATA.BIN", data2, srcFs.CreateDefaultAttributes(false),
            forcedRawType: (ushort)XDosFileType.Bin);
        srcFs.WriteFile("READ.TXT", data3, srcFs.CreateDefaultAttributes(true));

        var cloneService = Legacy89DiskKitApplication.CreateDiskCloneService(srcFs.GetFileSystemInfo());
        cloneService.CloneXDosBootable(srcFs, new XDosTransferAdapter(srcFs), dstFs, new XDosTransferAdapter(dstFs));

        Assert.True(dstFs.FileExists("PROG.CMD"));
        Assert.True(dstFs.FileExists("DATA.BIN"));
        Assert.True(dstFs.FileExists("READ.TXT"));

        var dstCmd = dstFs.GetFilesWithMetadata().First(e => e.FileName == "PROG.CMD");
        Assert.Equal((ushort)XDosFileType.Cmd, dstCmd.RawFileType);
        Assert.Equal((ushort)0x8000, dstCmd.StartAddress);
        Assert.Equal((ushort)0x8100, dstCmd.ExecAddressOrSizeHigh);
    }

    [Fact]
    public void CloneWorkflow_SmallFixture_ReopenTarget_VerifiesPayloadsAndBootArea()
    {
        var (srcContainer, srcFs) = CreateFormattedXDos("WF_REOPEN_SRC");
        var dstPath = GetRepoPath("images/test/WF_REOPEN_DST.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(dstPath)!);
        if (File.Exists(dstPath)) File.Delete(dstPath);

        IDiskContainer dstContainer;
        XDosFileSystem dstFs;
        {
            using var dstSvc = Legacy89DiskKitApplication.CreateDiskService();
            dstContainer = dstSvc.CreateDisk(dstPath, DiskType.TwoD);
            dstFs = new XDosFileSystem(dstContainer);
        }

        byte[] payload1 = new byte[256]; new Random(1).NextBytes(payload1);
        byte[] payload2 = new byte[384]; new Random(2).NextBytes(payload2);

        srcFs.WriteFileInternal("FILE_A.BIN", payload1, srcFs.CreateDefaultAttributes(false),
            loadAddress: 0xA000, executionAddress: 0xA000,
            forcedRawType: (ushort)XDosFileType.Bin);
        srcFs.WriteFileInternal("FILE_B.CMD", payload2, srcFs.CreateDefaultAttributes(false),
            loadAddress: 0xB000, executionAddress: 0xB100,
            forcedRawType: (ushort)XDosFileType.Cmd);

        var cloneService = Legacy89DiskKitApplication.CreateDiskCloneService(srcFs.GetFileSystemInfo());
        cloneService.CloneXDosBootable(srcFs, new XDosTransferAdapter(srcFs), dstFs, new XDosTransferAdapter(dstFs));

        var srcEntries = srcFs.GetFiles().ToList();
        var srcBoot = srcFs.ReadBootArea();
        dstContainer.Save();

        using var verifySvc = Legacy89DiskKitApplication.CreateDiskService();
        var verifyContainer = verifySvc.OpenDisk(dstPath, readOnly: true);
        var verifyFs = new XDosFileSystem(verifyContainer);

        var verifyFiles = verifyFs.GetFiles().ToList();
        Assert.Equal(srcEntries.Count, verifyFiles.Count);

        Assert.True(payload1.SequenceEqual(verifyFs.ReadFile("FILE_A.BIN")));
        Assert.True(payload2.SequenceEqual(verifyFs.ReadFile("FILE_B.CMD")));

        var verifyBoot = verifyFs.ReadBootArea();
        Assert.True(srcBoot.SequenceEqual(verifyBoot));

        var verifyMeta = verifyFs.GetFilesWithMetadata();
        Assert.All(verifyMeta, e => Assert.False(e.IsEmpty));
    }

    [Fact]
    public void RealImageClone_XdosSys_CompletesWithinTwoDCapacity()
    {
        var xdosSysPath = GetRepoPath("images/disk_org/x1/XDOS_SYS.D88");
        if (!File.Exists(xdosSysPath)) return;

        var (dstContainer, dstFs) = CreateFormattedXDos("WF_REALIMG_DST");

        using var srcSvc = Legacy89DiskKitApplication.CreateDiskService();
        var srcContainer = srcSvc.OpenDisk(xdosSysPath, readOnly: true);
        var srcFs = new XDosFileSystem(srcContainer);

        var cloneService = Legacy89DiskKitApplication.CreateDiskCloneService(srcFs.GetFileSystemInfo());
        cloneService.CloneXDosBootable(srcFs, new XDosTransferAdapter(srcFs), dstFs, new XDosTransferAdapter(dstFs));

        dstContainer.Save();

        using var verifySvc = Legacy89DiskKitApplication.CreateDiskService();
        var verifyContainer = verifySvc.OpenDisk(dstContainer.FilePath, readOnly: true);
        var verifyFs = new XDosFileSystem(verifyContainer);

        Assert.Equal(srcFs.ReadBootArea(), verifyFs.ReadBootArea());
        Assert.Equal(srcFs.GetFiles().Count(), verifyFs.GetFiles().Count());
    }
}
