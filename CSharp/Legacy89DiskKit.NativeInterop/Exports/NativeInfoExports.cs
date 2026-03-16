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

    private static int WriteCatalogItem(IReadOnlyList<string> items, int index, IntPtr bufferPtr, int capacity)
    {
        if (index < 0 || index >= items.Count)
        {
            return (int)LdkStatus.ErrorInvalidArgument;
        }

        return NativeStringWriter.WriteUtf8(bufferPtr, capacity, items[index]);
    }
}
