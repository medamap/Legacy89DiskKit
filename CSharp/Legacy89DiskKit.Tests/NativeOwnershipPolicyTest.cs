using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeOwnershipPolicyTest
{
    [Fact]
    public void OwnershipPolicy_ReturnsExpectedManagedSummaries()
    {
        Assert.Contains("open/create return owned handles", NativeOwnershipPolicy.GetHandleLifecycleSummary());
        Assert.Contains("fixed buffers truncate", NativeOwnershipPolicy.GetBufferStringPolicySummary());
    }

    [Fact]
    public void OwnershipPolicyExports_ReturnExpectedSummaries()
    {
        Assert.Contains("owned handles", ReadSummary(NativeExportInvoker.GetHandleLifecycleSummary));
        Assert.Contains("truncate", ReadSummary(NativeExportInvoker.GetBufferStringPolicySummary));
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
