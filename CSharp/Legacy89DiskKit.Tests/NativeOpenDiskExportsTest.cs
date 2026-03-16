using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

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
            Assert.True(NativeExportInvoker.GetOpenHandleCount() >= 1);
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
