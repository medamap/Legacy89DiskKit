namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeSurfaceInfo
{
    public const int AbiVersion = 1;
    public const int CapabilityPathOpen = 1 << 0;
    public const int CapabilityCreateDisk = 1 << 1;
    public const int CapabilityFileRead = 1 << 2;
    public const int CapabilityFileWrite = 1 << 3;
    public const int CapabilityBootArea = 1 << 4;
    public const int CapabilityFormat = 1 << 5;
    public const int CapabilityManagedBridge = 1 << 6;

    public static int GetCapabilityFlags()
    {
        return CapabilityPathOpen
            | CapabilityCreateDisk
            | CapabilityFileRead
            | CapabilityFileWrite
            | CapabilityBootArea
            | CapabilityFormat
            | CapabilityManagedBridge;
    }

    public static string GetCapabilitySummary()
    {
        return "path-open,create-disk,file-read,file-write,boot-area,format,managed-bridge";
    }
}
