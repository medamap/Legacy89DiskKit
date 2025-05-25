using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Exception;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;

namespace Legacy89DiskKit.DiskImage.Application;

public class DiskImageService
{
    public IDiskContainer OpenDiskImage(string filePath, bool readOnly = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Disk image file not found: {filePath}");
            
        try
        {
            return new D88DiskContainer(filePath, readOnly);
        }
        catch (InvalidDiskFormatException)
        {
            throw; // そのまま再スロー
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open disk image: {filePath}", ex);
        }
    }

    public IDiskContainer CreateNewDiskImage(string filePath, DiskType diskType, string diskName = "")
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            
        if (!Enum.IsDefined(typeof(DiskType), diskType))
            throw new ArgumentException($"Invalid disk type: {diskType}", nameof(diskType));
            
        if (diskName != null && diskName.Length > 16)
            throw new ArgumentException("Disk name cannot be longer than 16 characters", nameof(diskName));
            
        try
        {
            var container = D88DiskContainer.CreateNew(filePath, diskType, diskName ?? "");
            container.Save();
            return container;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create disk image: {filePath}", ex);
        }
    }

    public void ConvertDiskImage(string sourcePath, string targetPath, DiskType targetType)
    {
        using var source = OpenDiskImage(sourcePath, true);
        using var target = CreateNewDiskImage(targetPath, targetType);
        
        foreach (var sectorInfo in source.GetAllSectors())
        {
            if (target.SectorExists(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector))
            {
                var data = source.ReadSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector);
                target.WriteSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector, data);
            }
        }
        
        target.Save();
    }

    public void CloneDiskImage(string sourcePath, string targetPath)
    {
        using var source = OpenDiskImage(sourcePath, true);
        source.SaveAs(targetPath);
    }
}