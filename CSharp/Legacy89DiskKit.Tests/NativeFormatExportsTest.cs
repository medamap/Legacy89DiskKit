using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeFormatExportsTest
{
    [Fact]
    public void Format_ClearsWrittenFiles()
    {
        HandleManager.Clear();

        using var disk = new TempFormattedDiskScope();
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);
        service.FileSystem!.WriteFile("HELLO", [0x01], new ExtendedFileAttributes(FileAttributes.None, 0, false));

        var handle = HandleManager.Register(service);

        try
        {
            var result = NativeExportInvoker.Format(handle);
            Assert.Equal((int)LdkStatus.Success, result);
            Assert.Empty(service.FileSystem!.GetFiles());
        }
        finally
        {
            HandleManager.Clear();
        }
    }

    [Fact]
    public void Format_ReturnsInvalidHandleForUnknownHandle()
    {
        var result = NativeExportInvoker.Format(-22);
        Assert.Equal((int)LdkStatus.ErrorInvalidHandle, result);
    }
}
