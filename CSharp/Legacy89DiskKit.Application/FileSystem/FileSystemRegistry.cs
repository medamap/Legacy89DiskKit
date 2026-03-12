using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Application.FileSystem;

public class FileSystemRegistry : IFileSystemRegistry
{
    private readonly List<IFileSystemProvider> _providers = new();

    public void Register(IFileSystemProvider provider)
    {
        _providers.Add(provider);
    }

    public IFileSystem? DetectAndCreate(IDiskContainer container)
    {
        foreach (var provider in _providers)
        {
            if (provider.CanHandle(container))
            {
                return provider.Create(container);
            }
        }
        return null;
    }
}
