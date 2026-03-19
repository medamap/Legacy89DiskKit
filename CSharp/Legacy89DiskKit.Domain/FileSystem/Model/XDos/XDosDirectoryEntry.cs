namespace Legacy89DiskKit.Domain.FileSystem.Model.XDos;

public record XDosDirectoryEntry(
    byte        RawFileType,
    byte        Attribute,
    string      FileName,
    byte[]      RawFileName,
    ushort      LoadAddress,
    ushort      ByteSize,
    ushort      ExecutionAddress,
    ushort      DatePacked,
    ushort      TimePacked,
    byte        Flags,
    byte        FirstCluster,
    byte        FirstSectorR,
    byte        AlwaysOne
)
{
    public bool IsEmpty     => RawFileType == 0x00 || RawFileType == 0xFF || RawFileType == 0xD5;
    public XDosFileType FileType  => (XDosFileType)(RawFileType & 0x7F);
    public int  FileSize    => ByteSize;
}
