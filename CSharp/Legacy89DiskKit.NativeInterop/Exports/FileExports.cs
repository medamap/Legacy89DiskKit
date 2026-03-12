using System.Runtime.InteropServices;
using System.IO;
using Legacy89DiskKit.NativeInterop.Types;
using Legacy89DiskKit.NativeInterop.Core;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class FileExports
{
    [UnmanagedCallersOnly(EntryPoint = "ldk_read_file")]
    public static int ReadFile(int handle, IntPtr namePtr, IntPtr bufferPtr, int capacity)
    {
        if (!HandleManager.TryGet(handle, out var service) || service == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        string? fileName = Marshal.PtrToStringUTF8(namePtr);
        if (string.IsNullOrEmpty(fileName)) return (int)LdkStatus.ErrorInvalidArgument;

        var fs = service.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            byte[] data = fs.ReadFile(fileName);
            int size = Math.Min(data.Length, capacity);
            Marshal.Copy(data, 0, bufferPtr, size);
            return size;
        }
        catch (FileNotFoundException)
        {
            return (int)LdkStatus.ErrorFileNotFound;
        }
        catch (Exception)
        {
            return (int)LdkStatus.ErrorGeneric;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_delete_file")]
    public static int DeleteFile(int handle, IntPtr namePtr)
    {
        if (!HandleManager.TryGet(handle, out var service) || service == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        string? fileName = Marshal.PtrToStringUTF8(namePtr);
        if (string.IsNullOrEmpty(fileName)) return (int)LdkStatus.ErrorInvalidArgument;

        var fs = service.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            fs.DeleteFile(fileName);
            return (int)LdkStatus.Success;
        }
        catch (Exception)
        {
            return (int)LdkStatus.ErrorGeneric;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_write_file")]
    public static int WriteFile(int handle, IntPtr namePtr, IntPtr dataPtr, int length, ushort attributes)
    {
        if (!HandleManager.TryGet(handle, out var service) || service == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        string? fileName = Marshal.PtrToStringUTF8(namePtr);
        if (string.IsNullOrEmpty(fileName)) return (int)LdkStatus.ErrorInvalidArgument;

        var fs = service.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            byte[] data = new byte[length];
            Marshal.Copy(dataPtr, data, 0, length);
            
            fs.WriteFile(fileName, data, new Legacy89DiskKit.Domain.FileSystem.Model.ExtendedFileAttributes((Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes)attributes, 0, false));
            return (int)LdkStatus.Success;
        }
        catch (Exception)
        {
            return (int)LdkStatus.ErrorGeneric;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_rename_file")]
    public static int RenameFile(int handle, IntPtr oldNamePtr, IntPtr newNamePtr)
    {
        if (!HandleManager.TryGet(handle, out var service) || service == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        string? oldName = Marshal.PtrToStringUTF8(oldNamePtr);
        string? newName = Marshal.PtrToStringUTF8(newNamePtr);
        if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName)) return (int)LdkStatus.ErrorInvalidArgument;

        var fs = service.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            fs.RenameFile(oldName, newName);
            return (int)LdkStatus.Success;
        }
        catch (Exception)
        {
            return (int)LdkStatus.ErrorGeneric;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_update_attributes")]
    public static int UpdateAttributes(int handle, IntPtr namePtr, ushort attributes)
    {
        if (!HandleManager.TryGet(handle, out var service) || service == null)
            return (int)LdkStatus.ErrorInvalidHandle;

        string? fileName = Marshal.PtrToStringUTF8(namePtr);
        if (string.IsNullOrEmpty(fileName)) return (int)LdkStatus.ErrorInvalidArgument;

        var fs = service.FileSystem;
        if (fs == null) return (int)LdkStatus.ErrorFileNotFound;

        try
        {
            fs.UpdateAttributes(fileName, new Legacy89DiskKit.Domain.FileSystem.Model.ExtendedFileAttributes((Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes)attributes, 0, false));
            return (int)LdkStatus.Success;
        }
        catch (Exception)
        {
            return (int)LdkStatus.ErrorGeneric;
        }
    }
}
