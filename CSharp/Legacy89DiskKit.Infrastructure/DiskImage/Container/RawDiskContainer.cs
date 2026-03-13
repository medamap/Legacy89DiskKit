using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.DiskImage.Exception;
using Legacy89DiskKit.Infrastructure.DiskImage.Raw;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Container;

public class RawDiskContainer : IDiskContainer
{
    private readonly byte[] _diskData;
    private readonly string _filePath;
    private readonly bool _readOnly;
    private bool _hasChanges = false;
    
    private readonly RawDiskGeometry _geometry;
    private readonly RawSectorAddressCalculator _addressCalculator;

    public RawDiskContainer(string filePath, bool readOnly = true)
        : this(LoadDiskImage(filePath), filePath, readOnly)
    {
    }

    /// <summary>
    /// Initializes a raw disk container from an in-memory disk image.
    /// </summary>
    public RawDiskContainer(byte[] diskData, bool readOnly = true, string filePath = "")
        : this(diskData ?? throw new ArgumentNullException(nameof(diskData)), filePath, readOnly)
    {
    }

    private RawDiskContainer(byte[] diskData, string filePath, bool readOnly)
    {
        _filePath = filePath ?? "";
        _readOnly = readOnly;
        _diskData = (byte[])diskData.Clone();
        
        _geometry = RawDiskGeometryDetector.Detect(_diskData.Length);
        _addressCalculator = new RawSectorAddressCalculator(_geometry);
    }

    private static byte[] LoadDiskImage(string filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Disk image file not found: {filePath}");
        }

        return File.ReadAllBytes(filePath);
    }

    public static RawDiskContainer CreateNew(string filePath, DiskType type, int? sectorsPerTrack = null, ushort? sectorSize = null)
    {
        var data = CreateEmptyDiskData(type);
        File.WriteAllBytes(filePath, data);
        return new RawDiskContainer(data, false, filePath);
    }

    /// <summary>
    /// Creates a new in-memory raw disk container.
    /// </summary>
    public static RawDiskContainer CreateNewInMemory(DiskType type, int? sectorsPerTrack = null, ushort? sectorSize = null)
    {
        return new RawDiskContainer(CreateEmptyDiskData(type), false);
    }

    private static byte[] CreateEmptyDiskData(DiskType type)
    {
        int size = type switch
        {
            DiskType.TwoD => 327680,
            DiskType.TwoDD => 720 * 1024,
            DiskType.TwoHD => 1440 * 1024,
            _ => 327680
        };

        return new byte[size];
    }

    public byte[] ReadSector(int cylinder, int head, int sector) => ReadSector(cylinder, head, sector, false);

    public DiskContainerMetadata GetMetadata()
    {
        return new DiskContainerMetadata(
            ImageFormat: "raw-sector-image",
            DiskType: _geometry.DiskType,
            Geometry: new DiskGeometryInfo(
                _geometry.Cylinders,
                _geometry.Sides,
                _geometry.SectorsPerTrack,
                _geometry.BytesPerSector),
            IsWriteProtected: _readOnly,
            DeclaredImageSize: _diskData.LongLength);
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted)
    {
        ValidateAddress(cylinder, head, sector);
        int offset = _addressCalculator.CalculateOffset(cylinder, head, sector);
        byte[] sectorData = new byte[_geometry.BytesPerSector];
        Array.Copy(_diskData, offset, sectorData, 0, _geometry.BytesPerSector);
        return sectorData;
    }

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        if (_readOnly) throw new DiskImageException("Disk image is read-only");
        ValidateAddress(cylinder, head, sector);
        if (data.Length != _geometry.BytesPerSector)
            throw new ArgumentException($"Sector size must be {_geometry.BytesPerSector} bytes");

        int offset = _addressCalculator.CalculateOffset(cylinder, head, sector);
        Array.Copy(data, 0, _diskData, offset, _geometry.BytesPerSector);
        _hasChanges = true;
    }

    public bool SectorExists(int cylinder, int head, int sector)
    {
        return _addressCalculator.SectorExists(cylinder, head, sector);
    }

    public IEnumerable<SectorInfo> GetAllSectors()
    {
        for (int c = 0; c < _geometry.Cylinders; c++)
        {
            for (int h = 0; h < _geometry.Sides; h++)
            {
                for (int s = 1; s <= _geometry.SectorsPerTrack; s++)
                {
                    yield return new SectorInfo(c, h, s, _geometry.BytesPerSector, false, false);
                }
            }
        }
    }

    public void Save()
    {
        if (_readOnly) throw new DiskImageException("Cannot save read-only disk image");
        if (string.IsNullOrEmpty(_filePath)) throw new DiskImageException("Cannot save a disk image without a file path");
        File.WriteAllBytes(_filePath, _diskData);
    }

    public void SaveAs(string filePath) => File.WriteAllBytes(filePath, _diskData);

    /// <summary>
    /// Returns the current disk image bytes.
    /// </summary>
    public byte[] ToImageData() => (byte[])_diskData.Clone();

    private void ValidateAddress(int cylinder, int head, int sector)
    {
        if (!SectorExists(cylinder, head, sector))
            throw new ArgumentOutOfRangeException($"Invalid sector address: C:{cylinder} H:{head} S:{sector}");
    }

    public string FilePath => _filePath;
    public bool IsReadOnly => _readOnly;
    public DiskType DiskType => _geometry.DiskType;

    public void Dispose() 
    {
        if (_hasChanges && !_readOnly && !string.IsNullOrEmpty(_filePath))
        {
            try { Save(); } catch { }
        }
    }
}
