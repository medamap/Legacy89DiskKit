using Legacy89DiskKit.DiskImage.Domain.Model;

namespace Legacy89DiskKit.Drive.Domain.Interface;

public interface ISectorAddressableMedium : IMountedMedium
{
    bool SectorExists(int cylinder, int head, int sector);

    byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted = false);

    IEnumerable<SectorInfo> GetAllSectors();
}
