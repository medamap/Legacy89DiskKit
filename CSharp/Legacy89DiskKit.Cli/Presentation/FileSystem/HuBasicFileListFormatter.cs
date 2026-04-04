using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public class HuBasicFileListFormatter : IFileListFormatter
{
    public FileListView Format(FileListFormatContext context, IFileListLocalizer localizer)
    {
        var footnotes = new FootnoteRegistry();
        var summary = CreateSummary(context, localizer);
        var columns = new[]
        {
            new FileListColumn(localizer.NameHeader),
            new FileListColumn(localizer.TypeHeader),
            new FileListColumn(localizer.FlagsHeader),
            new FileListColumn(localizer.SizeHeader, true),
            new FileListColumn(localizer.LoadHeader, true),
            new FileListColumn(localizer.EndHeader, true),
            new FileListColumn(localizer.ExecHeader, true),
            new FileListColumn(localizer.DirectoryAddressHeader, true),
            new FileListColumn(localizer.BodyAddressHeader, true),
            new FileListColumn(localizer.NoteHeader)
        };

        var rows = context.Entries
            .Select(entry => new FileListRow(new[]
            {
                FormatName(entry),
                FormatType(entry.Entry),
                FormatFlags(entry.Entry),
                FormatSize(entry),
                FormatHex(entry.Entry.LoadAddress),
                FormatEnd(entry),
                FormatHex(entry.Entry.ExecutionAddress),
                FormatOffset(entry.DirectoryOffset),
                FormatOffset(entry.BodyOffset),
                footnotes.Register(GetNotes(entry.Entry, localizer).ToArray())
            }))
            .ToArray();

        var legends = new[]
        {
            new FileListLegendItem("P", localizer.HuBasicFlagPassword),
            new FileListLegendItem("H", localizer.HuBasicFlagHidden),
            new FileListLegendItem("V", localizer.HuBasicFlagVerify),
            new FileListLegendItem("W", localizer.HuBasicFlagWriteProtect)
        };

        return new FileListView(summary, columns, rows, legends, footnotes.ToList());
    }

    private static string FormatName(FileListEntryContext entry)
    {
        var name = DisplayWidthUtility.PadRight(entry.DisplayBaseName, 13);
        var extension = DisplayWidthUtility.PadRight(entry.DisplayExtension, 3);
        return string.IsNullOrWhiteSpace(entry.DisplayExtension)
            ? name
            : $"{name}.{extension}";
    }

    private static string FormatType(FileEntry entry)
    {
        if (entry.FileSystemMetadata is not HuBasicFileMetadata metadata)
        {
            return "UNK";
        }

        if (metadata.IsDirectory)
        {
            return "DIR";
        }

        return metadata.FileType switch
        {
            HuBasicFileType.Binary => "BIN",
            HuBasicFileType.Basic => "BAS",
            HuBasicFileType.Ascii => "ASC",
            _ => "UNK"
        };
    }

    private static string FormatFlags(FileEntry entry)
    {
        if (entry.FileSystemMetadata is not HuBasicFileMetadata metadata)
        {
            return "----";
        }

        return string.Create(4, metadata, static (buffer, value) =>
        {
            buffer[0] = value.HasPassword ? 'P' : '-';
            buffer[1] = value.IsHidden ? 'H' : '-';
            buffer[2] = value.IsVerify ? 'V' : '-';
            buffer[3] = value.IsWriteProtected ? 'W' : '-';
        });
    }

    private static string FormatSize(FileListEntryContext entry)
    {
        long size = entry.ActualSize ?? entry.Entry.Size;
        return size.ToString();
    }

    private static string FormatEnd(FileListEntryContext entry)
    {
        if (entry.Entry.FileSystemMetadata is not HuBasicFileMetadata metadata)
        {
            return FormatHex(entry.Entry.EndAddress);
        }

        if (metadata.FileType == HuBasicFileType.Binary && metadata.LoadAddress.HasValue)
        {
            long size = entry.ActualSize ?? entry.Entry.Size;
            if (size > 0)
            {
                return $"{metadata.LoadAddress.Value + size - 1:X4}";
            }
        }

        return FormatHex(entry.Entry.EndAddress);
    }

    private static string FormatHex(ushort? value)
    {
        return value.HasValue ? $"{value.Value:X4}" : "----";
    }

    private static string FormatOffset(long? value)
    {
        return value.HasValue ? $"{value.Value:X8}" : "--------";
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

    private static IEnumerable<string> GetNotes(FileEntry entry, IFileListLocalizer localizer)
    {
        if (entry.FileSystemMetadata is not HuBasicFileMetadata metadata)
        {
            yield break;
        }

        if (IsLabelLike(entry, metadata))
        {
            yield return localizer.HuBasicLabelEntryNote;
            yield break;
        }

        if (metadata.FileType == HuBasicFileType.Ascii)
        {
            yield return localizer.HuBasicAsciiNote;
        }

        if (metadata.FileType == HuBasicFileType.Basic)
        {
            yield return localizer.HuBasicBasicNote;
        }
    }

    private static bool IsLabelLike(FileEntry entry, HuBasicFileMetadata metadata)
    {
        if (metadata.FileType != HuBasicFileType.Ascii)
        {
            return false;
        }

        var name = entry.FullName;
        var looksDecorative = name.All(ch => ch is '-' or '.' or ' ');
        var hasSentinelAddresses = entry.LoadAddress == 0xFFFF &&
                                   entry.ExecutionAddress == 0xFFFF &&
                                   (entry.EndAddress == 0xFFFF || entry.Size == 0);
        var suspiciousCluster = entry.StartCluster >= 0x7FFF;
        var labelFlags = metadata.HasPassword && metadata.IsWriteProtected && !metadata.IsHidden && !metadata.IsVerify;

        return (looksDecorative || suspiciousCluster || hasSentinelAddresses) &&
               (labelFlags || suspiciousCluster || hasSentinelAddresses);
    }
}
