using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class MediumAdapterContractTest
{
    [Fact]
    public void SectorAddressableMedium_CanExposeDecodedSectorView()
    {
        ISectorAddressableMedium medium = new FakeSectorAddressableMedium();

        var sector = medium.ReadSector(0, 0, 1);
        var sectors = medium.GetAllSectors().ToArray();

        Assert.True(medium.SupportsDirectImageAccess);
        Assert.True(medium.SectorExists(0, 0, 1));
        Assert.Equal(new byte[] { 0x12, 0x34 }, sector);
        Assert.Single(sectors);
        Assert.Equal(1, sectors[0].Sector);
    }

    [Fact]
    public void ControllerFacingMedium_CanExposeControllerVisibleState()
    {
        var medium = new FakeControllerFacingMedium();

        medium.WriteTrackRegister(0x20);
        medium.WriteSectorRegister(0x05);
        medium.WriteDataRegister(0xAA);

        Assert.Equal("d88-emulated", medium.MediumKind);
        Assert.True(medium.IsReady);
        Assert.False(medium.IsWriteProtected);
        Assert.Equal(0x20, medium.ReadTrackRegister());
        Assert.Equal(0x05, medium.ReadSectorRegister());
        Assert.Equal(0xAA, medium.ReadDataRegister());
        Assert.False(medium.IsIrqAsserted);
        Assert.False(medium.IsDrqAsserted);
    }

    private sealed class FakeSectorAddressableMedium : ISectorAddressableMedium
    {
        public string MediumKind => "d88";

        public bool SupportsDirectImageAccess => true;

        public bool SupportsControllerFacingAccess => true;

        public bool SectorExists(int cylinder, int head, int sector)
        {
            return cylinder == 0 && head == 0 && sector == 1;
        }

        public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted = false)
        {
            return SectorExists(cylinder, head, sector) ? new byte[] { 0x12, 0x34 } : [];
        }

        public IEnumerable<SectorInfo> GetAllSectors()
        {
            yield return new SectorInfo(0, 0, 1, 256, false, false);
        }
    }

    private sealed class FakeControllerFacingMedium : IControllerFacingMedium
    {
        private byte _track;
        private byte _sector;
        private byte _data;

        public string MediumKind => "d88-emulated";

        public bool IsReady => true;

        public bool IsWriteProtected => false;

        public bool IsIrqAsserted => false;

        public bool IsDrqAsserted => false;

        public void Reset()
        {
            _track = 0;
            _sector = 0;
            _data = 0;
        }

        public void SelectSide(int side)
        {
        }

        public void SeekTrack(int track)
        {
            _track = (byte)track;
        }

        public byte ReadStatus()
        {
            return 0;
        }

        public byte ReadTrackRegister()
        {
            return _track;
        }

        public byte ReadSectorRegister()
        {
            return _sector;
        }

        public byte PeekDataRegister()
        {
            return _data;
        }

        public byte ReadDataRegister()
        {
            return _data;
        }

        public void WriteCommand(byte value)
        {
        }

        public void WriteTrackRegister(byte value)
        {
            _track = value;
        }

        public void WriteSectorRegister(byte value)
        {
            _sector = value;
        }

        public void WriteDataRegister(byte value)
        {
            _data = value;
        }
    }
}
