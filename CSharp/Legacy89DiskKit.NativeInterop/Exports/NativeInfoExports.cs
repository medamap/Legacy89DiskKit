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
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeStatusCatalog.GetName((LdkStatus)statusCode));
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_status_count")]
    public static int GetStatusCount()
    {
        return NativeStatusCatalog.GetEntries().Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_status_code_at")]
    public static int GetStatusCodeAt(int index)
    {
        var entries = NativeStatusCatalog.GetEntries();
        if (index < 0 || index >= entries.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return (int)entries[index].Status;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_status_name_at")]
    public static int GetStatusNameAt(int index, IntPtr bufferPtr, int capacity)
    {
        var entries = NativeStatusCatalog.GetEntries();
        if (index < 0 || index >= entries.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, entries[index].Name);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_supported_file_system_count")]
    public static int GetSupportedFileSystemCount()
    {
        return NativeSurfaceCatalog.GetSupportedFileSystems().Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_supported_file_system_name")]
    public static int GetSupportedFileSystemName(int index, IntPtr bufferPtr, int capacity)
    {
        return WriteCatalogItem(NativeSurfaceCatalog.GetSupportedFileSystems(), index, bufferPtr, capacity);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_supported_platform_count")]
    public static int GetSupportedPlatformCount()
    {
        return NativeSurfaceCatalog.GetSupportedPlatforms().Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_supported_platform_name")]
    public static int GetSupportedPlatformName(int index, IntPtr bufferPtr, int capacity)
    {
        return WriteCatalogItem(NativeSurfaceCatalog.GetSupportedPlatforms(), index, bufferPtr, capacity);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_supported_image_format_count")]
    public static int GetSupportedImageFormatCount()
    {
        return NativeSurfaceCatalog.GetSupportedImageFormats().Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_supported_image_format_name")]
    public static int GetSupportedImageFormatName(int index, IntPtr bufferPtr, int capacity)
    {
        return WriteCatalogItem(NativeSurfaceCatalog.GetSupportedImageFormats(), index, bufferPtr, capacity);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_handle_lifecycle_summary")]
    public static int GetHandleLifecycleSummary(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeOwnershipPolicy.GetHandleLifecycleSummary());
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_invalid_handle_value")]
    public static int GetInvalidHandleValue()
    {
        return NativeHandleContract.InvalidHandleValue;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_handle_value_summary")]
    public static int GetHandleValueSummary(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeHandleContract.GetHandleValueSummary());
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_buffer_string_policy_summary")]
    public static int GetBufferStringPolicySummary(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeOwnershipPolicy.GetBufferStringPolicySummary());
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_mutation_policy_summary")]
    public static int GetMutationPolicySummary(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeMutationPolicy.GetMutationPolicySummary());
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_backend_kind")]
    public static int GetBackendKind(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeBackendIdentity.BackendKind);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_backend_implementation")]
    public static int GetBackendImplementation(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeBackendIdentity.BackendImplementation);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_backend_target")]
    public static int GetBackendTarget(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeBackendIdentity.BackendTarget);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_backend_summary")]
    public static int GetBackendSummary(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeBackendIdentity.GetBackendSummary());
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_export_count")]
    public static int GetExportCount()
    {
        return NativeExportCatalog.GetEntries().Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_export_name_at")]
    public static int GetExportNameAt(int index, IntPtr bufferPtr, int capacity)
    {
        var entries = NativeExportCatalog.GetEntries();
        if (index < 0 || index >= entries.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, entries[index].Name);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_export_group_at")]
    public static int GetExportGroupAt(int index, IntPtr bufferPtr, int capacity)
    {
        var entries = NativeExportCatalog.GetEntries();
        if (index < 0 || index >= entries.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, entries[index].Group);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_mutating_operation_count")]
    public static int GetMutatingOperationCount()
    {
        return NativeMutationPolicy.GetMutatingOperations().Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_mutating_operation_name_at")]
    public static int GetMutatingOperationNameAt(int index, IntPtr bufferPtr, int capacity)
    {
        var operations = NativeMutationPolicy.GetMutatingOperations();
        if (index < 0 || index >= operations.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, operations[index]);
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_open_mode_summary")]
    public static int GetOpenModeSummary(IntPtr bufferPtr, int capacity)
    {
        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, NativeOpenModeCatalog.GetOpenModeSummary());
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_open_mode_count")]
    public static int GetOpenModeCount()
    {
        return NativeOpenModeCatalog.GetModes().Count;
    }

    [UnmanagedCallersOnly(EntryPoint = "ldk_get_open_mode_name_at")]
    public static int GetOpenModeNameAt(int index, IntPtr bufferPtr, int capacity)
    {
        var modes = NativeOpenModeCatalog.GetModes();
        if (index < 0 || index >= modes.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, modes[index]);
    }

    private static int WriteCatalogItem(IReadOnlyList<string> items, int index, IntPtr bufferPtr, int capacity)
    {
        if (index < 0 || index >= items.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, items[index]);
    }
}
