namespace Legacy89DiskKit.FileSystem.Domain.Model.XDos;

public record XDosDirectoryEntry(
    ushort         RawFileType,
    string         FileName,
    byte[]         RawFileName,
    ushort         StartAddress,
    ushort         SizeLow,
    ushort         ExecAddressOrSizeHigh,
    uint           TimestampRaw,
    byte           Attribute,
    XDosFamPointer FamPointer)
{
    public bool IsKilled => RawFileType == 0x0000;
    public bool IsEnd    => RawFileType == 0xFFFF;
    public bool IsEmpty  => IsKilled || IsEnd;
    public XDosFileType FileType => (XDosFileType)RawFileType;
    public int FileSize =>
        FileType == XDosFileType.Asc
            ? (int)(((uint)ExecAddressOrSizeHigh << 16) | SizeLow)
            : SizeLow;
}
