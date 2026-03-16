using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeHandleValueExportsTest
{
    [Fact]
    public void GetInvalidHandleValue_ReturnsConfiguredSentinel()
    {
        Assert.Equal(NativeHandleContract.InvalidHandleValue, NativeExportInvoker.GetInvalidHandleValue());
    }

    [Fact]
    public void GetHandleValueSummary_WritesConfiguredSummary()
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            var length = NativeExportInvoker.GetHandleValueSummary(buffer, 256);
            var text = Marshal.PtrToStringUTF8(buffer, length);
            Assert.Equal(NativeHandleContract.GetHandleValueSummary(), text);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
