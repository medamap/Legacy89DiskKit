using System.Runtime.InteropServices;
using System.Linq;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Application.CharacterEncoding;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.NativeInterop.Types;
using Legacy89DiskKit.NativeInterop.Core;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class DiskExports
{
    private static IFileSystemRegistry? _defaultRegistry;
    private static IEncoderRegistry? _defaultEncoderRegistry;

    private static IEncoderRegistry GetDefaultEncoderRegistry()
    {
        if (_defaultEncoderRegistry == null)
        {
            var registry = new EncoderRegistry();
            registry.Register("X1", new X1CharacterEncoder());
            registry.Register("PC88", new Pc8801CharacterEncoder());
            registry.Register("MSX", new Msx1CharacterEncoder());
            _defaultEncoderRegistry = registry;
        }
        return _defaultEncoderRegistry;
    }

    private static IFileSystemRegistry GetDefaultRegistry()
    {
        if (_defaultRegistry == null)
        {
            var registry = new FileSystemRegistry();
            registry.Register(new HuBasicFileSystemProvider());
            registry.Register(new N88BasicFileSystemProvider());
            registry.Register(new MsxDosFileSystemProvider());
            // Add other providers here as they are implemented
            _defaultRegistry = registry;
        }
        return _defaultRegistry;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_open_disk")]
    public static int OpenDisk(IntPtr pathPtr, int readOnlyFlag)
    {
        try
        {
            string? path = Marshal.PtrToStringUTF8(pathPtr);
            if (string.IsNullOrEmpty(path)) return (int)LdkStatus.ErrorInvalidArgument;

            var readOnly = NativeBoolean.ToManagedBoolean(readOnlyFlag);
            var isWritable = !readOnly;
            var session = NativeSessionFactory.OpenDisk(path, readOnly, GetDefaultRegistry());
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

            var session = NativeSessionFactory.CreateDisk(
                path,
                (Legacy89DiskKit.Domain.DiskImage.Model.DiskType)diskType,
                name,
                GetDefaultRegistry());
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
                FileName = file.FileName,
                Extension = file.Extension,
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
