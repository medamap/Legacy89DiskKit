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
public class NativeDiskExportsTest
{
    [Fact]
    public void GetFileSystemInfoAndFilesCount_ReturnExpectedNativeSurface()
    {
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());
        HandleManager.Clear();

        using var disk = new TempFormattedDiskScope();
        using var service = CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);
        service.FileSystem!.WriteFile(
            "HELLO",
            [0x01, 0x02, 0x03],
            new ExtendedFileAttributes(DiskFileAttributes.None, 0, false));

        var handle = HandleManager.Register(service.Session!);

        try
        {
            var info = new NativeFileSystemInfo();
            var infoResult = NativeExportInvoker.GetFileSystemInfo(handle, NativeStructPointer.Alloc(info, out var infoPtr));
            Assert.Equal((int)LdkStatus.Success, infoResult);

            info = NativeStructPointer.ReadAndFree<NativeFileSystemInfo>(infoPtr);
            Assert.Equal("Hu-BASIC", info.FileSystemName);
            Assert.Equal("X1", info.PlatformId);

            var metadata = new NativeDiskContainerMetadata();
            var metadataResult = NativeExportInvoker.GetContainerMetadata(handle, NativeStructPointer.Alloc(metadata, out var metadataPtr));
            Assert.Equal((int)LdkStatus.Success, metadataResult);

            metadata = NativeStructPointer.ReadAndFree<NativeDiskContainerMetadata>(metadataPtr);
            Assert.StartsWith("d88", metadata.ImageFormat);
            Assert.Equal(40, metadata.Cylinders);
            Assert.Equal(2, metadata.Heads);
            Assert.Equal(16, metadata.SectorsPerTrack);
            Assert.Equal(256, metadata.BytesPerSector);
            Assert.Equal(0, metadata.IsWriteProtected);

            using var entryBuffer = new NativeFileEntryBufferScope(8);
            var entryCount = NativeExportInvoker.GetFiles(handle, entryBuffer.Pointer, entryBuffer.Capacity);
            Assert.True(entryCount >= 1);
        }
        finally
        {
            HandleManager.Clear();
        }
    }

    [Fact]
    public void GetFileSystemInfo_ReturnsInvalidHandleForUnknownHandle()
    {
        HandleManager.Clear();

        var info = new NativeFileSystemInfo();
        NativeStructPointer.Alloc(info, out var infoPtr);

        try
        {
            var result = NativeExportInvoker.GetFileSystemInfo(-999, infoPtr);
            Assert.Equal((int)LdkStatus.ErrorInvalidHandle, result);
        }
        finally
        {
            Marshal.FreeHGlobal(infoPtr);
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

internal static class NativeStructPointer
{
    public static IntPtr Alloc<T>(T value, out IntPtr pointer) where T : struct
    {
        pointer = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(value, pointer, false);
        return pointer;
    }

    public static T ReadAndFree<T>(IntPtr pointer) where T : struct
    {
        try
        {
            return Marshal.PtrToStructure<T>(pointer);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }
}
