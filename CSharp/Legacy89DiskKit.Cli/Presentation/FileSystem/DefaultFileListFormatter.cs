using Legacy89DiskKit.Application.FileSystem;

namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public class DefaultFileListFormatter : IFileListFormatter
{
    public FileListView Format(FileListFormatContext context, IFileListLocalizer localizer)
    {
        var summary = CreateSummary(context, localizer);
        var columns = new[]
        {
            new FileListColumn(localizer.NameHeader),
            new FileListColumn(localizer.AttrHeader, true),
            new FileListColumn(localizer.SizeHeader, true),
            new FileListColumn(localizer.ClusterHeader, true)
        };

        var rows = context.Entries
            .Select(entry => new FileListRow(new[]
            {
                entry.DisplayName,
                $"0x{entry.Entry.Attributes.RawAttributes:X2}",
                entry.Entry.Size.ToString(),
                entry.Entry.StartCluster.ToString()
            }))
            .ToArray();

        return new FileListView(summary, columns, rows, Array.Empty<FileListLegendItem>(), Array.Empty<FileListFootnote>());
    }

    private static IReadOnlyList<FileListSummaryItem> CreateSummary(FileListFormatContext context, IFileListLocalizer localizer)
    {
        long used = context.FileSystemInfo.TotalCapacity - context.FileSystemInfo.FreeSpace;
        var items = new List<FileListSummaryItem>
        {
            new FileListSummaryItem(localizer.FileSystemLabel, context.FileSystemInfo.FileSystemName),
            new FileListSummaryItem(localizer.PlatformLabel, context.FileSystemInfo.PlatformId),
            new FileListSummaryItem(localizer.FilesLabel, context.Entries.Count.ToString()),
            new FileListSummaryItem(localizer.TotalLabel, context.FileSystemInfo.TotalCapacity.ToString()),
            new FileListSummaryItem(localizer.UsedLabel, used.ToString()),
            new FileListSummaryItem(localizer.FreeLabel, context.FileSystemInfo.FreeSpace.ToString())
        };

        AddBootSummary(items, context, localizer);
        return items;
    }

    private static void AddBootSummary(List<FileListSummaryItem> items, FileListFormatContext context, IFileListLocalizer localizer)
    {
        var boot = context.BootSummary;
        if (boot == null)
        {
            return;
        }

        items.Add(new FileListSummaryItem(localizer.BootLabel, boot.Mode switch
        {
            BootInfoMode.FileBacked => localizer.BootModeFileBacked,
            BootInfoMode.SectorResident => localizer.BootModeSectorResident,
            _ => localizer.BootModeNone
        }));

        if (!string.IsNullOrWhiteSpace(boot.FileName))
        {
            items.Add(new FileListSummaryItem(localizer.BootFileLabel, boot.FileName));
        }

        if (boot.LoadAddress.HasValue)
        {
            items.Add(new FileListSummaryItem(localizer.BootLoadLabel, $"{boot.LoadAddress.Value:X4}"));
        }

        if (boot.ExecutionAddress.HasValue)
        {
            items.Add(new FileListSummaryItem(localizer.BootExecLabel, $"{boot.ExecutionAddress.Value:X4}"));
        }
    }
}
