namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeBackendIdentity
{
    public const string BackendKind = "managed-bridge";
    public const string BackendImplementation = "Legacy89DiskKit.NativeInterop";
    public const string BackendTarget = "Legacy89DiskKit.Application";

    public static string GetBackendSummary()
    {
        return $"{BackendKind}:{BackendImplementation}->{BackendTarget}";
    }
}
