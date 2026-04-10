using System.Runtime.InteropServices;
using System.Linq;
using Legacy89DiskKit.NativeInterop.Types;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.DiskImage.Application;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class DiskExports
{
    [UnmanagedCallersOnly(EntryPoint = "ldk_open_disk")]
    public static int OpenDisk(IntPtr pathPtr, int readOnlyFlag)
    {
        try
        {
            string? path = Marshal.PtrToStringUTF8(pathPtr);
            if (string.IsNullOrEmpty(path)) return (int)LdkStatus.ErrorInvalidArgument;

            var readOnly = NativeBoolean.ToManagedBoolean(readOnlyFlag);
            var isWritable = !readOnly;
            var session = NativeBridgeBackend.Current.OpenDisk(path, readOnly);
            return HandleManager.Register(session, new HandleMetadata("open-disk", isWritable));
        }
        catch (Exception ex)
        {
            return (int)NativeStatusMapper.FromException(ex);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_create_disk")]
    public static int CreateDisk(IntPtr pathPtr, int diskType, IntPtr namePtr)
    {
        try
        {
            string? path = Marshal.PtrToStringUTF8(pathPtr);
            string? name = Marshal.PtrToStringUTF8(namePtr) ?? "";
            if (string.IsNullOrEmpty(path)) return (int)LdkStatus.ErrorInvalidArgument;

            var session = NativeBridgeBackend.Current.CreateDisk(
                path,
                (Legacy89DiskKit.Domain.DiskImage.Model.DiskType)diskType,
                name);
            return HandleManager.Register(session, new HandleMetadata("create-disk", true));
        }
        catch (Exception ex)
        {
            return (int)NativeStatusMapper.FromException(ex);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_close_disk")]
    public static int CloseDisk(int handle)
    {
        if (HandleManager.Unregister(handle))
        {
            return (int)LdkStatus.Success;
        }
        return (int)LdkStatus.ErrorInvalidHandle;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_file_system_info")]
    public static int GetFileSystemInfo(int handle, IntPtr infoPtr)
    {
        if (!HandleManager.TryGet(handle, out var session) || session == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        var fs = session.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        var info = fs.GetFileSystemInfo();
        var nativeInfo = new NativeFileSystemInfo
        {
            FileSystemName = info.FileSystemName,
            TotalCapacity = info.TotalCapacity,
            FreeSpace = info.FreeSpace,
            ClusterSize = info.ClusterSize,
            ReservedSectors = info.ReservedSectors,
            PlatformId = info.PlatformId
        };

        Marshal.StructureToPtr(nativeInfo, infoPtr, false);
        return (int)LdkStatus.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_container_metadata")]
    public static int GetContainerMetadata(int handle, IntPtr metadataPtr)
    {
        if (!HandleManager.TryGet(handle, out var session) || session == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        var metadata = session.GetContainerMetadata();
        if (metadata == null) return (int)LdkStatus.ErrorFileNotFound;

        var nativeMetadata = NativeDiskContainerMetadataFactory.Create(metadata);
        Marshal.StructureToPtr(nativeMetadata, metadataPtr, false);
        return (int)LdkStatus.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_files_count")]
    public static int GetFilesCount(int handle, IntPtr outCountPtr)
    {
        if (!HandleManager.TryGet(handle, out var session) || session == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        var fs = session.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        var files = fs.GetFiles();
        int count = 0;
        foreach (var _ in files) count++;

        Marshal.WriteInt32(outCountPtr, count);
        return (int)LdkStatus.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_files")]
    public static int GetFiles(int handle, IntPtr bufferPtr, int capacity)
    {
        if (!HandleManager.TryGet(handle, out var session) || session == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        var fs = session.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        var files = fs.GetFiles().ToList();
        int count = Math.Min(files.Count, capacity);

        int structSize = Marshal.SizeOf<NativeFileEntry>();
        for (int i = 0; i < count; i++)
        {
            var file = files[i];
            var nativeFile = new NativeFileEntry
            {
                FileName = CreatePaddedBytes(file.RawFileName, file.FileName, 16),
                Extension = CreatePaddedBytes(file.RawExtension, file.Extension, 8),
                Size = (int)file.Size,
                LoadAddress = file.LoadAddress ?? 0,
                ExecutionAddress = file.ExecutionAddress ?? 0,
                Attributes = (ushort)file.Attributes.StandardAttributes
            };
            
            IntPtr currentPtr = bufferPtr + (i * structSize);
            Marshal.StructureToPtr(nativeFile, currentPtr, false);
        }

        return count;
    }

    private static byte[] CreatePaddedBytes(byte[]? rawBytes, string fallbackString, int length)
    {
        var result = new byte[length];
        if (rawBytes != null && rawBytes.Length > 0)
        {
            Array.Copy(rawBytes, result, Math.Min(rawBytes.Length, length));
        }
        else
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(fallbackString);
            Array.Copy(bytes, result, Math.Min(bytes.Length, length));
        }
        return result;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_read_boot_area")]
    public static int ReadBootArea(int handle, IntPtr bufferPtr, int capacity)
    {
        if (!HandleManager.TryGet(handle, out var session) || session == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        var fs = session.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            byte[] data = fs.ReadBootArea();
            int size = Math.Min(data.Length, capacity);
            Marshal.Copy(data, 0, bufferPtr, size);
            return size;
        }
        catch (Exception ex)
        {
            return (int)NativeStatusMapper.FromException(ex);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_write_boot_area")]
    public static int WriteBootArea(int handle, IntPtr dataPtr, int length)
    {
        if (!HandleManager.TryGet(handle, out var session) || session == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        var fs = session.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            byte[] data = new byte[length];
            Marshal.Copy(dataPtr, data, 0, length);
            fs.WriteBootArea(data);
            return (int)LdkStatus.Success;
        }
        catch (Exception ex)
        {
            return (int)NativeStatusMapper.FromException(ex);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_format")]
    public static int Format(int handle)
    {
        if (!HandleManager.TryGet(handle, out var session) || session == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        var fs = session.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            fs.Format();
            return (int)LdkStatus.Success;
        }
        catch (Exception ex)
        {
            return (int)NativeStatusMapper.FromException(ex);
        }
    }
}
