using System.Runtime.InteropServices;

namespace Legacy89DiskKit.NativeInterop.Types;

/// <summary>
/// Status codes for Native API.
/// </summary>
public enum LdkStatus : int
{
    Success = 0,
    ErrorGeneric = -1,
    ErrorInvalidHandle = -2,
    ErrorInvalidArgument = -3,
    ErrorFileNotFound = -4,
    ErrorDiskFull = -5,
    ErrorReadOnly = -6,
    ErrorNotImplemented = -7,
    ErrorBufferTooSmall = -8
}

/// <summary>
/// C-compatible file entry structure.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct NativeFileEntry
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] FileName;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Extension;
    
    public int Size;
    public ushort LoadAddress;
    public ushort ExecutionAddress;
    public ushort Attributes; // Domain.FileSystem.Model.FileAttributes
}

/// <summary>
/// C-compatible file system information structure.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct NativeFileSystemInfo
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string FileSystemName;
    
    public long TotalCapacity;
    public long FreeSpace;
    public int ClusterSize;
    public int ReservedSectors;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string PlatformId;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct NativeDiskContainerMetadata
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string ImageFormat;

    public int DiskType;
    public int Cylinders;
    public int Heads;
    public int SectorsPerTrack;
    public int BytesPerSector;
    public int IsWriteProtected;
    public long DeclaredImageSize;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NativeDirectoryLayoutItem
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] Id;
    public int Order;
    public int Kind;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] DisplayName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] StableId;
}

/// <summary>
/// Disk types for creation.
/// </summary>
public enum LdkDiskType : int
{
    TwoD = 0,   // 5.25" 2D (320KB/360KB)
    TwoDD = 1,  // 3.5" 2DD (720KB)
    TwoHD = 2,  // 3.5" 2HD (1.2MB/1.44MB)
    HardDisk = 3
}
