using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeInfoExports
{
    [UnmanagedCallersOnly(EntryPoint = "ldk_get_abi_version")]
    public static int GetAbiVersion()
    {
        return NativeSurfaceInfo.AbiVersion;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_capability_flags")]
    public static int GetCapabilityFlags()
    {
        return NativeSurfaceInfo.GetCapabilityFlags();
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_capability_summary")]
    public static int GetCapabilitySummary(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeSurfaceInfo.GetCapabilitySummary());
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_status_name")]
    public static int GetStatusName(int statusCode, IntPtr bufferPtr, int capacity)
    {
        var statusName = statusCode switch
        {
            (int)LdkStatus.Success => "success",
            (int)LdkStatus.ErrorGeneric => "error-generic",
            (int)LdkStatus.ErrorInvalidHandle => "error-invalid-handle",
            (int)LdkStatus.ErrorInvalidArgument => "error-invalid-argument",
            (int)LdkStatus.ErrorFileNotFound => "error-file-not-found",
            (int)LdkStatus.ErrorDiskFull => "error-disk-full",
            (int)LdkStatus.ErrorReadOnly => "error-read-only",
            (int)LdkStatus.ErrorNotImplemented => "error-not-implemented",
            (int)LdkStatus.ErrorBufferTooSmall => "error-buffer-too-small",
            _ => "unknown-status"
        };

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, statusName);
    }
}
