namespace Legacy89DiskKit.Domain.DiskImage.Model;

public record SectorInfo(
    int Cylinder, 
    int Head, 
    int Sector, 
    int Size, 
    bool IsDeleted = false, 
    bool HasError = false
);
