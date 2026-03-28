using System.Globalization;
using System.Text;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using FileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos;

public class XDosTransferAdapter : IFileSystemTransferAdapter
{
    private const string XDosId = "X-DOS";

    private static readonly IReadOnlySet<XDosFileType> TextTypes =
        new HashSet<XDosFileType> { XDosFileType.Asc };

    static XDosTransferAdapter()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private readonly XDosFileSystem _fs;

    public XDosTransferAdapter(XDosFileSystem fs) => _fs = fs;

    public string FileSystemId => XDosId;

    public bool Supports(IFileSystem fs) => fs is XDosFileSystem;

    public bool IsCloneMode { get; set; }

    public TransferFileEnvelope Export(FileEntry entry)
    {
        byte[] rawName = entry.RawFileName ?? PadToSixteen(Encoding.Latin1.GetBytes(entry.FileName));

        var xdosEntry = _fs.GetFilesWithMetadata()
            .FirstOrDefault(e => !e.IsEmpty && e.RawFileName.SequenceEqual(rawName))
            ?? throw new FileNotFoundException($"File not found: {entry.FileName}");

        byte[] payload = _fs.ReadFileRaw(xdosEntry.RawFileName);

        bool isText = TextTypes.Contains(xdosEntry.FileType);
        var contentKind = isText ? ContentKind.Text : ContentKind.Binary;

        ushort? execAddress = isText ? null : (ushort?)xdosEntry.ExecAddressOrSizeHigh;

        var meta = new Dictionary<string, string>
        {
            ["xdos.fileType"]      = xdosEntry.RawFileType.ToString("X4"),
            ["xdos.rawAttributes"] = xdosEntry.Attribute.ToString("X2"),
            ["xdos.isAscii"]       = isText ? "true" : "false",
            ["xdos.timestampRaw"]  = xdosEntry.TimestampRaw.ToString("X6"),
        };

        if (IsCloneMode)
        {
            meta["xdos.isClone"] = "true";
            meta["xdos.famTrack"] = xdosEntry.FamPointer.Track.ToString();
            meta["xdos.famSector"] = xdosEntry.FamPointer.Sector.ToString();
            
            var famEntries = _fs.GetFamEntries(xdosEntry);
            meta["xdos.dataRecords"] = string.Join(";", famEntries.Select(e => $"{e.Track},{e.Sector},{e.RecordCount}"));
        }

        var dirSlot = _fs.FindDirectorySlot(xdosEntry.RawFileName, xdosEntry.RawFileType);
        if (dirSlot != null)
        {
            meta["xdos.dirSector"] = dirSlot.Value.Sector.ToString();
            meta["xdos.dirOffset"] = dirSlot.Value.Offset.ToString();
        }

        return new TransferFileEnvelope(
            FileName:          xdosEntry.FileName,
            Payload:           payload,
            ContentKind:       contentKind,
            SourceFileSystemId: XDosId,
            LoadAddress:       xdosEntry.StartAddress,
            ExecutionAddress:  execAddress,
            Timestamp:         ToDateTimeOffset(XDosTimestampHelper.DecodeTimestamp(xdosEntry.TimestampRaw)),
            EncodingId:        isText ? "shift_jis" : null,
            Metadata:          meta);
    }

    private static DateTimeOffset? ToDateTimeOffset(DateTime? dt)
    {
        if (dt == null) return null;
        return new DateTimeOffset(dt.Value.Year, dt.Value.Month, dt.Value.Day, 0, 0, 0, TimeSpan.Zero);
    }

    public void Import(TransferFileEnvelope envelope, string destFileName)
    {
        byte[] payload = ResolvePayload(envelope);

        ushort? forcedRawType = null;
        byte    rawAttributes = 0x00;
        int?    forcedDirSector = null;
        int?    forcedDirOffset = null;
        int?    forcedFamTrack = null;
        int?    forcedFamSector = null;
        List<(int Track, int Sector)>? forcedRecords = null;
        uint?   forcedTimestampRaw = null;

        if (envelope.SourceFileSystemId == XDosId && envelope.Metadata != null)
        {
            if (envelope.Metadata.TryGetValue("xdos.fileType", out var ftStr)
                && ushort.TryParse(ftStr, NumberStyles.HexNumber, null, out var ft))
            {
                forcedRawType = ft;
            }

            if (envelope.Metadata.TryGetValue("xdos.rawAttributes", out var raStr)
                && byte.TryParse(raStr, NumberStyles.HexNumber, null, out var ra))
            {
                rawAttributes = ra;
            }

            if (envelope.Metadata.TryGetValue("xdos.dirSector", out var dsStr)
                && int.TryParse(dsStr, out var ds))
            {
                forcedDirSector = ds;
            }

            if (envelope.Metadata.TryGetValue("xdos.dirOffset", out var doStr)
                && int.TryParse(doStr, out var dOff))
            {
                forcedDirOffset = dOff;
            }

            if (envelope.Metadata.TryGetValue("xdos.timestampRaw", out var tsStr)
                && uint.TryParse(tsStr, NumberStyles.HexNumber, null, out var ts))
            {
                forcedTimestampRaw = ts;
            }

            if (IsCloneMode && envelope.Metadata.TryGetValue("xdos.isClone", out var icStr) && icStr == "true")
            {
                if (envelope.Metadata.TryGetValue("xdos.famTrack", out var ftkStr) && int.TryParse(ftkStr, out var ftk))
                {
                    forcedFamTrack = ftk;
                }
                if (envelope.Metadata.TryGetValue("xdos.famSector", out var fscStr) && int.TryParse(fscStr, out var fsc))
                {
                    forcedFamSector = fsc;
                }
                if (envelope.Metadata.TryGetValue("xdos.dataRecords", out var drStr))
                {
                    forcedRecords = ParseDataRecords(drStr);
                }
            }
        }

        bool isAscii = envelope.ContentKind == ContentKind.Text;
        var attrs = new ExtendedFileAttributes(FileAttributes.None, rawAttributes, isAscii, XDosId);

        // If we have forced placement, we must construct the full record list: [FAM, Data...]
        List<(int Track, int Sector)>? fullForcedRecords = null;
        if (forcedFamTrack.HasValue && forcedFamSector.HasValue && forcedRecords != null)
        {
            fullForcedRecords = new List<(int Track, int Sector)> { (forcedFamTrack.Value, forcedFamSector.Value) };
            fullForcedRecords.AddRange(forcedRecords);
        }

        _fs.WriteFileInternal(
            destFileName,
            payload,
            attrs,
            loadAddress:        envelope.LoadAddress,
            executionAddress:   envelope.ExecutionAddress,
            forcedRawType:      forcedRawType,
            forcedRecords:      fullForcedRecords,
            forcedDirSector:    forcedDirSector,
            forcedDirOffset:    forcedDirOffset,
            forcedTimestampRaw: forcedTimestampRaw);
    }

    private static List<(int Track, int Sector)> ParseDataRecords(string drStr)
    {
        var result = new List<(int Track, int Sector)>();
        var parts = drStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var coords = part.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (coords.Length == 3 &&
                int.TryParse(coords[0], out var t) &&
                int.TryParse(coords[1], out var s) &&
                int.TryParse(coords[2], out var c))
            {
                for (int i = 0; i < c; i++)
                {
                    result.Add((t, s + i));
                }
            }
        }
        return result;
    }

    private static byte[] ResolvePayload(TransferFileEnvelope envelope)
    {
        if (envelope.ContentKind != ContentKind.Text)
            return envelope.Payload;

        string? enc = envelope.EncodingId?.ToLowerInvariant();

        if (enc == "shift_jis" || (enc == null && Is7BitAscii(envelope.Payload)))
            return envelope.Payload;

        if (enc == "utf-8" || enc == "utf8")
        {
            var sjis = Encoding.GetEncoding(932,
                new EncoderExceptionFallback(),
                new DecoderReplacementFallback("?"));
            string text = Encoding.UTF8.GetString(envelope.Payload);
            return sjis.GetBytes(text);
        }

        if (enc == null)
            throw new InvalidOperationException(
                "Text payload contains non-ASCII bytes but EncodingId is not specified.");

        throw new NotSupportedException($"Unsupported encoding: {envelope.EncodingId}");
    }

    private static bool Is7BitAscii(byte[] data)
    {
        foreach (byte b in data)
            if (b > 0x7F) return false;
        return true;
    }

    private static byte[] PadToSixteen(byte[] raw)
    {
        if (raw.Length == 16) return raw;
        var result = new byte[16];
        Array.Fill(result, (byte)0x20);
        Array.Copy(raw, 0, result, 0, Math.Min(raw.Length, 16));
        return result;
    }
}
