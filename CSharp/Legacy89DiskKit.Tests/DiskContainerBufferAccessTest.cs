using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class DiskContainerBufferAccessTest
{
    [Fact]
    public void RawDiskContainer_CanRoundTripThroughImageBuffer()
    {
        using var container = RawDiskContainer.CreateNewInMemory(DiskType.TwoD);
        var sectorData = Enumerable.Repeat((byte)0x5A, 256).ToArray();

        container.WriteSector(0, 0, 1, sectorData);

        var imageData = container.ToImageData();
        using var reopened = new RawDiskContainer(imageData, true);

        Assert.Equal(DiskType.TwoD, reopened.DiskType);
        Assert.Equal(sectorData, reopened.ReadSector(0, 0, 1));
    }

    [Fact]
    public void D88DiskContainer_CanRoundTripThroughImageBuffer()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TEST", DiskType.TwoD);
        var sectorData = Enumerable.Repeat((byte)0xA5, 256).ToArray();

        container.WriteSector(0, 0, 1, sectorData);

        var imageData = container.ToImageData();
        using var reopened = new D88DiskContainer(imageData, true);

        Assert.Equal(DiskType.TwoD, reopened.DiskType);
        Assert.Equal(sectorData, reopened.ReadSector(0, 0, 1));
    }
}
