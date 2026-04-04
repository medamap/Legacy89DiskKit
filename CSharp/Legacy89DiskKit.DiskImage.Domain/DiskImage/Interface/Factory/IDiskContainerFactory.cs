using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;

namespace Legacy89DiskKit.DiskImage.Domain.Interface.Factory;

public interface IDiskContainerFactory
{
    IDiskContainer Open(string filePath, bool readOnly = true);
    IDiskContainer Open(byte[] imageData, string imageFormat, bool readOnly = true);
    IDiskContainer Create(string filePath, DiskType diskType, string diskName = "", int? sectorsPerTrack = null, ushort? sectorSize = null);
}
