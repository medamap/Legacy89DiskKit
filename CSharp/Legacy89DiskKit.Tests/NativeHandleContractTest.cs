using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeHandleContractTest
{
    [Fact]
    public void InvalidHandleValue_IsZero()
    {
        Assert.Equal(0, NativeHandleContract.InvalidHandleValue);
    }

    [Fact]
    public void GetHandleValueSummary_DescribesPositiveAndNegativeRanges()
    {
        var summary = NativeHandleContract.GetHandleValueSummary();

        Assert.Contains("positive handles", summary);
        Assert.Contains("zero", summary);
        Assert.Contains("negative values", summary);
    }
}
