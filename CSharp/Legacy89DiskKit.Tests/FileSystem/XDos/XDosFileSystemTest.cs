using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;
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
}
