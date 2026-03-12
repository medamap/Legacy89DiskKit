namespace Legacy89DiskKit.Domain.FileSystem.Model;

public record DiskFileSystemInfo(
    string FileSystemName,
    long TotalCapacity,
    long FreeSpace,
    int ClusterSize,
    int ReservedSectors,
    string PlatformId = "",
    string DefaultEncodingId = ""
);

public record BootSector(
    byte[] RawData,
    string MachineName = "",
    string Signature = ""
);
