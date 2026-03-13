using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Raw;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class RawDiskGeometryTest
{
    [Theory]
    [InlineData(327680, 40, 2, 16, 256, DiskType.TwoD)]
    [InlineData(655360, 80, 2, 16, 256, DiskType.TwoDD)]
    [InlineData(737280, 80, 2, 9, 512, DiskType.TwoDD)]
    public void RawDiskGeometryDetector_CanDetectKnownFormats(long size, int cylinders, int sides, int spt, int bps, DiskType diskType)
    {
        var geometry = RawDiskGeometryDetector.Detect(size);

        Assert.Equal(cylinders, geometry.Cylinders);
        Assert.Equal(sides, geometry.Sides);
        Assert.Equal(spt, geometry.SectorsPerTrack);
        Assert.Equal(bps, geometry.BytesPerSector);
        Assert.Equal(diskType, geometry.DiskType);
    }

    [Fact]
    public void RawSectorAddressCalculator_CanCalculateSectorOffsets()
    {
        var geometry = new RawDiskGeometry(40, 2, 16, 256, DiskType.TwoD);
        var calculator = new RawSectorAddressCalculator(geometry);

        Assert.Equal(0, calculator.CalculateOffset(0, 0, 1));
        Assert.Equal(256, calculator.CalculateOffset(0, 0, 2));
        Assert.Equal(4096, calculator.CalculateOffset(0, 1, 1));
        Assert.Equal(8192, calculator.CalculateOffset(1, 0, 1));
    }

    [Fact]
    public void RawSectorAddressCalculator_RejectsInvalidAddress()
    {
        var geometry = new RawDiskGeometry(40, 2, 16, 256, DiskType.TwoD);
        var calculator = new RawSectorAddressCalculator(geometry);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => calculator.CalculateOffset(0, 0, 17));

        Assert.Contains("Invalid sector address", ex.Message);
    }
}
