using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeExportCatalogTest
{
    [Fact]
    public void GetEntries_ContainsKnownExportsInStableOrder()
    {
        var entries = NativeExportCatalog.GetEntries();

        Assert.NotEmpty(entries);
        Assert.Equal("ldk_open_disk", entries[0].Name);
        Assert.Equal("ldk_format", entries[^1].Name);
    }

    [Fact]
    public void GetEntries_AssignsExpectedGroups()
    {
        var entries = NativeExportCatalog.GetEntries();

        Assert.Contains(entries, entry => entry is { Name: "ldk_get_abi_version", Group: "info" });
        Assert.Contains(entries, entry => entry is { Name: "ldk_get_open_handle_count", Group: "handle" });
        Assert.Contains(entries, entry => entry is { Name: "ldk_get_file_system_info", Group: "disk" });
        Assert.Contains(entries, entry => entry is { Name: "ldk_read_file", Group: "file" });
    }
}
