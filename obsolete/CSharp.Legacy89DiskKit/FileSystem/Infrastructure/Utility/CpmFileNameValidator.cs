namespace Legacy89DiskKit.FileSystem.Infrastructure.Utility;

/// <summary>
/// Validates CP/M file names (8.3 format)
/// </summary>
public static class CpmFileNameValidator
{
    private const int MaxFileNameLength = 8;
    private const int MaxExtensionLength = 3;
    
    /// <summary>
    /// Characters that are not allowed in CP/M filenames
    /// </summary>
    private static readonly char[] InvalidChars = { '<', '>', '.', ',', ';', ':', '=', '?', '*', '[', ']', '%', '|', '(', ')', '/', '\\' };

    /// <summary>
    /// Validates a CP/M filename
    /// </summary>
    public static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        // Split into name and extension
        var parts = fileName.Split('.');
        if (parts.Length > 2)
            return false;

        var name = parts[0];
        var extension = parts.Length > 1 ? parts[1] : string.Empty;

        // Validate name part
        if (name.Length == 0 || name.Length > MaxFileNameLength)
            return false;

        // Validate extension part
        if (extension.Length > MaxExtensionLength)
            return false;

        // Check for invalid characters
        var fullName = name + extension;
        foreach (var ch in fullName)
        {
            if (InvalidChars.Contains(ch) || ch < 0x20 || ch > 0x7E)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Normalizes a filename to CP/M format (uppercase, 8.3 format)
    /// </summary>
    public static (string name, string extension) NormalizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return (string.Empty, string.Empty);

        // Convert to uppercase
        fileName = fileName.ToUpperInvariant();

        // Split into name and extension
        var lastDot = fileName.LastIndexOf('.');
        string name, extension;

        if (lastDot < 0)
        {
            name = fileName;
            extension = string.Empty;
        }
        else
        {
            name = fileName.Substring(0, lastDot);
            extension = fileName.Substring(lastDot + 1);
        }

        // Truncate to valid lengths
        if (name.Length > MaxFileNameLength)
            name = name.Substring(0, MaxFileNameLength);

        if (extension.Length > MaxExtensionLength)
            extension = extension.Substring(0, MaxExtensionLength);

        // Replace invalid characters with underscore
        name = ReplaceInvalidChars(name);
        extension = ReplaceInvalidChars(extension);

        return (name, extension);
    }

    /// <summary>
    /// Formats a filename for CP/M directory entry (space padded)
    /// </summary>
    public static (string name, string extension) FormatForDirectory(string fileName)
    {
        var (name, extension) = NormalizeFileName(fileName);

        // Pad with spaces to fixed length
        name = name.PadRight(MaxFileNameLength, ' ');
        extension = extension.PadRight(MaxExtensionLength, ' ');

        return (name, extension);
    }

    /// <summary>
    /// Converts a CP/M wildcard pattern to regex pattern
    /// </summary>
    public static string WildcardToRegex(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return ".*";

        // Handle the special case of "*.*" (all files)
        if (pattern == "*.*")
            return ".*";

        // Normalize the pattern but preserve wildcards
        pattern = pattern.ToUpperInvariant();
        
        // Split into name and extension parts
        var lastDot = pattern.LastIndexOf('.');
        string namePattern, extPattern;
        
        if (lastDot < 0)
        {
            namePattern = pattern;
            extPattern = "";
        }
        else
        {
            namePattern = pattern.Substring(0, lastDot);
            extPattern = pattern.Substring(lastDot + 1);
        }
        
        // Convert CP/M wildcards to regex
        // ? = any single character
        // * = any sequence of characters
        namePattern = System.Text.RegularExpressions.Regex.Escape(namePattern).Replace("\\?", ".").Replace("\\*", ".*");
        extPattern = System.Text.RegularExpressions.Regex.Escape(extPattern).Replace("\\?", ".").Replace("\\*", ".*");

        // Build complete pattern
        if (string.IsNullOrEmpty(extPattern))
        {
            // Pattern without extension - match files with or without extension
            return $"^{namePattern}(\\..{{0,3}})?$";
        }
        else
        {
            // Pattern with extension
            return $"^{namePattern}\\.{extPattern}$";
        }
    }

    private static string ReplaceInvalidChars(string text)
    {
        var result = text.ToCharArray();
        for (int i = 0; i < result.Length; i++)
        {
            if (InvalidChars.Contains(result[i]) || result[i] < 0x20 || result[i] > 0x7E)
            {
                result[i] = '_';
            }
        }
        return new string(result);
    }
}