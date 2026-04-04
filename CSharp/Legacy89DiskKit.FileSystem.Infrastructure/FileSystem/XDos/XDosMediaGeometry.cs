using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos;

public record XDosMediaGeometry(
    int DataSectorsPerTrack,
    int DataSectorSize,
    int BootSectorsPerTrack,
    int BootSectorSize,
    int TotalTracks
)
{
    public static XDosMediaGeometry FromDiskType(DiskType diskType) =>
        diskType switch
        {
            DiskType.TwoHD => new(16, 512, 16, 256, 160),
            DiskType.TwoDD => new(10, 512, 16, 256, 160),
            _ => new(10, 512, 16, 256, 80)
        };

    public (int sectors, ushort size, byte density) GetTrackGeometry(int c, int h) =>
        (c == 0 && h == 0)
            ? (BootSectorsPerTrack, (ushort)BootSectorSize, (byte)0x00)
            : (DataSectorsPerTrack, (ushort)DataSectorSize, (byte)0x00);
}
