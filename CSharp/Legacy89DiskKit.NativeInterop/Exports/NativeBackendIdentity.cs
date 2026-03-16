using Legacy89DiskKit.NativeInterop.Core;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeBackendIdentity
{
    public static string BackendKind => NativeBridgeBackend.Current.BackendKind;

    public static string BackendImplementation => NativeBridgeBackend.Current.BackendImplementation;

    public static string BackendTarget => NativeBridgeBackend.Current.BackendTarget;

    public static string GetBackendSummary()
    {
        return $"{BackendKind}:{BackendImplementation}->{BackendTarget}";
    }
}
