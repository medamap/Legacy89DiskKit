namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeOpenModeCatalog
{
    private static readonly string[] Modes =
    [
        "open-disk:read-only",
        "open-disk:writable",
        "create-disk:writable",
    ];

    public static IReadOnlyList<string> GetModes() => Modes;

    public static string GetOpenModeSummary()
    {
        return "open-disk supports read-only and writable modes; create-disk always returns writable handles";
    }
}
