using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.Drive.Medium;
using Legacy89DiskKit.Infrastructure.Fdc.Medium;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class D88BackedMediumTest
{
    [Fact]
    public void D88BackedSectorAddressableMedium_CanReadDecodedSectorData()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0xAB, 0xCD, 0xEF });

        ISectorAddressableMedium medium = new D88BackedSectorAddressableMedium(container);

        var data = medium.ReadSector(0, 0, 1);

        Assert.Equal("d88-family", medium.MediumKind);
        Assert.True(medium.SectorExists(0, 0, 1));
        Assert.Equal(new byte[] { 0xAB, 0xCD, 0xEF }, data);
    }

    [Fact]
    public void D88BackedControllerFacingMedium_CanServeReadSectorLikeFlow()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x5A, 0x00, 0x00 });

        IControllerFacingMedium medium = new D88BackedControllerFacingMedium(container);

        medium.Reset();
        medium.SelectSide(0);
        medium.WriteTrackRegister(0);
        medium.WriteSectorRegister(1);
        medium.WriteCommand(0x80);

        Assert.Equal("d88-family", medium.MediumKind);
        Assert.True(medium.IsReady);
        Assert.False(medium.IsWriteProtected);
        Assert.True(medium.IsIrqAsserted);
        Assert.True(medium.IsDrqAsserted);
        Assert.Equal(0x5A, medium.ReadDataRegister());
    }

    [Fact]
    public void D88BackedControllerFacingMedium_ReturnsRecordNotFoundStatusForMissingSector()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        IControllerFacingMedium medium = new D88BackedControllerFacingMedium(container);

        medium.Reset();
        medium.SelectSide(0);
        medium.WriteTrackRegister(0);
        medium.WriteSectorRegister(99);
        medium.WriteCommand(0x80);

        Assert.Equal(0x10, medium.ReadStatus());
        Assert.True(medium.IsIrqAsserted);
        Assert.False(medium.IsDrqAsserted);
    }
}
