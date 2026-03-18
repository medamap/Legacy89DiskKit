using Legacy89DiskKit.Application.Native;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;
using DiskFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeFilesListExportsTest
{
    [Fact]
    public void GetFiles_ReturnsNativeEntriesForWrittenFiles()
    {
        HandleManager.Clear();
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        using var disk = new TempFormattedDiskScope();
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);
        service.FileSystem!.WriteFile("HELLO", [0x01, 0x02], new ExtendedFileAttributes(DiskFileAttributes.None, 0, false));
        service.FileSystem!.WriteFile("WORLD", [0x03, 0x04, 0x05], new ExtendedFileAttributes(DiskFileAttributes.ReadOnly, 0, false));

        var handle = HandleManager.Register(service.Session!);

        try
        {
            using var buffer = new NativeFileEntryBufferScope(8);
            var count = NativeExportInvoker.GetFiles(handle, buffer.Pointer, buffer.Capacity);

            Assert.True(count >= 2);

            var names = Enumerable.Range(0, count)
                .Select(buffer.ReadEntry)
                .Select(entry => entry.FileName.TrimEnd('\0'))
                .ToList();

            Assert.Contains("HELLO", names);
            Assert.Contains("WORLD", names);
        }
        finally
        {
            HandleManager.Clear();
        }
    }

    [Fact]
    public void GetFiles_ReturnsInvalidHandleForUnknownHandle()
    {
        using var buffer = new NativeFileEntryBufferScope(1);
        var result = NativeExportInvoker.GetFiles(-99, buffer.Pointer, buffer.Capacity);
        Assert.Equal((int)LdkStatus.ErrorInvalidHandle, result);
    }

    [Fact]
    public void GetFiles_ClampsToRequestedCapacity()
    {
        HandleManager.Clear();

        using var disk = new TempFormattedDiskScope();
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);
        service.FileSystem!.WriteFile("HELLO", [0x01], new ExtendedFileAttributes(DiskFileAttributes.None, 0, false));
        service.FileSystem!.WriteFile("WORLD", [0x02], new ExtendedFileAttributes(DiskFileAttributes.None, 0, false));

        var handle = HandleManager.Register(service.Session!);

        try
        {
            using var buffer = new NativeFileEntryBufferScope(1);
            var count = NativeExportInvoker.GetFiles(handle, buffer.Pointer, buffer.Capacity);
            Assert.Equal(1, count);
        }
        finally
        {
            HandleManager.Clear();
        }
    }
}
