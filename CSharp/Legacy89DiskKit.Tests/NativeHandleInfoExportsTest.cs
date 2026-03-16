using System.Runtime.InteropServices;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeHandleInfoExportsTest
{
    [Fact]
    public void OpenDiskHandle_ReportsReadOnlySourceAndSummary()
    {
        using var disk = new TempFormattedDiskScope();
        using var path = new Utf8StringScope(disk.ImagePath);

        var handle = NativeExportInvoker.OpenDisk(path.Pointer, true);
        Assert.True(handle > 0);

        var sourceBuffer = Marshal.AllocHGlobal(128);
        var summaryBuffer = Marshal.AllocHGlobal(128);
        try
        {
            var sourceLength = NativeExportInvoker.GetHandleSourceOperation(handle, sourceBuffer, 128);
            var summaryLength = NativeExportInvoker.GetHandleSummary(handle, summaryBuffer, 128);

            Assert.Equal("open-disk", Marshal.PtrToStringUTF8(sourceBuffer, sourceLength));
            Assert.Equal(0, NativeExportInvoker.GetHandleIsWritable(handle));
            Assert.Equal("open-disk:read-only", Marshal.PtrToStringUTF8(summaryBuffer, summaryLength));
        }
        finally
        {
            Marshal.FreeHGlobal(sourceBuffer);
            Marshal.FreeHGlobal(summaryBuffer);
            NativeExportInvoker.CloseAllHandles();
        }
    }

    [Fact]
    public void HandleInfoExports_ReturnInvalidHandleForUnknownHandle()
    {
        var buffer = Marshal.AllocHGlobal(128);
        try
        {
            Assert.Equal(-2, NativeExportInvoker.GetHandleSourceOperation(9999, buffer, 128));
            Assert.Equal(-2, NativeExportInvoker.GetHandleIsWritable(9999));
            Assert.Equal(-2, NativeExportInvoker.GetHandleSummary(9999, buffer, 128));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
