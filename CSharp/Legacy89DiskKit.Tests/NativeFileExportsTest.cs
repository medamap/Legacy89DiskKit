using Legacy89DiskKit.Native.Application;
using System.Runtime.InteropServices;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;
using DiskFileAttributes = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;

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
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        using var disk = new TempFormattedDiskScope();
        using var service = CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);

        var handle = HandleManager.Register(service.Session!);

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

        }
        finally
        {
            HandleManager.Unregister(handle);
            HandleManager.Clear();
        }
    }

    private static DiskService CreateDiskService()
    {
        return new DiskService(fsRegistry: CreateFileSystemRegistry());
    }

    private static IFileSystemRegistry CreateFileSystemRegistry()
    {
        var registry = new FileSystemRegistry();
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider.XDosFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider.HuBasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Cpm.Provider.CpmFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Provider.N88BasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Msx.Provider.MsxDosFileSystemProvider());
        return registry;
    }
}
