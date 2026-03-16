using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeStatusExportsTest
{
    [Fact]
    public void StatusExports_ReturnExpectedCatalogEntries()
    {
        var entries = NativeStatusCatalog.GetEntries();
        Assert.Equal(entries.Count, NativeExportInvoker.GetStatusCount());
        Assert.Equal((int)LdkStatus.Success, NativeExportInvoker.GetStatusCodeAt(0));
        Assert.Equal((int)LdkStatus.ErrorBufferTooSmall, NativeExportInvoker.GetStatusCodeAt(entries.Count - 1));
        Assert.Equal("success", ReadStatusNameAt(0));
        Assert.Equal("error-buffer-too-small", ReadStatusNameAt(entries.Count - 1));
    }

    [Fact]
    public void StatusExports_ReturnInvalidArgumentForOutOfRangeIndex()
    {
        var buffer = Marshal.AllocHGlobal(32);
        try
        {
            Assert.Equal((int)LdkStatus.ErrorInvalidArgument, NativeExportInvoker.GetStatusCodeAt(999));
            Assert.Equal((int)LdkStatus.ErrorInvalidArgument, NativeExportInvoker.GetStatusNameAt(999, buffer, 32));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string ReadStatusNameAt(int index)
    {
        var buffer = Marshal.AllocHGlobal(64);
        try
        {
            var length = NativeExportInvoker.GetStatusNameAt(index, buffer, 64);
            return Marshal.PtrToStringUTF8(buffer, length) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
