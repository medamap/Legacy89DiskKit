using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.DiskImage.Domain.Interface.Factory;

public interface IDiskContainerFactory
{
    IDiskContainer OpenDiskImage(string filePath, bool readOnly = false);
    IDiskContainer CreateNewDiskImage(string filePath, DiskType diskType, string diskName);
}