namespace Legacy89DiskKit.NativeInterop.Core;

public static class NativeBridgeBackend
{
    private static readonly INativeBridgeBackend DefaultBackend = new ManagedNativeBridgeBackend();

    public static INativeBridgeBackend Current { get; private set; } = DefaultBackend;

    public static void SetCurrent(INativeBridgeBackend backend)
    {
        Current = backend;
    }

    public static void Reset()
    {
        Current = DefaultBackend;
    }
}
