using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeOpenModeCatalogTest
{
    [Fact]
    public void GetModes_ContainsExpectedModeSet()
    {
        var modes = NativeOpenModeCatalog.GetModes();

        Assert.Equal(3, modes.Count);
        Assert.Contains("open-disk:read-only", modes);
        Assert.Contains("open-disk:writable", modes);
        Assert.Contains("create-disk:writable", modes);
    }

    [Fact]
    public void GetOpenModeSummary_DescribesWritableCreateHandles()
    {
        var summary = NativeOpenModeCatalog.GetOpenModeSummary();

        Assert.Contains("read-only", summary);
        Assert.Contains("writable", summary);
        Assert.Contains("create-disk", summary);
    }
}
