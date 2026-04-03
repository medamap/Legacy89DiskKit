using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public class XDosFileListFormatter : IFileListFormatter
{
    public FileListView Format(FileListFormatContext context, IFileListLocalizer localizer)
    {
        var summary = CreateSummary(context, localizer);
        var columns = new[]
        {
            new FileListColumn(localizer.NameHeader),
            new FileListColumn(localizer.TypeHeader),
            new FileListColumn(localizer.AttrHeader, true),
            new FileListColumn(localizer.SizeHeader, true),
            new FileListColumn(localizer.LoadHeader, true),
            new FileListColumn(localizer.ExecHeader, true),
            new FileListColumn(localizer.ClusterHeader, true)
        };

        var rows = context.Entries
            .Select(entry => new FileListRow(new[]
            {
                entry.DisplayName,
                FormatType(entry.Entry),
                FormatAttributeFlags(entry.Entry.Attributes.RawAttributes),
                entry.Entry.Size.ToString(),
                FormatHex(entry.Entry.LoadAddress),
                FormatHex(entry.Entry.ExecutionAddress),
                entry.Entry.StartCluster.ToString()
            }))
            .ToArray();

        var legends = new[]
        {
            new FileListLegendItem("H", localizer.XDosFlagSecret),
            new FileListLegendItem("W", localizer.XDosFlagWriteProtect),
            new FileListLegendItem("S", localizer.XDosFlagSystem),
            new FileListLegendItem("K", localizer.XDosFlagKanji),
            new FileListLegendItem("0-F", localizer.XDosFlagUserNibble)
        };

        return new FileListView(summary, columns, rows, legends, Array.Empty<FileListFootnote>());
    }

    private static string FormatType(FileEntry entry)
    {
        if (entry.FileSystemMetadata is XDosFileMetadata metadata)
        {
            return metadata.FileType switch
            {
                XDosFileType.Bin => "BIN",
                XDosFileType.Bas => "BAS",
                XDosFileType.Cmd => "CMD",
                XDosFileType.Asc => "ASC",
                XDosFileType.Sub => "SUB",
                XDosFileType.Bat => "BAT",
                XDosFileType.Sys => "SYS",
                XDosFileType.Dic => "DIC",
                XDosFileType.Dir => "DIR",
                _ => $"0x{metadata.RawFileType:X4}"
            };
        }

        return "UNK";
    }

    private static string FormatHex(ushort? value)
    {
        return value.HasValue ? $"{value.Value:X4}" : "----";
    }

    private static string FormatAttributeFlags(byte value)
    {
        return string.Create(6, value, static (buffer, raw) =>
        {
            buffer[0] = (raw & 0x80) != 0 ? 'H' : '-';
            buffer[1] = (raw & 0x40) != 0 ? 'W' : '-';
            buffer[2] = (raw & 0x20) != 0 ? 'S' : '-';
            buffer[3] = (raw & 0x10) != 0 ? 'K' : '-';
            buffer[4] = ':';
            buffer[5] = GetHexNibble((byte)(raw & 0x0F));
        });
    }

    private static char GetHexNibble(byte value)
    {
        return (char)(value < 10 ? '0' + value : 'A' + (value - 10));
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
