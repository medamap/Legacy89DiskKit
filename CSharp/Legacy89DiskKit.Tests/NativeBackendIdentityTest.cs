using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeBackendIdentityTest
{
    [Fact]
    public void BackendIdentity_ReturnsExpectedManagedValues()
    {
        Assert.Equal("managed-bridge", NativeBackendIdentity.BackendKind);
        Assert.Equal("Legacy89DiskKit.NativeInterop", NativeBackendIdentity.BackendImplementation);
        Assert.Equal("Legacy89DiskKit.Application", NativeBackendIdentity.BackendTarget);
        Assert.Contains("managed-bridge", NativeBackendIdentity.GetBackendSummary());
    }

    [Fact]
    public void BackendIdentityExports_ReturnExpectedStrings()
    {
        Assert.Equal("managed-bridge", ReadSummary(NativeExportInvoker.GetBackendKind));
        Assert.Equal("Legacy89DiskKit.NativeInterop", ReadSummary(NativeExportInvoker.GetBackendImplementation));
        Assert.Equal("Legacy89DiskKit.Application", ReadSummary(NativeExportInvoker.GetBackendTarget));
        Assert.Contains("managed-bridge", ReadSummary(NativeExportInvoker.GetBackendSummary));
    }

    private static string ReadSummary(Func<IntPtr, int, int> reader)
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            var length = reader(buffer, 256);
            return Marshal.PtrToStringUTF8(buffer, length) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
