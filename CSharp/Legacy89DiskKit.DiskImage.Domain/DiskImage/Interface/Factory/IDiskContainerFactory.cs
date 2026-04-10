using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Domain.DiskImage.Interface.Factory;

public interface IDiskContainerFactory
{
    IDiskContainer Open(string filePath, bool readOnly = true);
    IDiskContainer Open(byte[] imageData, string imageFormat, bool readOnly = true);
    IDiskContainer Create(string filePath, DiskType diskType, string diskName = "", int? sectorsPerTrack = null, ushort? sectorSize = null);
}
