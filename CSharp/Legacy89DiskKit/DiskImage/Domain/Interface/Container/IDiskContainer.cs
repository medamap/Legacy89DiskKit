namespace Legacy89DiskKit.DiskImage.Domain.Interface.Container;

public interface IDiskContainer : IDisposable
{
    string FilePath { get; }
    bool IsReadOnly { get; }
    DiskType DiskType { get; }
    
    byte[] ReadSector(int cylinder, int head, int sector);
    byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted);
    void WriteSector(int cylinder, int head, int sector, byte[] data);
    
    bool SectorExists(int cylinder, int head, int sector);
    IEnumerable<SectorInfo> GetAllSectors();
    
    void Save();
    void SaveAs(string filePath);
}

public enum DiskType : byte
{
    TwoD = 0x00,
    TwoDD = 0x10, 
    TwoHD = 0x20
}

public record SectorInfo(int Cylinder, int Head, int Sector, int Size, bool IsDeleted, bool HasError);