using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class LibraryNativeDiskSession : INativeDiskSession
{
    private readonly int _handle;
    private bool _disposed;
    private LibraryNativeFileSystem? _fileSystem;

    public LibraryNativeDiskSession(int handle)
    {
        _handle = handle;
    }

    public IFileSystem? FileSystem
    {
        get
        {
            if (_fileSystem == null && _handle >= 0)
            {
                _fileSystem = new LibraryNativeFileSystem(_handle);
            }
            return _fileSystem;
        }
    }

    public DiskContainerMetadata? GetContainerMetadata()
    {
        if (NativeLibraryImports.GetContainerMetadata(_handle, out var metadata) == 0)
        {
            return new DiskContainerMetadata(
                metadata.ImageFormat,
                (DiskType)metadata.DiskType,
                new DiskGeometryInfo(
                    metadata.Cylinders,
                    metadata.Heads,
                    metadata.SectorsPerTrack,
                    metadata.BytesPerSector
                ),
                metadata.IsWriteProtected != 0,
                metadata.DeclaredImageSize
            );
        }
        return null;
    }

    public void CloseDisk()
    {
        if (_handle >= 0)
        {
            NativeLibraryImports.CloseDisk(_handle);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CloseDisk();
            _disposed = true;
        }
    }
}
