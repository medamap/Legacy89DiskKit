using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.Drive.Medium;
using Legacy89DiskKit.Infrastructure.Fdc.Medium;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class RawDiskBackedMediumTest
{
    [Fact]
    public void RawDiskBackedSectorAddressableMedium_CanReadDecodedSectorData()
    {
        using var container = RawDiskContainer.CreateNewInMemory(Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, CreateSector(256, 0x44));

        ISectorAddressableMedium medium = new RawDiskBackedSectorAddressableMedium(container);

        var data = medium.ReadSector(0, 0, 1);

        Assert.Equal("raw-sector-image", medium.MediumKind);
        Assert.True(medium.SectorExists(0, 0, 1));
        Assert.Equal(0x44, data[0]);
    }

    [Fact]
    public void RawDiskBackedControllerFacingMedium_CanServeReadSectorLikeFlow()
    {
        using var container = RawDiskContainer.CreateNewInMemory(Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, CreateSector(256, 0x7E));

        IControllerFacingMedium medium = new RawDiskBackedControllerFacingMedium(container);

        medium.Reset();
        medium.SelectSide(0);
        medium.WriteTrackRegister(0);
        medium.WriteSectorRegister(1);
        medium.WriteCommand(0x80);

        Assert.Equal("raw-sector-image", medium.MediumKind);
        Assert.True(medium.IsReady);
        Assert.False(medium.IsWriteProtected);
        Assert.True(medium.IsIrqAsserted);
        Assert.True(medium.IsDrqAsserted);
        Assert.Equal(0x7E, medium.ReadDataRegister());
    }

    [Fact]
    public void RawDiskBackedControllerFacingMedium_ReturnsRecordNotFoundStatusForMissingSector()
    {
        using var container = RawDiskContainer.CreateNewInMemory(Domain.DiskImage.Model.DiskType.TwoD);
        IControllerFacingMedium medium = new RawDiskBackedControllerFacingMedium(container);

        medium.Reset();
        medium.SelectSide(0);
        medium.WriteTrackRegister(0);
        medium.WriteSectorRegister(99);
        medium.WriteCommand(0x80);

        Assert.Equal(0x10, medium.ReadStatus());
        Assert.True(medium.IsIrqAsserted);
        Assert.False(medium.IsDrqAsserted);
    }

    private static byte[] CreateSector(int size, byte firstByte)
    {
        var data = new byte[size];
        data[0] = firstByte;
        return data;
    }
}
