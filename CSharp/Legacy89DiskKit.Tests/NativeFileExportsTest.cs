using System.Runtime.InteropServices;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;
using DiskFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeFileExportsTest
{
    [Fact]
    public void ReadFile_ReturnsInvalidHandleForUnknownHandle()
    {
        using var fileName = new Utf8StringScope("HELLO");
        var buffer = Marshal.AllocHGlobal(16);

        try
        {
            var result = NativeExportInvoker.ReadFile(-123, fileName.Pointer, buffer, 16);
            Assert.Equal((int)LdkStatus.ErrorInvalidHandle, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void WriteReadRenameAndDeleteFile_RoundTripsThroughNativeExports()
    {
        HandleManager.Clear();

        using var disk = new TempFormattedDiskScope();
        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);

        var handle = HandleManager.Register(service);

        try
        {
            var fileData = new byte[] { 0x10, 0x20, 0x30, 0x40 };
            using var writeName = new Utf8StringScope("HELLO");
            var dataBuffer = Marshal.AllocHGlobal(fileData.Length);
            Marshal.Copy(fileData, 0, dataBuffer, fileData.Length);

            try
            {
                var writeResult = NativeExportInvoker.WriteFile(handle, writeName.Pointer, dataBuffer, fileData.Length, (ushort)DiskFileAttributes.None);
                Assert.Equal((int)LdkStatus.Success, writeResult);
            }
            finally
            {
                Marshal.FreeHGlobal(dataBuffer);
            }

            var readBuffer = Marshal.AllocHGlobal(16);
            try
            {
                var readResult = NativeExportInvoker.ReadFile(handle, writeName.Pointer, readBuffer, 16);
                Assert.Equal(fileData.Length, readResult);

                var actual = new byte[fileData.Length];
                Marshal.Copy(readBuffer, actual, 0, fileData.Length);
                Assert.Equal(fileData, actual);
            }
            finally
            {
                Marshal.FreeHGlobal(readBuffer);
            }

            using var renamedName = new Utf8StringScope("HELLO2");
            var renameResult = NativeExportInvoker.RenameFile(handle, writeName.Pointer, renamedName.Pointer);
            Assert.Equal((int)LdkStatus.Success, renameResult);
            Assert.Contains(service.FileSystem!.GetFiles(), static file => file.FileName == "HELLO2");

            var deleteResult = NativeExportInvoker.DeleteFile(handle, renamedName.Pointer);
            Assert.Equal((int)LdkStatus.Success, deleteResult);
            Assert.DoesNotContain(service.FileSystem!.GetFiles(), static file => file.FileName == "HELLO2");

        }
        finally
        {
            Assert.True(HandleManager.Unregister(handle));
            HandleManager.Clear();
        }
    }
}
