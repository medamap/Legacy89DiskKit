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
        };

        return new TransferFileEnvelope(
            FileName:          xdosEntry.FileName,
            Payload:           payload,
            ContentKind:       contentKind,
            SourceFileSystemId: XDosId,
            LoadAddress:       xdosEntry.StartAddress,
            ExecutionAddress:  execAddress,
            Timestamp:         DecodeTimestamp(xdosEntry.TimestampRaw),
            EncodingId:        isText ? "shift_jis" : null,
            Metadata:          meta);
    }

    public void Import(TransferFileEnvelope envelope, string destFileName)
    {
        byte[] payload = ResolvePayload(envelope);

        ushort? forcedRawType = null;
        byte    rawAttributes = 0x00;

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
        }

        bool isAscii = envelope.ContentKind == ContentKind.Text;
        var attrs = new ExtendedFileAttributes(FileAttributes.None, rawAttributes, isAscii, XDosId);

        _fs.WriteFileInternal(
            destFileName,
            payload,
            attrs,
            loadAddress:      envelope.LoadAddress,
            executionAddress: envelope.ExecutionAddress,
            forcedRawType:    forcedRawType);
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

    private static DateTimeOffset? DecodeTimestamp(uint raw)
    {
        if (raw == 0) return null;
        byte year  = (byte)(raw >> 16);
        byte month = (byte)(raw >> 8);
        byte day   = (byte)(raw);
        int y = ((year  >> 4) & 0xF) * 10 + (year  & 0xF) + 2000;
        int m = ((month >> 4) & 0xF) * 10 + (month & 0xF);
        int d = ((day   >> 4) & 0xF) * 10 + (day   & 0xF);
        if (m < 1 || m > 12 || d < 1 || d > 31) return null;
        try { return new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero); }
        catch { return null; }
    }
}
