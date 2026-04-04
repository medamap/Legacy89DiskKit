using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeDiskContainerMetadataFactoryTest
{
    [Fact]
    public void Create_MapsManagedMetadataToNativeShape()
    {
        var metadata = new DiskContainerMetadata(
            "d88",
            DiskType.TwoD,
            new DiskGeometryInfo(40, 2, 16, 256),
            true,
            348848);

        var native = NativeDiskContainerMetadataFactory.Create(metadata);

        Assert.Equal("d88", native.ImageFormat);
        Assert.Equal((int)DiskType.TwoD, native.DiskType);
        Assert.Equal(40, native.Cylinders);
        Assert.Equal(2, native.Heads);
        Assert.Equal(16, native.SectorsPerTrack);
        Assert.Equal(256, native.BytesPerSector);
        Assert.Equal(1, native.IsWriteProtected);
        Assert.Equal(348848, native.DeclaredImageSize);
    }
}
