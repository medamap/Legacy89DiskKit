using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Exception;

namespace Legacy89DiskKit.DiskImage.Infrastructure.Container;

public class DskDiskContainer : IDiskContainer
{
    private string _filePath;
    private bool _isReadOnly;
    private byte[] _imageData = Array.Empty<byte>();
    private DskHeader _header = new DskHeader();

    public string FilePath => _filePath;
    public bool IsReadOnly => _isReadOnly;
    public DiskType DiskType => _header.DiskType;

    public DskDiskContainer(string filePath, bool isReadOnly = false)
    {
        _filePath = filePath;
        _isReadOnly = isReadOnly;
        
        if (File.Exists(filePath))
        {
            LoadFromFile();
        }
        else
        {
            throw new FileNotFoundException($"DSK file not found: {filePath}");
        }
    }

    public static DskDiskContainer CreateNew(string filePath, DiskType diskType, string diskName = "")
    {
        var container = new DskDiskContainer();
        container._filePath = filePath;
        container._isReadOnly = false;
        container.CreateEmptyImage(diskType);
        container.SaveToFile();
        return container;
    }

    private DskDiskContainer()
    {
        // Private constructor for CreateNew
        _filePath = string.Empty;
    }

    public byte[] ReadSector(int cylinder, int head, int sector)
    {
        return ReadSector(cylinder, head, sector, false);
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted)
    {
        ValidateParameters(cylinder, head, sector);

        try
        {
            var sectorOffset = CalculateSectorOffset(cylinder, head, sector);
            var sectorData = new byte[_header.SectorSize];
            
            if (sectorOffset + _header.SectorSize <= _imageData.Length)
            {
                Buffer.BlockCopy(_imageData, sectorOffset, sectorData, 0, _header.SectorSize);
            }
            else
            {
                if (allowCorrupted)
                {
                    Console.WriteLine($"Warning: Reading beyond disk image bounds (C:{cylinder} H:{head} S:{sector})");
                    // Return partial data or zeros
                    var availableBytes = Math.Max(0, _imageData.Length - sectorOffset);
                    if (availableBytes > 0)
                    {
                        Buffer.BlockCopy(_imageData, sectorOffset, sectorData, 0, availableBytes);
                    }
                }
                else
                {
                    throw new InvalidDiskFormatException($"Sector beyond disk bounds: C:{cylinder} H:{head} S:{sector}");
                }
            }

            return sectorData;
        }
        catch (Exception ex) when (allowCorrupted)
        {
            Console.WriteLine($"Warning: Error reading sector C:{cylinder} H:{head} S:{sector}: {ex.Message}");
            return new byte[_header.SectorSize]; // Return zeros for corrupted sectors
        }
    }

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        if (_isReadOnly)
        {
            throw new InvalidOperationException("Cannot write to read-only disk");
        }

        ValidateParameters(cylinder, head, sector);

        if (data.Length != _header.SectorSize)
        {
            throw new ArgumentException($"Data size {data.Length} does not match sector size {_header.SectorSize}");
        }

        try
        {
            var sectorOffset = CalculateSectorOffset(cylinder, head, sector);
            Array.Copy(data, 0, _imageData, sectorOffset, _header.SectorSize);
        }
        catch (Exception ex)
        {
            throw new DiskImageException($"Failed to write sector C:{cylinder} H:{head} S:{sector}: {ex.Message}", ex);
        }
    }

    public void Flush()
    {
        if (_isReadOnly) return;
        
        SaveToFile();
    }

    private void CreateEmptyImage(DiskType diskType)
    {
        // Set header parameters based on disk type
        _header.DiskType = diskType;
        _header.SectorSize = 512;

        switch (diskType)
        {
            case DiskType.TwoD:
                _header.Cylinders = 40;
                _header.Heads = 1;
                _header.SectorsPerTrack = 16;
                break;
            case DiskType.TwoDD:
                _header.Cylinders = 40;
                _header.Heads = 2;
                _header.SectorsPerTrack = 16;
                break;
            case DiskType.TwoHD:
                _header.Cylinders = 80;
                _header.Heads = 2;
                _header.SectorsPerTrack = 18;
                break;
            default:
                throw new ArgumentException($"Unsupported disk type: {diskType}");
        }

        // Calculate total image size
        var totalSize = _header.Cylinders * _header.Heads * _header.SectorsPerTrack * _header.SectorSize;
        _imageData = new byte[totalSize];
        
        // Fill with zeros (empty disk)
        Array.Fill(_imageData, (byte)0x00);
    }

    private void SaveToFile()
    {
        try
        {
            File.WriteAllBytes(_filePath, _imageData);
        }
        catch (Exception ex)
        {
            throw new DiskImageException($"Failed to save DSK file to {_filePath}: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        // DSK format doesn't require special cleanup
    }

    private void LoadFromFile()
    {
        try
        {
            _imageData = File.ReadAllBytes(_filePath);
            
            if (_imageData.Length == 0)
            {
                throw new InvalidDiskFormatException("DSK file is empty");
            }

            AnalyzeDiskStructure();
        }
        catch (Exception ex) when (!(ex is InvalidDiskFormatException))
        {
            throw new InvalidDiskFormatException($"Failed to load DSK file: {ex.Message}", ex);
        }
    }

    private void AnalyzeDiskStructure()
    {
        // DSK format analysis - typically raw sector data
        // Common DSK formats for retro computers
        
        var fileSize = _imageData.Length;
        
        // Standard floppy disk sizes
        if (fileSize == 163840) // 160KB - single sided
        {
            _header = new DskHeader
            {
                Cylinders = 40,
                Heads = 1,
                SectorsPerTrack = 8,
                SectorSize = 512,
                DiskType = DiskType.TwoD
            };
        }
        else if (fileSize == 327680) // 320KB - double sided
        {
            _header = new DskHeader
            {
                Cylinders = 40,
                Heads = 2,
                SectorsPerTrack = 8,
                SectorSize = 512,
                DiskType = DiskType.TwoD
            };
        }
        else if (fileSize == 368640) // 360KB - 9 sectors per track
        {
            _header = new DskHeader
            {
                Cylinders = 40,
                Heads = 2,
                SectorsPerTrack = 9,
                SectorSize = 512,
                DiskType = DiskType.TwoDD
            };
        }
        else if (fileSize == 737280) // 720KB - 3.5" DD
        {
            _header = new DskHeader
            {
                Cylinders = 80,
                Heads = 2,
                SectorsPerTrack = 9,
                SectorSize = 512,
                DiskType = DiskType.TwoDD
            };
        }
        else if (fileSize == 1228800) // 1.2MB - 5.25" HD
        {
            _header = new DskHeader
            {
                Cylinders = 80,
                Heads = 2,
                SectorsPerTrack = 15,
                SectorSize = 512,
                DiskType = DiskType.TwoHD
            };
        }
        else if (fileSize == 1474560) // 1.44MB - 3.5" HD
        {
            _header = new DskHeader
            {
                Cylinders = 80,
                Heads = 2,
                SectorsPerTrack = 18,
                SectorSize = 512,
                DiskType = DiskType.TwoHD
            };
        }
        else
        {
            // Custom size - try to guess parameters
            _header = GuessGeometry(fileSize);
        }
    }

    private DskHeader GuessGeometry(int fileSize)
    {
        // Try different sector sizes
        int[] sectorSizes = { 512, 256, 1024 };
        
        foreach (var sectorSize in sectorSizes)
        {
            var totalSectors = fileSize / sectorSize;
            if (fileSize % sectorSize != 0) continue;
            
            // Try common geometries
            var geometries = new[]
            {
                (40, 1), (40, 2), (80, 1), (80, 2), (77, 2)
            };
            
            foreach (var (cylinders, heads) in geometries)
            {
                var sectorsPerTrack = totalSectors / (cylinders * heads);
                if (sectorsPerTrack > 0 && sectorsPerTrack <= 26 && 
                    cylinders * heads * sectorsPerTrack == totalSectors)
                {
                    return new DskHeader
                    {
                        Cylinders = cylinders,
                        Heads = heads,
                        SectorsPerTrack = sectorsPerTrack,
                        SectorSize = sectorSize,
                        DiskType = cylinders >= 80 ? DiskType.TwoHD : DiskType.TwoD
                    };
                }
            }
        }
        
        // Fallback to default
        return new DskHeader
        {
            Cylinders = 80,
            Heads = 2,
            SectorsPerTrack = fileSize / (80 * 2 * 512),
            SectorSize = 512,
            DiskType = DiskType.TwoDD
        };
    }

    private int CalculateSectorOffset(int cylinder, int head, int sector)
    {
        // DSK format stores sectors sequentially: C0H0S1, C0H0S2, ..., C0H1S1, C0H1S2, ..., C1H0S1, ...
        var trackNumber = cylinder * _header.Heads + head;
        var sectorIndex = trackNumber * _header.SectorsPerTrack + (sector - 1);
        return sectorIndex * _header.SectorSize;
    }

    private void ValidateParameters(int cylinder, int head, int sector)
    {
        if (cylinder < 0 || cylinder >= _header.Cylinders)
        {
            throw new ArgumentOutOfRangeException(nameof(cylinder), 
                $"Cylinder {cylinder} out of range (0-{_header.Cylinders - 1})");
        }
        
        if (head < 0 || head >= _header.Heads)
        {
            throw new ArgumentOutOfRangeException(nameof(head), 
                $"Head {head} out of range (0-{_header.Heads - 1})");
        }
        
        if (sector < 1 || sector > _header.SectorsPerTrack)
        {
            throw new ArgumentOutOfRangeException(nameof(sector), 
                $"Sector {sector} out of range (1-{_header.SectorsPerTrack})");
        }
    }

    public bool SectorExists(int cylinder, int head, int sector)
    {
        return cylinder >= 0 && cylinder < _header.Cylinders &&
               head >= 0 && head < _header.Heads &&
               sector >= 1 && sector <= _header.SectorsPerTrack;
    }

    public IEnumerable<SectorInfo> GetAllSectors()
    {
        for (int c = 0; c < _header.Cylinders; c++)
        {
            for (int h = 0; h < _header.Heads; h++)
            {
                for (int s = 1; s <= _header.SectorsPerTrack; s++)
                {
                    yield return new SectorInfo(c, h, s, _header.SectorSize, false, false);
                }
            }
        }
    }

    public void Save()
    {
        SaveAs(_filePath);
    }

    public void SaveAs(string filePath)
    {
        try
        {
            File.WriteAllBytes(filePath, _imageData);
        }
        catch (IOException ex)
        {
            throw new DiskImageException($"Failed to save DSK image: {ex.Message}", ex);
        }
    }
}

public class DskHeader
{
    public int Cylinders { get; set; }
    public int Heads { get; set; }
    public int SectorsPerTrack { get; set; }
    public int SectorSize { get; set; }
    public DiskType DiskType { get; set; }
}