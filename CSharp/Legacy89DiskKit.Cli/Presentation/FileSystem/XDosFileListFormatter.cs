using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.FileSystem.Application;
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
            new FileListColumn(localizer.DirectoryAddressHeader, true),
            new FileListColumn(localizer.BodyAddressHeader, true)
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
                FormatOffset(entry.DirectoryOffset),
                FormatOffset(entry.BodyOffset)
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
            var rawType = metadata.RawFileType;
            if ((rawType & 0x8000) != 0 && rawType != (ushort)XDosFileType.Dir)
            {
                return DecodeUserDefinedType(rawType);
            }

            ushort subtype = (ushort)(rawType & 0x00FF);
            return metadata.FileType switch
            {
                XDosFileType.Bin => "BIN",
                XDosFileType.Bas => "BAS",
                XDosFileType.Cmd => subtype switch
                {
                    0x10 => "SX-BASIC",
                    0x11 => "XASM",
                    0x12 => "XEDIT",
                    0x13 => "SLANG",
                    0x00 => "CMD",
                    _ => $"CMD:{subtype:X2}"
                },
                XDosFileType.Asc => "ASC",
                XDosFileType.Sub => subtype switch
                {
                    0x00 => "SUB",
                    0x01 => "PRINTER",
                    >= 0x10 and <= 0x1F => $"OVL{subtype - 0x10:X1}",
                    >= 0x20 and <= 0x2F => $"ACM{subtype - 0x20:X1}",
                    _ => $"SUB:{subtype:X2}"
                },
                XDosFileType.Bat => "BAT",
                XDosFileType.Sys => subtype switch
                {
                    0x00 => "SYS",
                    0x01 => "SYS:X1",
                    0x02 => "SYS:MZ",
                    _ => $"SYS:{subtype:X2}"
                },
                XDosFileType.Dic => "DIC",
                XDosFileType.Dir => "DIR",
                _ => $"0x{rawType:X4}"
            };
        }

        return "UNK";
    }

    private static string DecodeUserDefinedType(ushort rawType)
    {
        rawType &= 0x7FFF;
        Span<char> chars = stackalloc char[3];
        chars[0] = DecodeUserTypeChar((rawType >> 10) & 0x1F);
        chars[1] = DecodeUserTypeChar((rawType >> 5) & 0x1F);
        chars[2] = DecodeUserTypeChar(rawType & 0x1F);
        return chars.ToString();
    }

    private static char DecodeUserTypeChar(int value)
    {
        return value switch
        {
            0 => '@',
            >= 1 and <= 26 => (char)('A' + value - 1),
            _ => '?'
        };
    }

    private static string FormatHex(ushort? value)
    {
        return value.HasValue ? $"{value.Value:X4}" : "----";
    }

    private static string FormatOffset(long? value)
    {
        return value.HasValue ? $"{value.Value:X8}" : "--------";
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
