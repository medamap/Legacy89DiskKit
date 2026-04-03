using System.Text;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;

namespace Legacy89DiskKit.Application.FileSystem;

public sealed class FileInspectionService
{
    public InspectionReport BuildReport(
        IFileSystem fileSystem,
        FileEntry entry,
        string displayName,
        string detailLevel)
    {
        var detail = detailLevel.Trim().ToLowerInvariant();
        var items = new List<InspectionItem>
        {
            new("File", "Name", displayName),
            new("File", "Stored Name", entry.FullName),
            new("File", "Size", entry.Size.ToString()),
            new("File", "Start Cluster", entry.StartCluster.ToString())
        };

        if (entry.LoadAddress is { } load)
        {
            items.Add(new("File", "Load", load.ToString("X4")));
        }

        if (entry.EndAddress is { } end)
        {
            items.Add(new("File", "End", end.ToString("X4")));
        }

        if (entry.ExecutionAddress is { } exec)
        {
            items.Add(new("File", "Exec", exec.ToString("X4")));
        }

        if (entry.LastModifiedAt is { } modified)
        {
            items.Add(new("File", "Modified", modified.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        if (detail is "normal" or "full")
        {
            items.Add(new("File", "Raw Attributes", $"0x{entry.Attributes.RawAttributes:X2}"));
            if (entry.RawFileName is { Length: > 0 })
            {
                items.Add(new("Raw", "Raw Name", Convert.ToHexString(entry.RawFileName)));
            }

            if (entry.RawExtension is { Length: > 0 })
            {
                items.Add(new("Raw", "Raw Extension", Convert.ToHexString(entry.RawExtension)));
            }
        }

        switch (fileSystem)
        {
            case HuBasicFileSystem huBasic:
                AddHuBasicDetails(items, huBasic, entry, detail);
                break;
            case XDosFileSystem xdos:
                AddXDosDetails(items, xdos, entry, detail);
                break;
        }

        return new InspectionReport("File Inspector", items);
    }

    private static void AddHuBasicDetails(List<InspectionItem> items, HuBasicFileSystem fileSystem, FileEntry entry, string detail)
    {
        if (entry.FileSystemMetadata is HuBasicFileMetadata metadata)
        {
            items.Add(new("Hu-BASIC", "Type", metadata.FileType.ToString()));
            items.Add(new("Hu-BASIC", "Flags", FormatHuBasicFlags(metadata)));
            items.Add(new("Hu-BASIC", "Raw Mode", metadata.RawModeByte.ToString("X2")));
            items.Add(new("Hu-BASIC", "Password Byte", metadata.PasswordByte.ToString("X2")));
        }

        var slot = fileSystem.FindDirectorySlot(entry.FullName);
        if (slot is { } dirSlot)
        {
            var dirRecord = fileSystem.GetDirectoryRecordNumber(dirSlot.SectorIndex);
            items.Add(new("Addresses", "DIR-ADR", FormatOffsetAddress((long)dirRecord * 256 + dirSlot.Offset)));
            if (detail == "full")
            {
                var bytes = fileSystem.ReadDirectoryEntryBytes(dirSlot.SectorIndex, dirSlot.Offset);
                items.Add(new("Raw", "Directory Entry Hex", Convert.ToHexString(bytes)));
                items.Add(new("Raw", "Directory Entry ASCII", ToAscii(bytes)));
            }
        }

        var startRecord = fileSystem.GetStartRecordForCluster(entry.StartCluster);
        items.Add(new("Addresses", "BDY-ADR", FormatOffsetAddress((long)startRecord * 256)));

        if (detail == "full")
        {
            var chain = fileSystem.GetClusterChain(entry.StartCluster);
            items.Add(new("Chain", "Clusters", string.Join(" -> ", chain)));
            items.Add(new("Chain", "Records", string.Join(" -> ", chain.Select(fileSystem.GetStartRecordForCluster))));
        }
    }

    private static void AddXDosDetails(List<InspectionItem> items, XDosFileSystem fileSystem, FileEntry entry, string detail)
    {
        if (entry.FileSystemMetadata is not XDosFileMetadata metadata)
        {
            return;
        }

        items.Add(new("X-DOS", "Type", FormatXDosType(metadata)));
        items.Add(new("X-DOS", "Flags", FormatXDosFlags(metadata.RawAttribute)));
        items.Add(new("X-DOS", "Raw Type", metadata.RawFileType.ToString("X4")));
        items.Add(new("X-DOS", "Timestamp Raw", metadata.TimestampRaw.ToString("X8")));

        if (entry.RawFileName == null)
        {
            return;
        }

        var dirEntry = fileSystem.FindDirectoryEntry(entry.RawFileName, metadata.RawFileType);
        var dirSlot = fileSystem.FindDirectorySlot(entry.RawFileName, metadata.RawFileType);
        if (dirSlot is { } slot)
        {
            items.Add(new("Addresses", "DIR-ADR", FormatOffsetAddress(GetXDosOffset(fileSystem.Geometry, 1, slot.Sector - 1, slot.Offset))));
            if (detail == "full")
            {
                var bytes = fileSystem.ReadDirectoryEntryBytes(slot.Sector, slot.Offset);
                items.Add(new("Raw", "Directory Entry Hex", Convert.ToHexString(bytes)));
                items.Add(new("Raw", "Directory Entry ASCII", ToAscii(bytes)));
            }
        }

        if (dirEntry != null)
        {
            var famEntries = fileSystem.GetFamEntries(dirEntry);
            if (famEntries.Count > 0)
            {
                var first = famEntries[0];
                items.Add(new("Addresses", "BDY-ADR", FormatOffsetAddress(GetXDosOffset(fileSystem.Geometry, first.Track, first.Sector - 1, 0))));
            }

            if (detail == "full")
            {
                var chain = famEntries.Select(entryValue => $"{entryValue.Track}:{entryValue.Sector}+{entryValue.RecordCount}");
                items.Add(new("Chain", "FAM", string.Join(" | ", chain)));
            }
        }
    }

    private static long GetXDosOffset(XDosMediaGeometry geometry, int track, int sectorIndex, int intraSectorOffset)
    {
        return (((long)track * geometry.DataSectorsPerTrack) + sectorIndex) * geometry.DataSectorSize + intraSectorOffset;
    }

    private static string FormatOffsetAddress(long value) => $"0x{value:X8}";

    private static string ToAscii(byte[] bytes)
    {
        var chars = bytes.Select(x => x is >= 0x20 and <= 0x7E ? (char)x : '.').ToArray();
        return new string(chars);
    }

    private static string FormatHuBasicFlags(HuBasicFileMetadata metadata)
    {
        Span<char> chars = stackalloc char[4];
        chars[0] = metadata.HasPassword ? 'P' : '-';
        chars[1] = metadata.IsHidden ? 'H' : '-';
        chars[2] = metadata.IsVerify ? 'V' : '-';
        chars[3] = metadata.IsWriteProtected ? 'W' : '-';
        return chars.ToString();
    }

    private static string FormatXDosFlags(byte value)
    {
        return string.Create(6, value, static (buffer, raw) =>
        {
            buffer[0] = (raw & 0x80) != 0 ? 'H' : '-';
            buffer[1] = (raw & 0x40) != 0 ? 'W' : '-';
            buffer[2] = (raw & 0x20) != 0 ? 'S' : '-';
            buffer[3] = (raw & 0x10) != 0 ? 'K' : '-';
            buffer[4] = ':';
            buffer[5] = ((raw & 0x0F) <= 9 ? (char)('0' + (raw & 0x0F)) : (char)('A' + ((raw & 0x0F) - 10)));
        });
    }

    private static string FormatXDosType(XDosFileMetadata metadata)
    {
        var rawType = metadata.RawFileType;
        if ((rawType & 0x8000) != 0 && rawType != (ushort)XDosFileType.Dir)
        {
            rawType &= 0x7FFF;
            return new string(new[]
            {
                DecodeUserTypeChar((rawType >> 10) & 0x1F),
                DecodeUserTypeChar((rawType >> 5) & 0x1F),
                DecodeUserTypeChar(rawType & 0x1F)
            });
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

    private static char DecodeUserTypeChar(int value) => value switch
    {
        0 => '@',
        >= 1 and <= 26 => (char)('A' + value - 1),
        _ => '?'
    };
}
