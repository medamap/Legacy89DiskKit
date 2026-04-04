using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using System.Globalization;

namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;

public class HuBasicTransferAdapter : IFileSystemTransferAdapter
{
    private const string HuBasicId = "Hu-BASIC";
    private readonly HuBasicFileSystem _fs;

    public HuBasicTransferAdapter(HuBasicFileSystem fs) => _fs = fs;

    public string FileSystemId => HuBasicId;

    public bool Supports(IFileSystem fs) => fs is HuBasicFileSystem;

    public TransferFileEnvelope Export(FileEntry entry)
    {
        var payload = _fs.ReadFile(entry.FullName);
        var metadata = entry.FileSystemMetadata as HuBasicFileMetadata;

        var meta = new Dictionary<string, string>
        {
            ["hubasic.rawMode"] = metadata?.RawModeByte.ToString("X2") ?? entry.Attributes.RawAttributes.ToString("X2"),
            ["hubasic.passwordByte"] = metadata?.PasswordByte.ToString("X2") ?? "20",
            ["hubasic.recordedSize"] = metadata?.RecordedSize.ToString("X4") ?? ((ushort)entry.Size).ToString("X4"),
        };

        if (entry.RawFileName != null)
        {
            meta["hubasic.rawFileName"] = Convert.ToHexString(entry.RawFileName);
        }
        if (entry.RawExtension != null)
        {
            meta["hubasic.rawExtension"] = Convert.ToHexString(entry.RawExtension);
        }

        return new TransferFileEnvelope(
            FileName: entry.FullName,
            Payload: payload,
            ContentKind: entry.Attributes.IsAscii ? ContentKind.Text : ContentKind.Binary,
            SourceFileSystemId: HuBasicId,
            LoadAddress: entry.LoadAddress,
            ExecutionAddress: entry.ExecutionAddress,
            Timestamp: entry.LastModifiedAt.HasValue ? new DateTimeOffset(entry.LastModifiedAt.Value, TimeSpan.Zero) : null,
            EncodingId: "x1_shift_jis",
            Metadata: meta
        );
    }

    public void Import(TransferFileEnvelope envelope, string destFileName)
    {
        var attrs = _fs.CreateDefaultAttributes(envelope.ContentKind == ContentKind.Text);
        
        byte rawMode = attrs.RawAttributes;
        byte passwordByte = 0x20;
        byte[]? rawFileName = null;
        byte[]? rawExtension = null;

        if (envelope.SourceFileSystemId == HuBasicId && envelope.Metadata != null)
        {
            if (envelope.Metadata.TryGetValue("hubasic.rawMode", out var rmStr) && 
                byte.TryParse(rmStr, NumberStyles.HexNumber, null, out var rm))
            {
                rawMode = rm;
            }
            if (envelope.Metadata.TryGetValue("hubasic.passwordByte", out var pbStr) && 
                byte.TryParse(pbStr, NumberStyles.HexNumber, null, out var pb))
            {
                passwordByte = pb;
            }
            if (envelope.Metadata.TryGetValue("hubasic.rawFileName", out var rfnStr))
            {
                rawFileName = Convert.FromHexString(rfnStr);
            }
            if (envelope.Metadata.TryGetValue("hubasic.rawExtension", out var reStr))
            {
                rawExtension = Convert.FromHexString(reStr);
            }
        }

        var extendedAttrs = new ExtendedFileAttributes(
            attrs.StandardAttributes,
            rawMode,
            envelope.ContentKind == ContentKind.Text,
            HuBasicId
        );

        var metadata = new HuBasicFileMetadata(
            GetHuBasicFileType(rawMode),
            passwordByte != 0x20,
            (rawMode & 0x10) != 0,
            (rawMode & 0x20) != 0,
            (rawMode & 0x40) != 0,
            (rawMode & 0x80) != 0,
            (ushort)envelope.Payload.Length,
            envelope.LoadAddress,
            envelope.ExecutionAddress,
            0,
            rawMode,
            passwordByte
        );

        _fs.WriteFileInternal(
            destFileName,
            envelope.Payload,
            extendedAttrs,
            loadAddress: envelope.LoadAddress,
            executionAddress: envelope.ExecutionAddress,
            forcedRawName: rawFileName,
            forcedRawExtension: rawExtension,
            forcedModifiedAt: envelope.Timestamp?.DateTime,
            forcedMetadata: metadata
        );
    }

    private static HuBasicFileType GetHuBasicFileType(byte modeByte)
    {
        if ((modeByte & 0x01) != 0) return HuBasicFileType.Binary;
        if ((modeByte & 0x02) != 0) return HuBasicFileType.Basic;
        if ((modeByte & 0x0C) != 0) return HuBasicFileType.Ascii;
        return HuBasicFileType.Unknown;
    }
}
