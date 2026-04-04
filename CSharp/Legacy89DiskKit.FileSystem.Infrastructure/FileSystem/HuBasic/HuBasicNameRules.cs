namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;

public static class HuBasicNameRules
{
    public static (string FileName, string Extension) ParseFileName(string fileName)
    {
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));

        var parts = fileName.Split('.');
        string name = parts[0];
        if (name.Length > 13)
        {
            name = name[..13];
        }

        string extension = parts.Length > 1 ? parts[1] : string.Empty;
        if (extension.Length > 3)
        {
            extension = extension[..3];
        }

        return (name, extension);
    }

    public static string BuildDisplayName(string fileName, string extension)
    {
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
        if (extension == null) throw new ArgumentNullException(nameof(extension));

        return string.IsNullOrEmpty(extension) ? fileName : $"{fileName}.{extension}";
    }
}
