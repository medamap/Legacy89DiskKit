using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.Registry;

/// <summary>
/// Proivder that can detect and create a specific file system.
/// </summary>
public interface IFileSystemProvider
{
    /// <summary>
    /// Gets the display name of the file system.
    /// </summary>
    string FileSystemName { get; }

    /// <summary>
    /// Checks if this provider can handle the given disk container.
    /// </summary>
    bool CanHandle(IDiskContainer container);

    /// <summary>
    /// Creates an instance of the file system for the container.
    /// </summary>
    IFileSystem Create(IDiskContainer container);
}
