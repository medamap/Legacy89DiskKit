using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.FileSystem.Application;

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
