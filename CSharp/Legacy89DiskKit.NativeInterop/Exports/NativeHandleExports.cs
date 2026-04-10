using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeHandleExports
{
    [UnmanagedCallersOnly(EntryPoint = "ldk_is_handle_valid")]
    public static int IsHandleValid(int handle)
    {
        return HandleManager.IsRegistered(handle) ? 1 : 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_open_handle_count")]
    public static int GetOpenHandleCount()
    {
        return HandleManager.GetOpenHandleCount();
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_handle_source_operation")]
    public static int GetHandleSourceOperation(int handle, IntPtr bufferPtr, int capacity)
    {
        if (!HandleManager.TryGetMetadata(handle, out var metadata))
        {
            return (int)LdkStatus.ErrorInvalidHandle;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, metadata.SourceOperation);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_handle_is_writable")]
    public static int GetHandleIsWritable(int handle)
    {
        if (!HandleManager.TryGetMetadata(handle, out var metadata))
        {
            return (int)LdkStatus.ErrorInvalidHandle;
        }

        return NativeBoolean.FromManagedBoolean(metadata.IsWritable);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_handle_summary")]
    public static int GetHandleSummary(int handle, IntPtr bufferPtr, int capacity)
    {
        if (!HandleManager.TryGetMetadata(handle, out var metadata))
        {
            return (int)LdkStatus.ErrorInvalidHandle;
        }

        var summary = $"{metadata.SourceOperation}:{(metadata.IsWritable ? "writable" : "read-only")}";
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, summary);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_close_all_handles")]
    public static int CloseAllHandles()
    {
        HandleManager.Clear();
        return 0;
    }
}
