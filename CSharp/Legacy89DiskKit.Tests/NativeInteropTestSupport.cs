using System.Runtime.InteropServices;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Tests;

internal sealed class Utf8StringScope : IDisposable
{
    public Utf8StringScope(string value)
    {
        Pointer = Marshal.StringToCoTaskMemUTF8(value);
    }

    public IntPtr Pointer { get; }

    public void Dispose()
    {
        Marshal.FreeCoTaskMem(Pointer);
    }
}

internal sealed class TempFormattedDiskScope : IDisposable
{
    public TempFormattedDiskScope(string fileSystemName = "hu-basic")
    {
        ImagePath = Path.Combine(Path.GetTempPath(), $"ldk-native-{Guid.NewGuid():N}.d88");

        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.CreateDisk(ImagePath, DiskType.TwoD, "NATIVETEST");

        var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
        var container = service.OpenDisk(ImagePath, readOnly: false);
        using var fileSystem = resolver.Create(fileSystemName, container);
        fileSystem.Format();
        resolver.InitializeForDetection(fileSystem);
    }

    public string ImagePath { get; }

    public void Dispose()
    {
        if (File.Exists(ImagePath))
        {
            File.Delete(ImagePath);
        }
    }
}
