namespace Legacy89DiskKit.Domain.FileSystem.Model.XDos;

public enum XDosFileType : byte
{
    SubProgram  = 0x01,
    BasicText   = 0x02,
    Binary      = 0x03,
    Data        = 0x04,
    Overlay     = 0x05,
    Script      = 0x06,
    System      = 0x07,
}
