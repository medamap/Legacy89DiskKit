using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeMutationPolicyTest
{
    [Fact]
    public void GetMutatingOperations_ContainsExpectedExports()
    {
        var operations = NativeMutationPolicy.GetMutatingOperations();

        Assert.Contains("ldk_create_disk", operations);
        Assert.Contains("ldk_write_file", operations);
        Assert.Contains("ldk_format", operations);
    }

    [Fact]
    public void GetMutationPolicySummary_DescribesReadOnlyAndWritableHandles()
    {
        var summary = NativeMutationPolicy.GetMutationPolicySummary();

        Assert.Contains("writable handles", summary);
        Assert.Contains("read-only handles", summary);
        Assert.Contains("create-disk", summary);
    }
}
