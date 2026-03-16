using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;
using System.Runtime.InteropServices;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeOpenDiskExportsTest
{
    [Fact]
    public void OpenDisk_ReturnsOwnedHandleForExistingImage()
    {
        HandleManager.Clear();

        using var disk = new TempFormattedDiskScope();
        using var path = new Utf8StringScope(disk.ImagePath);

        var handle = NativeExportInvoker.OpenDisk(path.Pointer, true);

        try
        {
            Assert.True(handle > 0);
            Assert.Equal(1, NativeExportInvoker.IsHandleValid(handle));
            Assert.Equal(0, NativeExportInvoker.GetHandleIsWritable(handle));
            Assert.True(NativeExportInvoker.GetOpenHandleCount() >= 1);

            var buffer = Marshal.AllocHGlobal(128);
            try
            {
                var length = NativeExportInvoker.GetHandleSourceOperation(handle, buffer, 128);
                Assert.Equal("open-disk", Marshal.PtrToStringUTF8(buffer, length));
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
        }
    }

    [Fact]
    public void OpenDisk_ReturnsInvalidArgumentForEmptyPath()
    {
        using var path = new Utf8StringScope(string.Empty);
        var result = NativeExportInvoker.OpenDisk(path.Pointer, true);
        Assert.Equal((int)LdkStatus.ErrorInvalidArgument, result);
    }
}
