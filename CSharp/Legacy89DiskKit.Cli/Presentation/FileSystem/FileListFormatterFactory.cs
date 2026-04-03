namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public static class FileListFormatterFactory
{
    public static IFileListFormatter Create(string fileSystemName)
    {
        return fileSystemName switch
        {
            "Hu-BASIC" => new HuBasicFileListFormatter(),
            "X-DOS" => new XDosFileListFormatter(),
            _ => new DefaultFileListFormatter()
        };
    }
}
