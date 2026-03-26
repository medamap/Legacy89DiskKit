using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosDomainModelTest
{
    private static XDosDirectoryEntry MakeEntry(
        ushort rawFileType,
        string fileName = "TEST",
        ushort startAddress = 0,
        ushort sizeLow = 0,
        ushort execAddressOrSizeHigh = 0,
        uint timestampRaw = 0,
        byte attribute = 0,
        byte famTrack = 0,
        byte famSector = 0,
        byte famRecord = 0)
    {
        var rawName = new byte[16];
        System.Text.Encoding.Latin1.GetBytes(fileName.PadRight(16, ' ')).CopyTo(rawName, 0);
        return new XDosDirectoryEntry(
            rawFileType, fileName, rawName,
            startAddress, sizeLow, execAddressOrSizeHigh,
            timestampRaw, attribute,
            new XDosFamPointer(famTrack, famSector, famRecord));
    }

    [Fact]
    public void DirectoryEntry_RawFileType_IsUInt16()
    {
        Assert.Equal(typeof(ushort), typeof(XDosDirectoryEntry).GetProperty("RawFileType")!.PropertyType);
    }

    [Fact]
    public void DirectoryEntry_IsKilled_WhenRawFileTypeIsZero()
    {
        var entry = MakeEntry(0x0000);
        Assert.True(entry.IsKilled);
        Assert.True(entry.IsEmpty);
        Assert.False(entry.IsEnd);
    }

    [Fact]
    public void DirectoryEntry_IsEnd_WhenRawFileTypeIsFFFF()
    {
        var entry = MakeEntry(0xFFFF);
        Assert.True(entry.IsEnd);
        Assert.True(entry.IsEmpty);
        Assert.False(entry.IsKilled);
    }

    [Fact]
    public void DirectoryEntry_IsNotEmpty_ForNormalEntry()
    {
        var entry = MakeEntry((ushort)XDosFileType.Bin);
        Assert.False(entry.IsEmpty);
        Assert.False(entry.IsKilled);
        Assert.False(entry.IsEnd);
    }

    [Fact]
    public void DirectoryEntry_FileType_CastsFromRawFileType()
    {
        var entry = MakeEntry((ushort)XDosFileType.Bin);
        Assert.Equal(XDosFileType.Bin, entry.FileType);
    }

    [Fact]
    public void DirectoryEntry_FamPointer_HoldsTrackSectorRecord()
    {
        var entry = MakeEntry(0x0100, famTrack: 2, famSector: 1, famRecord: 1);
        Assert.Equal(2, entry.FamPointer.Track);
        Assert.Equal(1, entry.FamPointer.Sector);
        Assert.Equal(1, entry.FamPointer.Record);
    }

    [Fact]
    public void DirectoryEntry_Attribute_IsRawBytePreserved()
    {
        var entry = MakeEntry(0x0100, attribute: 0xB5);
        Assert.Equal(0xB5, entry.Attribute);
    }

    [Fact]
    public void DirectoryEntry_FileSize_EqualsSizeLow_ForNonAsc()
    {
        var entry = MakeEntry((ushort)XDosFileType.Bin, sizeLow: 0x2E00, execAddressOrSizeHigh: 0x0001);
        Assert.Equal(0x2E00, entry.FileSize);
    }

    [Fact]
    public void DirectoryEntry_FileSize_UsesOnlySizeLow_ForSmallAsc()
    {
        var entry = MakeEntry((ushort)XDosFileType.Asc, sizeLow: 0x1234, execAddressOrSizeHigh: 0x0000);
        Assert.Equal(0x1234, entry.FileSize);
    }

    [Fact]
    public void DirectoryEntry_FileSize_CombinesSizeLowAndSizeHigh_ForLargeAsc()
    {
        var entry = MakeEntry((ushort)XDosFileType.Asc, sizeLow: 0x0000, execAddressOrSizeHigh: 0x0001);
        Assert.Equal(0x00010000, entry.FileSize);
    }

    [Fact]
    public void DirectoryEntry_FileSize_CombinesBothWords_ForLargeAsc()
    {
        var entry = MakeEntry((ushort)XDosFileType.Asc, sizeLow: 0xABCD, execAddressOrSizeHigh: 0x0002);
        Assert.Equal(0x0002ABCD, entry.FileSize);
    }

    [Theory]
    [InlineData((ushort)XDosFileType.Killed, 0x0000)]
    [InlineData((ushort)XDosFileType.Bin,    0x0100)]
    [InlineData((ushort)XDosFileType.Bas,    0x0200)]
    [InlineData((ushort)XDosFileType.Cmd,    0x0300)]
    [InlineData((ushort)XDosFileType.Asc,    0x0400)]
    [InlineData((ushort)XDosFileType.Sub,    0x0500)]
    [InlineData((ushort)XDosFileType.Bat,    0x0600)]
    [InlineData((ushort)XDosFileType.Sys,    0x0700)]
    [InlineData((ushort)XDosFileType.Dic,    0x0800)]
    [InlineData((ushort)XDosFileType.Dir,    0x8000)]
    [InlineData((ushort)XDosFileType.End,    0xFFFF)]
    public void XDosFileType_Values_MatchSpec(ushort actual, ushort expected)
    {
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void XDosFileType_BaseType_IsUInt16()
    {
        Assert.Equal(typeof(ushort), Enum.GetUnderlyingType(typeof(XDosFileType)));
    }

    [Fact]
    public void XDosFamPointer_HoldsThreeSeparateBytes()
    {
        var p = new XDosFamPointer(Track: 2, Sector: 1, Record: 1);
        Assert.Equal(2, p.Track);
        Assert.Equal(1, p.Sector);
        Assert.Equal(1, p.Record);
    }

    [Fact]
    public void XDosDirParser_2HD_ParsesUpToSector16()
    {
        var container = new Fake2HdContainer();
        var parser = new XDosDirParser();
        parser.Parse(container);
        Assert.True(container.MaxRequestedSector >= 16,
            $"Expected sector >= 16 to be requested, but max was {container.MaxRequestedSector}");
    }

    [Fact]
    public void XDosDirParser_2DD_StopsAtSector10()
    {
        var container = new Fake2DdContainer();
        var parser = new XDosDirParser();
        parser.Parse(container);
        Assert.True(container.MaxRequestedSector <= 10,
            $"Expected sector <= 10, but max was {container.MaxRequestedSector}");
        Assert.False(container.RequestedSectors.Contains(11),
            "Sector 11 should not be requested for 2DD");
    }

    private class Fake2HdContainer : IDiskContainer
    {
        public int MaxRequestedSector { get; private set; }
        public string FilePath  => string.Empty;
        public DiskType DiskType => DiskType.TwoHD;
        public bool IsReadOnly   => true;
        public bool SectorExists(int c, int h, int r)
        {
            if (c == 0 && h == 1 && r >= 2 && r <= 16) { MaxRequestedSector = Math.Max(MaxRequestedSector, r); return true; }
            return false;
        }
        public byte[] ReadSector(int c, int h, int r) => new byte[512];
        public byte[] ReadSector(int c, int h, int r, bool allowCorrupted) => new byte[512];
        public void WriteSector(int c, int h, int r, byte[] data) { }
        public DiskContainerMetadata GetMetadata() => throw new NotSupportedException();
        public IEnumerable<SectorInfo> GetAllSectors() => Enumerable.Empty<SectorInfo>();
        public void Save() { }
        public void SaveAs(string filePath) { }
        public void Dispose() { }
    }

    private class Fake2DdContainer : IDiskContainer
    {
        public int MaxRequestedSector { get; private set; }
        public HashSet<int> RequestedSectors { get; } = new();
        public string FilePath  => string.Empty;
        public DiskType DiskType => DiskType.TwoDD;
        public bool IsReadOnly   => true;
        public bool SectorExists(int c, int h, int r)
        {
            if (c == 0 && h == 1 && r >= 2 && r <= 10)  { MaxRequestedSector = Math.Max(MaxRequestedSector, r); RequestedSectors.Add(r); return true; }
            if (c == 0 && h == 1 && r > 10) { RequestedSectors.Add(r); return false; }
            return false;
        }
        public byte[] ReadSector(int c, int h, int r) => new byte[512];
        public byte[] ReadSector(int c, int h, int r, bool allowCorrupted) => new byte[512];
        public void WriteSector(int c, int h, int r, byte[] data) { }
        public DiskContainerMetadata GetMetadata() => throw new NotSupportedException();
        public IEnumerable<SectorInfo> GetAllSectors() => Enumerable.Empty<SectorInfo>();
        public void Save() { }
        public void SaveAs(string filePath) { }
        public void Dispose() { }
    }
}
