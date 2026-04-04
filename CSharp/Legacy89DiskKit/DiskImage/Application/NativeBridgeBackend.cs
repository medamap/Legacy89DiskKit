using Legacy89DiskKit.Native.Application;
using Legacy89DiskKit.Native.Domain;

namespace Legacy89DiskKit.DiskImage.Application;

public static class NativeBridgeBackend
{
    private static INativeBridgeBackend? _current;

    public static INativeBridgeBackend Current
    {
        get => _current ?? new ManagedNativeBridgeBackend();
        private set => _current = value;
    }

    public static void SetCurrent(INativeBridgeBackend backend)
    {
        _current = backend;
    }

    public static void Reset()
    {
        _current = null;
    }
}
