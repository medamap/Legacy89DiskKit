using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;

namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;

public static class HuBasicDirectoryRules
{
    public static FileEntry CreateFileEntryForWrite(
        string fileName,
        byte[] data,
        ExtendedFileAttributes attributes,
        int startCluster,
        ushort? loadAddress,
        ushort? executionAddress)
    {
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (attributes == null) throw new ArgumentNullException(nameof(attributes));

        var (name, ext) = HuBasicNameRules.ParseFileName(fileName);
        var fileType = attributes.IsAscii || (attributes.RawAttributes & 0x0C) != 0
            ? HuBasicFileType.Ascii
            : HuBasicFileType.Binary;

        var metadata = new HuBasicFileMetadata(
            fileType,
            false,
            false,
            false,
            false,
            false,
            (ushort)data.Length,
            loadAddress,
            executionAddress,
            startCluster,
            attributes.RawAttributes
        );

        ushort? endAddress = loadAddress.HasValue ? (ushort?)(loadAddress.Value + data.Length - 1) : null;

        return new FileEntry(
            name,
            ext,
            data.Length,
            null,
            DateTime.Now,
            attributes,
            startCluster,
            loadAddress,
            endAddress,
            executionAddress,
            null,
            null,
            metadata
        );
    }
}
