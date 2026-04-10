namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeSurfaceCatalog
{
    private static readonly string[] SupportedFileSystems =
    [
        "hu-basic",
        "n88-basic",
        "msx-dos"
    ];

    private static readonly string[] SupportedPlatforms =
    [
        "X1",
        "PC88",
        "MSX"
    ];

    private static readonly string[] SupportedImageFormats =
    [
        "d88",
        "d77",
        "2d",
        "dsk"
    ];

    public static IReadOnlyList<string> GetSupportedFileSystems() => SupportedFileSystems;
    public static IReadOnlyList<string> GetSupportedPlatforms() => SupportedPlatforms;
    public static IReadOnlyList<string> GetSupportedImageFormats() => SupportedImageFormats;
}
