using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.DiskImage.D88;
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
}
