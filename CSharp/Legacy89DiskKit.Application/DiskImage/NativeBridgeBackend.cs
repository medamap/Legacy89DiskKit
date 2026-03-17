using Legacy89DiskKit.Domain.Native;

namespace Legacy89DiskKit.Application.DiskImage;

public static class NativeBridgeBackend
{
    private static INativeBridgeBackend? _current;

    public static INativeBridgeBackend Current
    {
        get => _current ?? throw new InvalidOperationException("Native bridge backend not initialized. Please call SetCurrent before using DiskService.");
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
