using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Core;

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
}
