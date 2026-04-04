namespace Legacy89DiskKit.FileSystem.Domain.Model;

public record DiskFileSystemInfo(
    string FileSystemName,
    long TotalCapacity,
    long FreeSpace,
    int ClusterSize,
    int ReservedSectors,
    string PlatformId = "",
    string DefaultEncodingId = "",
    int MaxBaseNameLength = 8,
    int MaxExtensionLength = 3
);

public record BootSector(
    byte[] RawData,
    string MachineName = "",
    string Signature = ""
);
