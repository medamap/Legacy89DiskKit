using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.DiskImage.Infrastructure.D88;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class D88ImageParserTest
{
    [Fact]
    public void D88ImageParser_CanParseHeaderFromImageBytes()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", DiskType.TwoD);
        var imageData = container.ToImageData();

        var header = D88ImageParser.ParseHeader(imageData);

        Assert.Equal("TESTDISK", header.ImageName);
        Assert.Equal(DiskType.TwoD, header.MediaType);
        Assert.False(header.WriteProtect);
        Assert.True(header.DiskSize > 0);
    }

    [Fact]
    public void D88ImageParser_CanParseTrackSectorsFromImageBytes()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x77, 0x88, 0x99 });
        var imageData = container.ToImageData();

        var header = D88ImageParser.ParseHeader(imageData);
        var sectors = D88ImageParser.ParseSectors(imageData, header);

        Assert.True(sectors.ContainsKey((0, 0, 1)));
        Assert.Equal(0x77, sectors[(0, 0, 1)].Data[0]);
    }

    [Fact]
    public void D88ImageParser_CanBuildReadOnlyImageLayout()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x77, 0x88, 0x99 });
        var imageData = container.ToImageData();

        var layout = D88ImageParser.ParseImage(imageData);

        Assert.Equal("d88-sector-container", layout.Metadata.ImageFormat);
        Assert.Equal(DiskType.TwoD, layout.Metadata.DiskType);
        Assert.Contains(layout.Sectors, s => s.Sector.Cylinder == 0 && s.Sector.Head == 0 && s.Sector.Sector == 1);
        Assert.Equal(0x77, layout.Sectors.First(s => s.Sector.Cylinder == 0 && s.Sector.Head == 0 && s.Sector.Sector == 1).Data[0]);
    }
}
