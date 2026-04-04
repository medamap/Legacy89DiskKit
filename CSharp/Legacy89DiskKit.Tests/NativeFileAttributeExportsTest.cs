using Legacy89DiskKit.Native.Application;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;
using DiskFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeFileAttributeExportsTest
{
    [Fact]
    public void UpdateAttributes_ChangesVisibleFileAttributes()
    {
        HandleManager.Clear();
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        using var disk = new TempFormattedDiskScope();
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);
        service.FileSystem!.WriteFile("HELLO", [0x01], new ExtendedFileAttributes(DiskFileAttributes.None, 0, false));

        var handle = HandleManager.Register(service.Session!);

        try
        {
            using var fileName = new Utf8StringScope("HELLO");
            var result = NativeExportInvoker.UpdateAttributes(handle, fileName.Pointer, (ushort)DiskFileAttributes.ReadOnly);

            Assert.Equal((int)LdkStatus.Success, result);

            var file = Assert.Single(service.FileSystem!.GetFiles());
            Assert.Equal("HELLO", file.FileName);
        }
        finally
        {
            HandleManager.Clear();
        }
    }

    [Fact]
    public void UpdateAttributes_ReturnsInvalidHandleForUnknownHandle()
    {
        using var fileName = new Utf8StringScope("HELLO");
        var result = NativeExportInvoker.UpdateAttributes(-31, fileName.Pointer, (ushort)DiskFileAttributes.ReadOnly);
        Assert.Equal((int)LdkStatus.ErrorInvalidHandle, result);
    }
}
