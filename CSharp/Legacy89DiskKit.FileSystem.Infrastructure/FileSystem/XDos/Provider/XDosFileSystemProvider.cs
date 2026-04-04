using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;

public class XDosFileSystemProvider : IFileSystemProvider
{
    public string FileSystemName => "X-DOS";

    public bool CanHandle(IDiskContainer container)
    {
        try
        {
            var sector = container.ReadSector(0, 0, 1);
            return sector.Length >= 25 && sector[0] == 0x01 && sector[24] == 0x88;
        }
        catch
        {
            return false;
        }
    }

    public IFileSystem Create(IDiskContainer container)
        => new XDosFileSystem(container);
}
