namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeHandleContract
{
    public const int InvalidHandleValue = 0;

    public static string GetHandleValueSummary()
    {
        return "successful open/create operations return positive handles; zero means invalid handle placeholder; negative values are status errors";
    }
}
