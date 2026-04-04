using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.Native.Domain;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class LibraryNativeDiskSession : INativeDiskSession, IDiskContainer
{
    private readonly int _handle;
    private bool _disposed;
    private LibraryNativeFileSystem? _fileSystem;

    public LibraryNativeDiskSession(int handle)
    {
        _handle = handle;
    }

    // INativeDiskSession implementation
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

    // IDiskContainer implementation
    public string FilePath => ""; // Not easily available from handle without extra metadata

    public bool IsReadOnly
    {
        get
        {
            var meta = GetContainerMetadata();
            return meta?.IsWriteProtected ?? true;
        }
    }

    public DiskType DiskType
    {
        get
        {
            var meta = GetContainerMetadata();
            return meta?.DiskType ?? DiskType.TwoD;
        }
    }

    DiskContainerMetadata IDiskContainer.GetMetadata() => GetContainerMetadata() ?? throw new InvalidOperationException();

    public byte[] ReadSector(int cylinder, int head, int sector)
    {
        byte[] buffer = new byte[256]; // Assuming 256 for now or fetch from metadata
        int result = NativeLibraryImports.ReadSector(_handle, cylinder, head, sector, buffer, buffer.Length);
        if (result < 0) throw new Exception($"Failed to read sector (Error: {result})");
        return buffer;
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted) => ReadSector(cylinder, head, sector);

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        int result = NativeLibraryImports.WriteSector(_handle, cylinder, head, sector, data, data.Length);
        if (result < 0) throw new Exception($"Failed to write sector (Error: {result})");
    }

    public bool SectorExists(int cylinder, int head, int sector) => true; // Simplified

    public IEnumerable<SectorInfo> GetAllSectors() => Enumerable.Empty<SectorInfo>(); // Simplified

    public void Save()
    {
        int result = NativeLibraryImports.Save(_handle);
        if (result < 0) throw new Exception($"Failed to save disk (Error: {result})");
    }

    public void SaveAs(string filePath) => throw new NotSupportedException();

    public void Dispose()
    {
        if (!_disposed)
        {
            CloseDisk();
            _disposed = true;
        }
    }
}
