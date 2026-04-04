using Legacy89DiskKit.DiskImage.Domain.Model;

namespace Legacy89DiskKit.DiskImage.Domain.Interface.Container;

public interface IDiskContainer : IDisposable
{
    string FilePath { get; }
    bool IsReadOnly { get; }
    DiskType DiskType { get; }
    DiskContainerMetadata GetMetadata();
    
    byte[] ReadSector(int cylinder, int head, int sector);
    byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted);
    void WriteSector(int cylinder, int head, int sector, byte[] data);
    
    bool SectorExists(int cylinder, int head, int sector);
    IEnumerable<SectorInfo> GetAllSectors();
    
    void Save();
    void SaveAs(string filePath);
}
