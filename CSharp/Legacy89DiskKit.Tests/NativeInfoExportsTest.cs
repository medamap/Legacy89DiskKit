using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeInfoExportsTest
{
    [Fact]
    public void GetAbiVersion_ReturnsStableVersion()
    {
        Assert.Equal(NativeSurfaceInfo.AbiVersion, NativeInfoExports.GetAbiVersion());
    }

    [Fact]
    public void GetCapabilityFlags_ReturnsExpectedFlags()
    {
        Assert.Equal(NativeSurfaceInfo.GetCapabilityFlags(), NativeInfoExports.GetCapabilityFlags());
    }

    [Fact]
    public void GetCapabilitySummary_WritesUtf8Summary()
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            var length = NativeInfoExports.GetCapabilitySummary(buffer, 256);
            var text = Marshal.PtrToStringUTF8(buffer, length);
            Assert.Equal(NativeSurfaceInfo.GetCapabilitySummary(), text);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void GetStatusName_WritesKnownStatusName()
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            var length = NativeInfoExports.GetStatusName(0, buffer, 256);
            var text = Marshal.PtrToStringUTF8(buffer, length);
            Assert.Equal("success", text);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
