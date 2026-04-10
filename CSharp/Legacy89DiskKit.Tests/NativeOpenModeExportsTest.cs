using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeOpenModeExportsTest
{
    [Fact]
    public void GetOpenModeSummary_WritesConfiguredSummary()
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            var length = NativeExportInvoker.GetOpenModeSummary(buffer, 256);
            var text = Marshal.PtrToStringUTF8(buffer, length);
            Assert.Equal(NativeOpenModeCatalog.GetOpenModeSummary(), text);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void GetOpenModeCountAndNameAt_ReturnConfiguredModes()
    {
        var modes = NativeOpenModeCatalog.GetModes();
        Assert.Equal(modes.Count, NativeExportInvoker.GetOpenModeCount());

        var buffer = Marshal.AllocHGlobal(128);
        try
        {
            var length = NativeExportInvoker.GetOpenModeNameAt(0, buffer, 128);
            Assert.Equal(modes[0], Marshal.PtrToStringUTF8(buffer, length));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void GetOpenModeNameAt_ReturnsInvalidArgumentForOutOfRangeIndex()
    {
        var buffer = Marshal.AllocHGlobal(128);
        try
        {
            var status = NativeExportInvoker.GetOpenModeNameAt(99, buffer, 128);
            Assert.Equal((int)LdkStatus.ErrorInvalidArgument, status);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
