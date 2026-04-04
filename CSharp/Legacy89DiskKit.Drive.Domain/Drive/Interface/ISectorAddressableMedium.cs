using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Domain.Drive.Interface;

public interface ISectorAddressableMedium : IMountedMedium
{
    bool SectorExists(int cylinder, int head, int sector);

    byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted = false);

    IEnumerable<SectorInfo> GetAllSectors();
}
