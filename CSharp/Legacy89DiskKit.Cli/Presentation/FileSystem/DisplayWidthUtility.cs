namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public static class DisplayWidthUtility
{
    public static int GetWidth(string value)
    {
        var width = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            width += GetRuneWidth(rune.Value);
        }

        return width;
    }

    public static string PadRight(string value, int totalWidth)
    {
        var padding = totalWidth - GetWidth(value);
        return padding > 0 ? value + new string(' ', padding) : value;
    }

    public static string PadLeft(string value, int totalWidth)
    {
        var padding = totalWidth - GetWidth(value);
        return padding > 0 ? new string(' ', padding) + value : value;
    }

    private static int GetRuneWidth(int value)
    {
        if (value is >= 0x1100 and <= 0x115F) return 2;
        if (value is >= 0x2329 and <= 0x232A) return 2;
        if (value is >= 0x2E80 and <= 0xA4CF) return 2;
        if (value is >= 0xAC00 and <= 0xD7A3) return 2;
        if (value is >= 0xF900 and <= 0xFAFF) return 2;
        if (value is >= 0xFE10 and <= 0xFE19) return 2;
        if (value is >= 0xFE30 and <= 0xFE6F) return 2;
        if (value is >= 0xFF00 and <= 0xFF60) return 2;
        if (value is >= 0xFFE0 and <= 0xFFE6) return 2;
        if (value is >= 0x1F300 and <= 0x1FAFF) return 2;
        if (value is >= 0x20000 and <= 0x3FFFD) return 2;
        return 1;
    }
}
