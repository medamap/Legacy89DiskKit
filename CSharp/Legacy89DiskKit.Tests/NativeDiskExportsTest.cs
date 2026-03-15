using System.Runtime.InteropServices;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Exports;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeDiskExportsTest
{
    [Fact]
    public void OpenDiskAndReadFileSystemInfo_ReturnsExpectedNativeSurface()
    {
        HandleManager.Clear();

        using var disk = new TempFormattedDiskScope();
        using (var writer = Legacy89DiskKitApplication.CreateDiskService())
        {
            writer.OpenDisk(disk.ImagePath, readOnly: false);
            writer.FileSystem!.WriteFile(
                "HELLO",
                [0x01, 0x02, 0x03],
                new ExtendedFileAttributes(FileAttributes.Binary, 0, false));
        }

        using var path = new Utf8StringScope(disk.ImagePath);
        var handle = DiskExports.OpenDisk(path.Pointer, true);

        try
        {
            Assert.True(handle > 0);

            var info = new NativeFileSystemInfo();
            var infoResult = DiskExports.GetFileSystemInfo(handle, NativeStructPointer.Alloc(info, out var infoPtr));
            Assert.Equal((int)LdkStatus.Success, infoResult);

            info = NativeStructPointer.ReadAndFree<NativeFileSystemInfo>(infoPtr);
            Assert.Equal("Hu-BASIC", info.FileSystemName);
            Assert.Equal("X1", info.PlatformId);

            var countPtr = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                var countResult = DiskExports.GetFilesCount(handle, countPtr);
                Assert.Equal((int)LdkStatus.Success, countResult);
                Assert.True(Marshal.ReadInt32(countPtr) >= 1);
            }
            finally
            {
                Marshal.FreeHGlobal(countPtr);
            }
        }
        finally
        {
            Assert.Equal((int)LdkStatus.Success, DiskExports.CloseDisk(handle));
            HandleManager.Clear();
        }
    }

    [Fact]
    public void CreateDisk_ReturnsHandleThatCanBeClosed()
    {
        HandleManager.Clear();

        var imagePath = Path.Combine(Path.GetTempPath(), $"ldk-native-create-{Guid.NewGuid():N}.d88");
        using var path = new Utf8StringScope(imagePath);
        using var name = new Utf8StringScope("NATIVECRT");

        try
        {
            var handle = DiskExports.CreateDisk(path.Pointer, (int)LdkDiskType.TwoD, name.Pointer);
            Assert.True(handle > 0);
            Assert.Equal(1, NativeHandleExports.IsHandleValid(handle));
            Assert.Equal((int)LdkStatus.Success, DiskExports.CloseDisk(handle));
            Assert.Equal(0, NativeHandleExports.IsHandleValid(handle));
        }
        finally
        {
            HandleManager.Clear();

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
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
