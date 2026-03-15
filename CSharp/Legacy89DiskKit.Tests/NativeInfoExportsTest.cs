using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeInfoExportsTest
{
    [Fact]
    public void GetAbiVersion_ReturnsStableVersion()
    {
        Assert.Equal(NativeSurfaceInfo.AbiVersion, NativeExportInvoker.GetAbiVersion());
    }

    [Fact]
    public void GetCapabilityFlags_ReturnsExpectedFlags()
    {
        Assert.Equal(NativeSurfaceInfo.GetCapabilityFlags(), NativeExportInvoker.GetCapabilityFlags());
    }

    [Fact]
    public void GetCapabilitySummary_WritesUtf8Summary()
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            var length = NativeExportInvoker.GetCapabilitySummary(buffer, 256);
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
            var length = NativeExportInvoker.GetStatusName(0, buffer, 256);
            var text = Marshal.PtrToStringUTF8(buffer, length);
            Assert.Equal("success", text);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void GetCapabilitySummary_ReturnsBufferTooSmallForTinyBuffer()
    {
        var buffer = Marshal.AllocHGlobal(4);
        try
        {
            var result = NativeExportInvoker.GetCapabilitySummary(buffer, 4);
            Assert.Equal((int)LdkStatus.ErrorBufferTooSmall, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void GetStatusName_ReturnsBufferTooSmallForTinyBuffer()
    {
        var buffer = Marshal.AllocHGlobal(3);
        try
        {
            var result = NativeExportInvoker.GetStatusName((int)LdkStatus.Success, buffer, 3);
            Assert.Equal((int)LdkStatus.ErrorBufferTooSmall, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
