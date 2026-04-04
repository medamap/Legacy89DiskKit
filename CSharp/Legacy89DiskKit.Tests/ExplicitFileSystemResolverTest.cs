using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class ExplicitFileSystemResolverTest
{
    [Fact]
    public void Create_HuBasicTwoD_CreatesDetectableDisk()
    {
        var resolver = new ExplicitFileSystemResolver();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            using (var container = D88DiskContainer.CreateNew(path, DiskType.TwoD, "TEST"))
            using (var fs = resolver.Create("hu-basic", container))
            {
                fs.Format();
                resolver.InitializeForDetection(fs);
            }

            using var reopened = new D88DiskContainer(path, true);
            var provider = new HuBasicFileSystemProvider();
            Assert.True(provider.CanHandle(reopened));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void Create_N88BasicTwoHd_Throws()
    {
        var resolver = new ExplicitFileSystemResolver();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            using var container = D88DiskContainer.CreateNew(path, DiskType.TwoHD, "TEST");
            var ex = Assert.Throws<InvalidOperationException>(() => resolver.Create("n88-basic", container));
            Assert.Contains("2D and 2DD", ex.Message);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void Create_MsxDosTwoD_Throws()
    {
        var resolver = new ExplicitFileSystemResolver();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            using var container = D88DiskContainer.CreateNew(path, DiskType.TwoD, "TEST");
            var ex = Assert.Throws<InvalidOperationException>(() => resolver.Create("msx-dos", container));
            Assert.Contains("2DD", ex.Message);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData("cpm")]
    [InlineData("fat12")]
    [InlineData("fat32")]
    [InlineData("pc-9801-harddisk")]
    [InlineData("msx-cartridge")]
    public void Create_ReservedFileSystem_ThrowsReservedMessage(string fileSystemName)
    {
        var resolver = new ExplicitFileSystemResolver();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            using var container = D88DiskContainer.CreateNew(path, DiskType.TwoD, "TEST");
            var ex = Assert.Throws<NotSupportedException>(() => resolver.Create(fileSystemName, container));
            Assert.Contains("This feature is reserved, please request!!", ex.Message);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
