using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public sealed class DiskInspectionService
{
    public InspectionReport BuildReport(DiskContainerMetadata metadata, IFileSystem? fileSystem, BootInfoSummary? bootSummary, string detailLevel, string encodingId)
    {
        var normalizedDetail = detailLevel.Trim().ToLowerInvariant();
        var items = new List<InspectionItem>
        {
            new("Disk", "Container", metadata.ImageFormat),
            new("Disk", "Disk Type", FormatDiskType(metadata.DiskType))
        };
        if (fileSystem == null)
        {
            items.Add(new("Disk", "Machine Profile", "unknown"));
            items.Add(new("Disk", "File System", "unknown"));
            if (normalizedDetail is "normal" or "full")
            {
                items.Add(new("Disk", "Geometry", FormatGeometry(metadata.Geometry)));
                items.Add(new("Disk", "Image Size", metadata.DeclaredImageSize.ToString()));
                items.Add(new("Disk", "Write Protected", metadata.IsWriteProtected.ToString()));
            }

            return new InspectionReport("Disk Inspector", items);
        }

        var fsInfo = fileSystem.GetFileSystemInfo();
        var machineProfile = ResolveMachineProfile(fsInfo, bootSummary);
        items.Add(new("Disk", "Machine Profile", machineProfile));
        items.Add(new("Disk", "File System", fsInfo.FileSystemName));
        items.Add(new("Disk", "Files", fileSystem.GetFiles().Count().ToString()));
        items.Add(new("Disk", "Boot", FormatBootMode(bootSummary?.Mode ?? BootInfoMode.None)));
        if (normalizedDetail is "normal" or "full")
        {
            items.Add(new("Disk", "Total", fsInfo.TotalCapacity.ToString()));
            items.Add(new("Disk", "Used", (fsInfo.TotalCapacity - fsInfo.FreeSpace).ToString()));
            items.Add(new("Disk", "Free", fsInfo.FreeSpace.ToString()));
            items.Add(new("Disk", "Encoding", encodingId));
        }

        if (normalizedDetail == "full")
        {
            items.Add(new("Disk", "Geometry", FormatGeometry(metadata.Geometry)));
            items.Add(new("Disk", "Image Size", metadata.DeclaredImageSize.ToString()));
            items.Add(new("Disk", "Write Protected", metadata.IsWriteProtected.ToString()));
            if (!string.IsNullOrWhiteSpace(bootSummary?.FileName))
            {
                items.Add(new("Boot", "Boot File", bootSummary!.FileName!));
            }

            if (bootSummary?.LoadAddress is { } load)
            {
                items.Add(new("Boot", "Boot Load", load.ToString("X4")));
            }

            if (bootSummary?.ExecutionAddress is { } exec)
            {
                items.Add(new("Boot", "Boot Exec", exec.ToString("X4")));
            }
        }

        return new InspectionReport("Disk Inspector", items);
    }

    private static string ResolveMachineProfile(DiskFileSystemInfo fsInfo, BootInfoSummary? bootSummary)
    {
        if (bootSummary != null && bootSummary.MachineFamily != CharacterEncoding.Domain.Model.MachineType.Unknown)
        {
            return bootSummary.MachineFamily.ToString();
        }

        return string.IsNullOrWhiteSpace(fsInfo.PlatformId) ? "unknown" : fsInfo.PlatformId;
    }

    private static string FormatDiskType(DiskType diskType) => diskType switch
    {
        DiskType.TwoD => "2D",
        DiskType.TwoDD => "2DD",
        DiskType.TwoHD => "2HD",
        DiskType.HardDisk => "HardDisk",
        _ => diskType.ToString()};
    private static string FormatGeometry(DiskGeometryInfo geometry) => $"{geometry.Cylinders}c x {geometry.Heads}h x {geometry.SectorsPerTrack}spt x {geometry.BytesPerSector}bps";
    private static string FormatBootMode(BootInfoMode mode) => mode switch
    {
        BootInfoMode.FileBacked => "file-backed",
        BootInfoMode.SectorResident => "sector-resident",
        _ => "none"
    };
}
