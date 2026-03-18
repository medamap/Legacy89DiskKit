namespace Legacy89DiskKit.Domain.FileSystem.Model.XDos;

public record XDosDirectoryEntry(
    byte        RawFileType,
    byte        Attribute,
    string      FileName,
    byte[]      RawFileName,
    ushort      LoadAddress,
    ushort      EndAddress,
    ushort      ExecutionAddress,
    byte        Flags,
    byte        FirstCluster,
    byte        FirstSectorR,
    byte        AlwaysOne
)
{
    public bool IsEmpty   => RawFileType == 0x00 || RawFileType == 0xFF || RawFileType == 0xD5;
    public XDosFileType FileType => (XDosFileType)(RawFileType & 0x7F);
    public bool IsKnownType => (RawFileType & 0x7F) >= 0x01 && (RawFileType & 0x7F) <= 0x07;
    public int FileSize => EndAddress > LoadAddress ? EndAddress - LoadAddress : 0;
}
