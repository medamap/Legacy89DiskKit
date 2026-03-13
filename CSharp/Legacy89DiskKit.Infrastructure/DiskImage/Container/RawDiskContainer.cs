using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.DiskImage.Exception;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Container;

public class RawDiskContainer : IDiskContainer
{
    private readonly byte[] _diskData;
    private readonly string _filePath;
    private readonly bool _readOnly;
    private bool _hasChanges = false;
    
    private readonly int _cylinders;
    private readonly int _sides;
    private readonly int _sectorsPerTrack;
    private readonly int _bytesPerSector;
    private readonly DiskType _diskType;

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
        
        (_cylinders, _sides, _sectorsPerTrack, _bytesPerSector, _diskType) = DetectGeometry(_diskData.Length);
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

    private static (int c, int h, int spt, int bps, DiskType type) DetectGeometry(long size)
    {
        return size switch
        {
            327680 => (40, 2, 16, 256, DiskType.TwoD),    // X1/PC88 2D
            655360 => (80, 2, 16, 256, DiskType.TwoDD),   // X1/PC88 2DD (256B)
            737280 => (80, 2, 9, 512, DiskType.TwoDD),    // MSX/PC 2DD (512B)
            1261568 => (77, 2, 8, 1024, DiskType.TwoHD),  // PC-98 2HD
            1474560 => (80, 2, 18, 512, DiskType.TwoHD),  // PC 2HD
            _ => (40, 2, 16, 256, DiskType.TwoD)           // Default to 2D but warn?
        };
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

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted)
    {
        ValidateAddress(cylinder, head, sector);
        int offset = CalculateOffset(cylinder, head, sector);
        byte[] sectorData = new byte[_bytesPerSector];
        Array.Copy(_diskData, offset, sectorData, 0, _bytesPerSector);
        return sectorData;
    }

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        if (_readOnly) throw new DiskImageException("Disk image is read-only");
        ValidateAddress(cylinder, head, sector);
        if (data.Length != _bytesPerSector)
            throw new ArgumentException($"Sector size must be {_bytesPerSector} bytes");

        int offset = CalculateOffset(cylinder, head, sector);
        Array.Copy(data, 0, _diskData, offset, _bytesPerSector);
        _hasChanges = true;
    }

    public bool SectorExists(int cylinder, int head, int sector)
    {
        return cylinder >= 0 && cylinder < _cylinders &&
               head >= 0 && head < _sides &&
               sector >= 1 && sector <= _sectorsPerTrack;
    }

    public IEnumerable<SectorInfo> GetAllSectors()
    {
        for (int c = 0; c < _cylinders; c++)
        {
            for (int h = 0; h < _sides; h++)
            {
                for (int s = 1; s <= _sectorsPerTrack; s++)
                {
                    yield return new SectorInfo(c, h, s, _bytesPerSector, false, false);
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

    private int CalculateOffset(int cylinder, int head, int sector)
    {
        return ((cylinder * _sides + head) * _sectorsPerTrack + (sector - 1)) * _bytesPerSector;
    }

    private void ValidateAddress(int cylinder, int head, int sector)
    {
        if (!SectorExists(cylinder, head, sector))
            throw new ArgumentOutOfRangeException($"Invalid sector address: C:{cylinder} H:{head} S:{sector}");
    }

    public string FilePath => _filePath;
    public bool IsReadOnly => _readOnly;
    public DiskType DiskType => _diskType;

    public void Dispose() 
    {
        if (_hasChanges && !_readOnly && !string.IsNullOrEmpty(_filePath))
        {
            try { Save(); } catch { }
        }
    }
}
