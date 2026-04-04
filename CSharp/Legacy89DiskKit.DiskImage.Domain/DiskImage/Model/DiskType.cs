namespace Legacy89DiskKit.DiskImage.Domain.Model;

public enum DiskType : byte
{
    TwoD = 0x00,
    TwoDD = 0x10, 
    TwoHD = 0x20,
    HardDisk = 0x80
}
