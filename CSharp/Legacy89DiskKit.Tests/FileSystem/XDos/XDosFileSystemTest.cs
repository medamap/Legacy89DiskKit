using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;
using FileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosFileSystemTest
{
    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        // From bin/Debug/net9.0/Legacy89DiskKit.Tests.dll up to repo root
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private string XDosSysPath => GetRepoPath("images/disk_org/x1/XDOS_SYS.D88");
    private string XDosUtilPath => GetRepoPath("images/disk_org/x1/XDOSUTIL.D88");
    private string HuBasicPath => GetRepoPath("images/disk_org/x1/X1turboIIIDemo.d88");

    [Fact]
    public void Provider_CanHandle_XDosDisk_ReturnsTrue()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var container = diskService.Session as IDiskContainer;
        Assert.NotNull(container);

        var provider = new XDosFileSystemProvider();
        Assert.True(provider.CanHandle(container));
    }

    [Fact]
    public void Provider_CanHandle_NonXDosDisk_ReturnsFalse()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(HuBasicPath, true);
        var container = diskService.Session as IDiskContainer;
        Assert.NotNull(container);

        var provider = new XDosFileSystemProvider();
        Assert.False(provider.CanHandle(container));
    }

    [Fact]
    public void GetFileSystemInfo_ReturnsValidInfo()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var info = fs.GetFileSystemInfo();
        Assert.Equal("X-DOS", info.FileSystemName);
        Assert.Equal("X1", info.PlatformId);
    }

    [Fact]
    public void GetFiles_XDosSys_ReturnsAtLeast5Entries()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var files = fs.GetFiles().ToList();
        Assert.True(files.Count >= 5);
        Assert.All(files, f => Assert.False(string.IsNullOrEmpty(f.FileName)));
        Assert.Contains(files, f => f.LoadAddress > 0);
    }

    [Fact]
    public void GetFiles_XDosUtil_ReturnsAtLeast10Entries()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosUtilPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var files = fs.GetFiles().ToList();
        Assert.True(files.Count >= 10);
    }

    [Fact]
    public void FileExists_ReturnsExpectedResult()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var files = fs.GetFiles().ToList();
        var firstFile = files.First().FileName;

        Assert.True(fs.FileExists(firstFile));
        Assert.False(fs.FileExists("DOESNOTEXIST"));
    }

    [Fact]
    public void Registry_AutoDetects_XDos()
    {
        var registry = Legacy89DiskKitApplication.CreateFileSystemRegistry();
        using var diskService = new DiskService(fsRegistry: registry);
        diskService.OpenDisk(XDosSysPath, true);

        Assert.NotNull(diskService.FileSystem);
        Assert.IsType<XDosFileSystem>(diskService.FileSystem);
    }

    [Fact]
    public void ExplicitResolver_CanCreate_XDos()
    {
        var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        var container = diskService.OpenDisk(XDosSysPath, true);

        var fs = resolver.Create("xdos", container);
        Assert.NotNull(fs);
        Assert.IsType<XDosFileSystem>(fs);
    }

    [Fact]
    public void WriteFileInternal_DuplicateDisk_LogicalReconstruction()
    {
        // 1. Open Source
        using var srcService = Legacy89DiskKitApplication.CreateDiskService();
        var srcContainer = srcService.OpenDisk(XDosSysPath, true);
        var srcFs = (srcService.FileSystem as XDosFileSystem)!;
        var srcBoot = srcFs.ReadBootArea();

        var uniqueFiles = srcFs.GetFilesWithMetadata()
            .GroupBy(e => new { e.FirstCluster, e.FirstSectorR })
            .Select(g => g.First())
            .Where(e => e.FirstSectorR > 0)
            .ToList();

        // 2. Create Target — clone source D88 to inherit exact sector geometry
        //    (CreateDisk would create all tracks as 256-byte/16-sector,
        //     but X-DOS data tracks are 512-byte/10-sector)
        var outputPath = GetRepoPath("images/test/XDOS_RECONST.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.Copy(XDosSysPath, outputPath, overwrite: true);

        using var destService = Legacy89DiskKitApplication.CreateDiskService();
        var destContainer = destService.OpenDisk(outputPath, false);
        var destFs = new XDosFileSystem(destContainer);

        destFs.Format();

        // 3. Write each file at its original cluster position, following source FAM chain
        var srcFamReader = new XDosFamReader(srcContainer);
        foreach (var e in uniqueFiles)
        {
            var data = srcFs.ReadFileRaw(e.RawFileName);
            var clusterChain = srcFamReader.GetChain((byte)e.FirstCluster);
            destFs.WriteFileInternal(
                e.FileName, data,
                new ExtendedFileAttributes(FileAttributes.None, e.Attribute, false, "X-DOS"),
                e.LoadAddress, e.ExecutionAddress,
                e.FirstCluster, e.RawFileName, e.RawFileType, e.FirstSectorR,
                forcedClusterChain: clusterChain);
        }

        // 4. Clone FAT, FAM and Directory from source (restores original management state)
        destContainer.WriteSector(0, 1, 1, srcContainer.ReadSector(0, 1, 1)); // FAT
        destContainer.WriteSector(1, 0, 1, srcContainer.ReadSector(1, 0, 1)); // FAM
        for (int r = 2; r <= 10; r++) // Directory
        {
            destContainer.WriteSector(0, 1, r, srcContainer.ReadSector(0, 1, r));
        }

        // 5. Write Boot Area
        destFs.WriteBootArea(srcBoot);
        destContainer.Save();

        // 6. Verification
        using var verifyService = Legacy89DiskKitApplication.CreateDiskService();
        verifyService.OpenDisk(outputPath, true);
        var verifyFs = (verifyService.FileSystem as XDosFileSystem)!;

        foreach (var srcEntry in uniqueFiles)
        {
            Assert.True(verifyFs.FileExistsRaw(srcEntry.RawFileName), $"Missing: {srcEntry.FileName}");
            var srcData  = srcFs.ReadFileRaw(srcEntry.RawFileName);
            var destData = verifyFs.ReadFileRaw(srcEntry.RawFileName);
            Assert.True(srcData.SequenceEqual(destData),
                $"Parity failure: {srcEntry.FileName} (T{srcEntry.FirstCluster} R{srcEntry.FirstSectorR})");
        }
    }
}
