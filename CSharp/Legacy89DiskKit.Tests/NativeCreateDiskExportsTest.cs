using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;
using System.Runtime.InteropServices;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeCreateDiskExportsTest
{
    [Fact]
    public void CreateDisk_ReturnsOwnedHandleForWritableImage()
    {
        HandleManager.Clear();

        var imagePath = Path.Combine(Path.GetTempPath(), $"ldk-native-create-{Guid.NewGuid():N}.d88");
        using var path = new Utf8StringScope(imagePath);
        using var name = new Utf8StringScope("CRTTEST");

        var handle = NativeExportInvoker.CreateDisk(path.Pointer, (int)LdkDiskType.TwoD, name.Pointer);

        try
        {
            Assert.True(handle > 0);
            Assert.Equal(1, NativeExportInvoker.IsHandleValid(handle));
            Assert.Equal(1, NativeExportInvoker.GetHandleIsWritable(handle));
            Assert.True(File.Exists(imagePath));

            var buffer = Marshal.AllocHGlobal(128);
            try
            {
                var length = NativeExportInvoker.GetHandleSourceOperation(handle, buffer, 128);
                Assert.Equal("create-disk", Marshal.PtrToStringUTF8(buffer, length));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            if (handle > 0)
            {
                NativeExportInvoker.CloseDisk(handle);
            }

            HandleManager.Clear();

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }

    [Fact]
    public void CreateDisk_ReturnsInvalidArgumentForEmptyPath()
    {
        using var path = new Utf8StringScope(string.Empty);
        using var name = new Utf8StringScope("CRTTEST");

        var result = NativeExportInvoker.CreateDisk(path.Pointer, (int)LdkDiskType.TwoD, name.Pointer);

        Assert.Equal((int)LdkStatus.ErrorInvalidArgument, result);
    }
}
