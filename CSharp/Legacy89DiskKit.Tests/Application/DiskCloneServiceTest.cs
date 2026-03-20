using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Xunit;

namespace Legacy89DiskKit.Tests.Application;

public class DiskCloneServiceTest
{
    private class FakeDiskContainer : IDiskContainer
    {
        public string FilePath => "fake.d88";
        public bool IsReadOnly => false;
        public DiskType DiskType { get; set; }
        public List<SectorInfo> Sectors { get; set; } = new();
        public Dictionary<(int, int, int), byte[]> Data { get; set; } = new();
        public bool ReadThrowAtSector1 { get; set; }

        public DiskContainerMetadata GetMetadata() => new DiskContainerMetadata("Fake", DiskType, new DiskGeometryInfo(0, 0, 0, 0), false, 0);
        public byte[] ReadSector(int c, int h, int s) => ReadSector(c, h, s, false);
        public byte[] ReadSector(int c, int h, int s, bool allowCorrupted)
        {
            if (ReadThrowAtSector1 && s == 1) throw new Exception("Read error");
            return Data[(c, h, s)];
        }
        public void WriteSector(int c, int h, int s, byte[] data) => Data[(c, h, s)] = data;
        public bool SectorExists(int c, int h, int s) => Data.ContainsKey((c, h, s));
        public bool CylinderExists(int c) => Sectors.Any(s => s.Cylinder == c);
        public bool HeadExists(int c, int h) => Sectors.Any(s => s.Cylinder == c && s.Head == h);
        public IEnumerable<SectorInfo> GetAllSectors() => Sectors;
        public void Save() { }
        public void SaveAs(string path) { }
        public void Dispose() { }
    }

    [Fact]
    public void CopySectors_SameDiskType_CopiesAllSectors()
    {
        var source = new FakeDiskContainer { DiskType = DiskType.TwoD };
        var destination = new FakeDiskContainer { DiskType = DiskType.TwoD };
        
        source.Sectors.Add(new SectorInfo(0, 0, 1, 256));
        source.Sectors.Add(new SectorInfo(0, 0, 2, 256));
        source.Data[(0, 0, 1)] = new byte[256];
        source.Data[(0, 0, 2)] = new byte[256];
        
        var service = new DiskCloneService(null!, null!);
        var result = service.CopySectors(source, destination);
        
        Assert.Equal(1, result.tracksCopied);
        Assert.Equal(0, result.sectorsSkipped);
        Assert.True(destination.Data.ContainsKey((0, 0, 1)));
        Assert.True(destination.Data.ContainsKey((0, 0, 2)));
    }

    [Fact]
    public void CopySectors_DifferentDiskType_Throws()
    {
        var source = new FakeDiskContainer { DiskType = DiskType.TwoD };
        var destination = new FakeDiskContainer { DiskType = DiskType.TwoHD };
        
        var service = new DiskCloneService(null!, null!);
        Assert.Throws<ArgumentException>(() => service.CopySectors(source, destination));
    }

    [Fact]
    public void CopySectors_SourceReadFailure_ContinuesAndReports()
    {
        var source = new FakeDiskContainer { DiskType = DiskType.TwoD, ReadThrowAtSector1 = true };
        var destination = new FakeDiskContainer { DiskType = DiskType.TwoD };
        
        source.Sectors.Add(new SectorInfo(0, 0, 1, 256));
        source.Sectors.Add(new SectorInfo(0, 0, 2, 256));
        source.Data[(0, 0, 2)] = new byte[256];
        
        var service = new DiskCloneService(null!, null!);
        var result = service.CopySectors(source, destination, true);
        
        Assert.Equal(1, result.tracksCopied);
        Assert.Equal(1, result.sectorsSkipped);
        Assert.False(destination.Data.ContainsKey((0, 0, 1)));
        Assert.True(destination.Data.ContainsKey((0, 0, 2)));
    }
}
