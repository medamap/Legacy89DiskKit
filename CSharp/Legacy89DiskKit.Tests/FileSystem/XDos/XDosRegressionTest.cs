using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosRegressionTest
{
    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    [Fact]
    public void WriteMultipleFiles_ReservedFatEntries_RemainUnchanged_2D()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_RESERVED_REGRESS_2D.D88");
        var container = svc.CreateDisk(path, DiskType.TwoD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        // Initial check after Format
        var fat = container.ReadSector(0, 1, 1);
        Assert.Equal(0x00, fat[0]);
        Assert.Equal(0x01, fat[1]);
        Assert.Equal(0x4A, fat[2]);

        // Write first file
        fs.WriteFile("FILE1.BIN", new byte[100], fs.CreateDefaultAttributes(false));
        fat = container.ReadSector(0, 1, 1);
        Assert.Equal(0x00, fat[0]);
        Assert.Equal(0x01, fat[1]);
        Assert.Equal(0x4A, fat[2]);

        // Write second file
        fs.WriteFile("FILE2.BIN", new byte[100], fs.CreateDefaultAttributes(false));
        fat = container.ReadSector(0, 1, 1);
        Assert.Equal(0x00, fat[0]);
        Assert.Equal(0x01, fat[1]);
        Assert.Equal(0x4A, fat[2]);

        // Fill disk until full
        int clustersToWrite = 80; 
        byte[] largeData = new byte[5120]; // 1 cluster
        try
        {
            for (int i = 0; i < clustersToWrite; i++)
            {
                fs.WriteFile($"FILL{i}.BIN", largeData, fs.CreateDefaultAttributes(false));
            }
        }
        catch (Exception ex) when (ex.Message == "Disk full." || ex.Message == "Directory full.")
        {
            // Expected
        }

        fat = container.ReadSector(0, 1, 1);
        Assert.Equal(0x00, fat[0]);
        Assert.Equal(0x01, fat[1]);
        Assert.Equal(0x4A, fat[2]);
    }
}
