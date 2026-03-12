namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public interface IFileListFormatter
{
    FileListView Format(FileListFormatContext context, IFileListLocalizer localizer);
}
