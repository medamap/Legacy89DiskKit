namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeBoolean
{
    public static bool ToManagedBoolean(int value)
    {
        return value != 0;
    }

    public static int FromManagedBoolean(bool value)
    {
        return value ? 1 : 0;
    }
}
