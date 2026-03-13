using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using System.Text;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public class HuBasicDirParser
{
    private readonly HuBasicConfiguration _config;
    private readonly Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder.X1CharacterEncoder _encoder = new();

    public HuBasicDirParser(HuBasicConfiguration config)
    {
        _config = config;
    }

    public FileEntry Parse(byte[] data)
    {
        var rawEntry = HuBasicDirectoryEntryCodec.Parse(data, _encoder);

        var standardAttr = Domain.FileSystem.Model.FileAttributes.None;
        if ((rawEntry.ModeByte & 0x80) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.Directory;
        if ((rawEntry.ModeByte & 0x40) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.ReadOnly;
        if ((rawEntry.ModeByte & 0x10) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.Hidden;

        var fileType = GetFileType(rawEntry.ModeByte);
        bool isAscii = fileType == HuBasicFileType.Ascii;

        var extAttr = new ExtendedFileAttributes(
            standardAttr,
            rawEntry.ModeByte,
            isAscii,
            "Hu-BASIC"
        );

        var metadata = new HuBasicFileMetadata(
            fileType,
            rawEntry.PasswordByte != 0x20,
            (rawEntry.ModeByte & 0x10) != 0,
            (rawEntry.ModeByte & 0x20) != 0,
            (rawEntry.ModeByte & 0x40) != 0,
            (rawEntry.ModeByte & 0x80) != 0,
            rawEntry.RecordedSize,
            rawEntry.LoadAddress,
            rawEntry.ExecutionAddress,
            rawEntry.StartCluster,
            rawEntry.ModeByte,
            rawEntry.PasswordByte
        );

        return new FileEntry(
            rawEntry.FileName,
            rawEntry.Extension,
            rawEntry.RecordedSize,
            null, // CreatedAt not supported
            rawEntry.ModifiedDate,
            extAttr,
            rawEntry.StartCluster,
            rawEntry.LoadAddress,
            (ushort?)(rawEntry.LoadAddress + rawEntry.RecordedSize - 1),
            rawEntry.ExecutionAddress,
            rawEntry.RawFileName,
            rawEntry.RawExtension,
            metadata
        );
    }

    public void WriteToBuffer(byte[] buffer, int offset, FileEntry entry)
    {
        var att = entry.Attributes;
        var metadata = entry.FileSystemMetadata as HuBasicFileMetadata;
        byte modeByte = metadata != null
            ? BuildModeByte(metadata)
            : BuildModeByte(att, entry);

        var rawEntry = new HuBasicDirectoryEntryData(
            modeByte,
            metadata?.PasswordByte ?? 0x20,
            (entry.RawFileName != null && entry.RawFileName.Length == 13) ? entry.RawFileName : Array.Empty<byte>(),
            (entry.RawExtension != null && entry.RawExtension.Length == 3) ? entry.RawExtension : Array.Empty<byte>(),
            entry.FileName,
            entry.Extension,
            (ushort)entry.Size,
            entry.LoadAddress ?? 0,
            entry.ExecutionAddress ?? 0,
            entry.LastModifiedAt ?? DateTime.Now,
            entry.StartCluster);

        HuBasicDirectoryEntryCodec.WriteToBuffer(buffer, offset, rawEntry, _encoder);
    }

    private static HuBasicFileType GetFileType(byte modeByte)
    {
        if ((modeByte & 0x01) != 0) return HuBasicFileType.Binary;
        if ((modeByte & 0x02) != 0) return HuBasicFileType.Basic;
        if ((modeByte & 0x0C) != 0) return HuBasicFileType.Ascii;
        return HuBasicFileType.Unknown;
    }

    private static byte BuildModeByte(HuBasicFileMetadata metadata)
    {
        byte modeByte = 0;

        if (metadata.IsDirectory) modeByte |= 0x80;
        if (metadata.IsWriteProtected) modeByte |= 0x40;
        if (metadata.IsVerify) modeByte |= 0x20;
        if (metadata.IsHidden) modeByte |= 0x10;

        modeByte |= metadata.FileType switch
        {
            HuBasicFileType.Binary => (byte)0x01,
            HuBasicFileType.Basic => (byte)0x02,
            HuBasicFileType.Ascii => (byte)0x04,
            _ => (byte)0x00
        };

        return modeByte;
    }

    private static byte BuildModeByte(ExtendedFileAttributes attributes, FileEntry entry)
    {
        byte modeByte = (byte)(attributes.RawAttributes & 0x0F);

        if ((attributes.StandardAttributes & Domain.FileSystem.Model.FileAttributes.Directory) != 0) modeByte |= 0x80;
        if ((attributes.StandardAttributes & Domain.FileSystem.Model.FileAttributes.ReadOnly) != 0) modeByte |= 0x40;
        if ((attributes.StandardAttributes & Domain.FileSystem.Model.FileAttributes.Hidden) != 0) modeByte |= 0x10;

        if ((modeByte & 0x0F) == 0)
        {
            modeByte |= entry.Attributes.IsAscii ? (byte)0x04 : (byte)0x01;
        }

        return modeByte;
    }
}
