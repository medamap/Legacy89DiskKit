using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeSurfaceCatalogTest
{
    [Fact]
    public void SupportedFileSystems_ReturnExpectedNames()
    {
        Assert.Equal(["hu-basic", "n88-basic", "msx-dos"], NativeSurfaceCatalog.GetSupportedFileSystems());
    }

    [Fact]
    public void SupportedPlatforms_ReturnExpectedNames()
    {
        Assert.Equal(["X1", "PC88", "MSX"], NativeSurfaceCatalog.GetSupportedPlatforms());
    }

    [Fact]
    public void SupportedImageFormats_ReturnExpectedNames()
    {
        Assert.Equal(["d88", "d77", "2d", "dsk"], NativeSurfaceCatalog.GetSupportedImageFormats());
    }
}
