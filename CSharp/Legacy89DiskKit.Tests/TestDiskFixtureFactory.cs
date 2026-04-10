using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;

namespace Legacy89DiskKit.Tests;

internal static class TestDiskFixtureFactory
{
    private static readonly string TempRoot = Path.Combine(
        Path.GetTempPath(),
        "Legacy89DiskKitTests");

    public static string CreateTempDiskPath(string fileName)
    {
        Directory.CreateDirectory(TempRoot);
        return Path.Combine(TempRoot, fileName);
    }

    public static string CreateFormattedXDosDisk(
        string fileName,
        DiskType diskType = DiskType.TwoD,
        int fileCount = 0)
    {
        var path = CreateTempDiskPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var service = Legacy89DiskKitApplication.CreateDiskService();
        var container = service.CreateDisk(path, diskType);
        var fs = new XDosFileSystem(container);
        fs.Format();

        for (var i = 0; i < fileCount; i++)
        {
            var payload = Enumerable.Repeat((byte)(0x40 + i), 256 + (i * 32)).ToArray();
            var name = $"FILE{i:D2}.BIN";
            fs.WriteFile(name, payload, fs.CreateDefaultAttributes(false), (ushort)(0x8000 + i), (ushort)(0x8000 + i));
        }

        container.Save();
        return path;
    }

    public static string CreateFormattedHuBasicDisk(
        string fileName,
        DiskType diskType = DiskType.TwoD,
        bool writeSampleFile = false)
    {
        var path = CreateTempDiskPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var service = Legacy89DiskKitApplication.CreateDiskService();
        var container = service.CreateDisk(path, diskType);
        var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
        using var fileSystem = resolver.Create("hu-basic", container);
        fileSystem.Format();

        if (writeSampleFile)
        {
            fileSystem.WriteFile("HELLO.BAS", [0x10, 0x20, 0x30], fileSystem.CreateDefaultAttributes(true));
        }

        container.Save();
        return path;
    }

    public static (IDiskContainer container, XDosFileSystem fs) CreateOpenFormattedXDos(
        string fileName,
        DiskType diskType = DiskType.TwoD)
    {
        var path = CreateTempDiskPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var service = Legacy89DiskKitApplication.CreateDiskService();
        var container = service.CreateDisk(path, diskType);
        var fs = new XDosFileSystem(container);
        fs.Format();
        return (container, fs);
    }

    public static string CreateMultiSlotD88WithFormattedHuBasicFirstSlot(string fileName)
    {
        var path = CreateTempDiskPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var firstSlotPath = CreateFormattedHuBasicDisk($"FIRST_{fileName}", DiskType.TwoD);
        var secondSlotPath = CreateTempDiskPath($"SECOND_{fileName}");
        if (File.Exists(secondSlotPath))
        {
            File.Delete(secondSlotPath);
        }

        using (var service = Legacy89DiskKitApplication.CreateDiskService())
        {
            service.CreateDisk(secondSlotPath, DiskType.TwoD, "SLOT1");
        }

        var firstBytes = File.ReadAllBytes(firstSlotPath);
        var secondBytes = File.ReadAllBytes(secondSlotPath);
        var combined = new byte[firstBytes.Length + secondBytes.Length];
        Buffer.BlockCopy(firstBytes, 0, combined, 0, firstBytes.Length);
        Buffer.BlockCopy(secondBytes, 0, combined, firstBytes.Length, secondBytes.Length);
        File.WriteAllBytes(path, combined);

        File.Delete(firstSlotPath);
        File.Delete(secondSlotPath);
        return path;
    }
}
