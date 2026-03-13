using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class ManagedPublicSurfaceTest
{
    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    [Fact]
    public void CreateDiskService_ReturnsPreconfiguredService()
    {
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        Assert.NotNull(service);
    }

    [Fact]
    public void CreateFileTransferService_UsesSupportedBootstrap()
    {
        var fsInfo = new DiskFileSystemInfo("Hu-BASIC", 1024000, 0, 256, 16, "X1");
        var service = Legacy89DiskKitApplication.CreateFileTransferService(fsInfo, "sjis");
        Assert.NotNull(service);
    }

    [Fact]
    public void CreateDirectoryLayoutService_ReturnsSupportedService()
    {
        var service = Legacy89DiskKitApplication.CreateDirectoryLayoutService();
        Assert.NotNull(service);
    }

    [Fact]
    public void ManagedBootstrap_CanOpenAndListKnownSample()
    {
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.OpenDisk(GetRepoPath("images/disk_org/x1/X1turboIIIDemo.d88"));
        var fileSystem = Assert.IsAssignableFrom<Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem.IFileSystem>(service.FileSystem);
        var files = fileSystem.GetFiles().ToList();
        Assert.NotEmpty(files);
    }

    [Fact]
    public void ManagedBootstrap_CanOpenKnownSampleFromBufferWithExplicitFormat()
    {
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        var imagePath = GetRepoPath("images/disk_org/x1/X1turboIIIDemo.d88");
        var imageData = File.ReadAllBytes(imagePath);

        service.OpenDisk(imageData, "d88");

        var fileSystem = Assert.IsAssignableFrom<Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem.IFileSystem>(service.FileSystem);
        var files = fileSystem.GetFiles().ToList();
        Assert.NotEmpty(files);
    }

    [Fact]
    public void ManagedBootstrap_CanCreateAndFormatHuBasicDisk()
    {
        var imagePath = Path.Combine(Path.GetTempPath(), $"ldk-managed-{Guid.NewGuid():N}.d88");
        try
        {
            using var service = Legacy89DiskKitApplication.CreateDiskService();
            service.CreateDisk(imagePath, Legacy89DiskKit.Domain.DiskImage.Model.DiskType.TwoD, "WORKDISK");

            var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
            var container = service.OpenDisk(imagePath, readOnly: false);
            using var fileSystem = resolver.Create("hu-basic", container);
            fileSystem.Format();
            resolver.InitializeForDetection(fileSystem);

            using var verify = Legacy89DiskKitApplication.CreateDiskService();
            verify.OpenDisk(imagePath);
            Assert.NotNull(verify.FileSystem);
        }
        finally
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }

    [Fact]
    public void ManagedBootstrap_CanExportAndValidateLayout()
    {
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.OpenDisk(GetRepoPath("images/disk_org/x1/XPL3A.2D"));
        var fileSystem = service.FileSystem;
        Assert.NotNull(fileSystem);

        var layoutService = Legacy89DiskKitApplication.CreateDirectoryLayoutService();
        var plan = layoutService.ExportPlan(fileSystem!);
        var validation = layoutService.ValidatePlan(fileSystem, plan);

        Assert.True(validation.IsValid);
    }
}
