using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Core;

public static class NativeLibraryImports
{
    private const string LibName = "Legacy89DiskKitCpp";

    [DllImport(LibName, EntryPoint = "ldk_open_disk")]
    public static extern int OpenDisk([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int readOnlyFlag);

    [DllImport(LibName, EntryPoint = "ldk_open_disk_from_buffer")]
    public static extern int OpenDiskFromBuffer(byte[] data, int length, int readOnlyFlag);

    [DllImport(LibName, EntryPoint = "ldk_create_disk")]
    public static extern int CreateDisk([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int diskType, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(LibName, EntryPoint = "ldk_close_disk")]
    public static extern int CloseDisk(int handle);

    [DllImport(LibName, EntryPoint = "ldk_get_file_system_info")]
    public static extern int GetFileSystemInfo(int handle, out NativeFileSystemInfo info);

    [DllImport(LibName, EntryPoint = "ldk_get_container_metadata")]
    public static extern int GetContainerMetadata(int handle, out NativeDiskContainerMetadata metadata);

    [DllImport(LibName, EntryPoint = "ldk_get_files_count")]
    public static extern int GetFilesCount(int handle, out int count);

    [DllImport(LibName, EntryPoint = "ldk_get_files")]
    public static extern int GetFiles(int handle, [Out] NativeFileEntry[] buffer, int capacity);

    [DllImport(LibName, EntryPoint = "ldk_read_file")]
    public static extern int ReadFile(int handle, byte[] name, byte[] buffer, int capacity);

    [DllImport(LibName, EntryPoint = "ldk_read_sector")]
    public static extern int ReadSector(int handle, int cylinder, int head, int sector, byte[] buffer, int capacity);

    [DllImport(LibName, EntryPoint = "ldk_write_file")]
    public static extern int WriteFile(int handle, byte[] name, byte[] data, int length, ushort attributes, ushort loadAddress, ushort executionAddress);

    [DllImport(LibName, EntryPoint = "ldk_write_sector")]
    public static extern int WriteSector(int handle, int cylinder, int head, int sector, byte[] data, int length);

    [DllImport(LibName, EntryPoint = "ldk_delete_file")]
    public static extern int DeleteFile(int handle, byte[] name);

    [DllImport(LibName, EntryPoint = "ldk_rename_file")]
    public static extern int RenameFile(int handle, byte[] oldName, byte[] newName);

    [DllImport(LibName, EntryPoint = "ldk_update_attributes")]
    public static extern int UpdateAttributes(int handle, byte[] name, ushort attributes);

    [DllImport(LibName, EntryPoint = "ldk_read_boot_area")]
    public static extern int ReadBootArea(int handle, byte[] buffer, int capacity);

    [DllImport(LibName, EntryPoint = "ldk_write_boot_area")]
    public static extern int WriteBootArea(int handle, byte[] data, int length);

    [DllImport(LibName, EntryPoint = "ldk_format")]
    public static extern int Format(int handle);

    [DllImport(LibName, EntryPoint = "ldk_save")]
    public static extern int Save(int handle);

    [DllImport(LibName, EntryPoint = "ldk_read_directory_layout")]
    public static extern int ReadDirectoryLayout(int handle, [Out] NativeDirectoryLayoutItem[] buffer, int capacity);

    [DllImport(LibName, EntryPoint = "ldk_apply_directory_layout")]
    public static extern int ApplyDirectoryLayout(int handle, NativeDirectoryLayoutItem[] items, int count);
    
    [DllImport(LibName, EntryPoint = "ldk_get_backend_implementation")]
    public static extern int GetBackendImplementation(byte[] buffer, int capacity);
}
