namespace Legacy89DiskKit.Domain.FileSystem.Model.XDos;

public enum XDosFileType : ushort
{
    Killed = 0x0000,
    Bin    = 0x0100,
    Bas    = 0x0200,
    Cmd    = 0x0300,
    Asc    = 0x0400,
    Sub    = 0x0500,
    Bat    = 0x0600,
    Sys    = 0x0700,
    Dic    = 0x0800,
    Dir    = 0x8000,
    End    = 0xFFFF,
}
