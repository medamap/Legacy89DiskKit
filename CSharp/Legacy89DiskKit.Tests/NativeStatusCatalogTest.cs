using Legacy89DiskKit.NativeInterop.Exports;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeStatusCatalogTest
{
    [Fact]
    public void Entries_ReturnExpectedStableOrdering()
    {
        var entries = NativeStatusCatalog.GetEntries();

        Assert.Equal(9, entries.Count);
        Assert.Equal(LdkStatus.Success, entries[0].Status);
        Assert.Equal("success", entries[0].Name);
        Assert.Equal(LdkStatus.ErrorBufferTooSmall, entries[^1].Status);
        Assert.Equal("error-buffer-too-small", entries[^1].Name);
    }

    [Fact]
    public void GetName_ReturnsUnknownForMissingStatus()
    {
        Assert.Equal("unknown-status", NativeStatusCatalog.GetName((LdkStatus)12345));
    }
}
