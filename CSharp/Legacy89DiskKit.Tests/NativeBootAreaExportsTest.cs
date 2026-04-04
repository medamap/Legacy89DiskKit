using Legacy89DiskKit.Native.Application;
using System.Runtime.InteropServices;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeBootAreaExportsTest
{
    [Fact]
    public void ReadAndWriteBootArea_RoundTripsThroughNativeExports()
    {
        HandleManager.Clear();
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        using var disk = new TempFormattedDiskScope();
        using var service = CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);

        var handle = HandleManager.Register(service.Session!);

        try
        {
            var bootData = Enumerable.Range(0, 128).Select(static i => (byte)i).ToArray();
            var writeBuffer = Marshal.AllocHGlobal(bootData.Length);
            Marshal.Copy(bootData, 0, writeBuffer, bootData.Length);

            try
            {
                var writeResult = NativeExportInvoker.WriteBootArea(handle, writeBuffer, bootData.Length);
                Assert.Equal((int)LdkStatus.Success, writeResult);
            }
            finally
            {
                Marshal.FreeHGlobal(writeBuffer);
            }

            var readBuffer = Marshal.AllocHGlobal(256);
            try
            {
                var readLength = NativeExportInvoker.ReadBootArea(handle, readBuffer, 256);
                Assert.True(readLength >= bootData.Length);

                var actual = new byte[bootData.Length];
                Marshal.Copy(readBuffer, actual, 0, bootData.Length);
                Assert.Equal(bootData, actual);
            }
            finally
            {
                Marshal.FreeHGlobal(readBuffer);
            }
        }
        finally
        {
            HandleManager.Clear();
        }
    }

    [Fact]
    public void ReadBootArea_ReturnsInvalidHandleForUnknownHandle()
    {
        var buffer = Marshal.AllocHGlobal(32);
        try
        {
            var result = NativeExportInvoker.ReadBootArea(-11, buffer, 32);
            Assert.Equal((int)LdkStatus.ErrorInvalidHandle, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
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
