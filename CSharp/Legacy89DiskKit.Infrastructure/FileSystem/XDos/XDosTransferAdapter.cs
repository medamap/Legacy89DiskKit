using System.Globalization;
using System.Text;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using FileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos;

public class XDosTransferAdapter : IFileSystemTransferAdapter
{
    private const string FileSystemId = "X-DOS";

    private static readonly IReadOnlySet<XDosFileType> TextTypes =
        new HashSet<XDosFileType> { XDosFileType.Asc };

    static XDosTransferAdapter()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private readonly XDosFileSystem _fs;

    public XDosTransferAdapter(XDosFileSystem fs) => _fs = fs;

    public TransferFileEnvelope Export(string fileName)
    {
        var entry = _fs.GetFilesWithMetadata()
            .FirstOrDefault(e => !e.IsEmpty && e.FileName == fileName)
            ?? throw new FileNotFoundException($"File not found: {fileName}");

        byte[] payload = _fs.ReadFileRaw(entry.RawFileName);

        bool isText = TextTypes.Contains(entry.FileType);
        var contentKind = isText ? ContentKind.Text : ContentKind.Binary;

        var meta = new Dictionary<string, string>
        {
            ["xdos.fileType"]       = entry.RawFileType.ToString("X4"),
            ["xdos.rawAttributes"]  = entry.Attribute.ToString("X2"),
            ["xdos.isAscii"]        = isText ? "true" : "false",
        };

        return new TransferFileEnvelope(
            FileName:         entry.FileName,
            Payload:          payload,
            ContentKind:      contentKind,
            SourceFileSystemId: FileSystemId,
            LoadAddress:      entry.StartAddress,
            ExecutionAddress: entry.ExecAddressOrSizeHigh,
            Timestamp:        entry.TimestampRaw == 0 ? null : entry.TimestampRaw,
            EncodingId:       isText ? "shift_jis" : null,
            Metadata:         meta);
    }

    public void Import(TransferFileEnvelope envelope, string destFileName)
    {
        byte[] payload = ResolvePayload(envelope);

        ushort? forcedRawType  = null;
        byte    rawAttributes  = 0x00;

        if (envelope.SourceFileSystemId == FileSystemId && envelope.Metadata != null)
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
        var attrs = new ExtendedFileAttributes(FileAttributes.None, rawAttributes, isAscii, FileSystemId);

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

        if (enc == "shift_jis" || enc == null && Is7BitAscii(envelope.Payload))
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
}
