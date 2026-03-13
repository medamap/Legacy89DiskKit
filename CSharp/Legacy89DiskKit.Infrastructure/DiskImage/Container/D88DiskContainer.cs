using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.DiskImage.Exception;
using Legacy89DiskKit.Infrastructure.DiskImage.D88;
using System.Text;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Container;

public class D88DiskContainer : IDiskContainer, IDisposable
{
    private string _filePath;
    private bool _isReadOnly;
    private byte[] _imageData = Array.Empty<byte>();
    private D88Header _header = new D88Header();
    private readonly Dictionary<(int, int, int), D88SectorData> _sectors;
    private bool _hasChanges = false;
    private bool _disposed = false;

    public string FilePath => _filePath;
    public bool IsReadOnly => _isReadOnly;
    public DiskType DiskType => _header.MediaType;

    public D88DiskContainer(string filePath, bool isReadOnly = false)
        : this(LoadDiskImage(filePath), isReadOnly, filePath)
    {
    }

    /// <summary>
    /// Initializes a D88 container from an in-memory disk image.
    /// </summary>
    public D88DiskContainer(byte[] imageData, bool isReadOnly = true, string filePath = "")
        : this(imageData, isReadOnly, filePath, false)
    {
    }

    private D88DiskContainer(byte[] imageData, bool isReadOnly, string filePath, bool skipClone)
    {
        _filePath = filePath;
        _isReadOnly = isReadOnly;
        _sectors = new Dictionary<(int, int, int), D88SectorData>();
        _imageData = skipClone ? imageData : (byte[])imageData.Clone();
        LoadFromBytes();
    }

    public static D88DiskContainer CreateNew(string filePath, DiskType diskType, string diskName = "", int? sectorsPerTrack = null, ushort? sectorSize = null)
    {
        var container = new D88DiskContainer();
        container._filePath = filePath;
        container._isReadOnly = false;
        container.CreateEmptyImage(diskType, diskName, sectorsPerTrack, sectorSize);
        container.SaveToFile();
        return container;
    }

    /// <summary>
    /// Creates a new in-memory D88 container.
    /// </summary>
    public static D88DiskContainer CreateNewInMemory(string diskName, DiskType diskType, int? sectorsPerTrack = null, ushort? sectorSize = null)
    {
        var container = new D88DiskContainer();
        container._filePath = "";
        container._isReadOnly = false;
        container.CreateEmptyImage(diskType, diskName, sectorsPerTrack, sectorSize);
        container.BuildImageData();
        return new D88DiskContainer(container._imageData, false);
    }

    private D88DiskContainer()
    {
        _filePath = "";
        _isReadOnly = false;
        _sectors = new Dictionary<(int, int, int), D88SectorData>();
    }

    private static byte[] LoadDiskImage(string filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"D88 file not found: {filePath}");
        }

        return File.ReadAllBytes(filePath);
    }

    private void LoadFromBytes()
    {
        try
        {
            if (_imageData.Length < 0x2b0)
            {
                throw new DiskImageException($"Invalid D88 file: too small ({_imageData.Length} bytes)");
            }

            _header = D88ImageParser.ParseHeader(_imageData);
            var parsedSectors = D88ImageParser.ParseSectors(_imageData, _header);
            _sectors.Clear();
            foreach (var entry in parsedSectors)
            {
                _sectors[entry.Key] = entry.Value;
            }
        }
        catch (Exception ex) when (ex is not DiskImageException)
        {
            throw new DiskImageException($"Error loading D88 file: {ex.Message}", ex);
        }
    }

    private static int GetMaxSectorsPerTrack(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => 16,
            DiskType.TwoDD => 16,
            DiskType.TwoHD => 26,
            _ => 26
        };
    }

    public byte[] ReadSector(int cylinder, int head, int sector)
    {
        if (!_sectors.TryGetValue((cylinder, head, sector), out var d88Sector))
            throw new DiskImageException($"Sector not found: C={cylinder}, H={head}, R={sector}");
        return d88Sector.Data;
    }

    public DiskContainerMetadata GetMetadata()
    {
        var geometry = GetGeometry();
        return new DiskContainerMetadata(
            ImageFormat: "d88-sector-container",
            DiskType: _header.MediaType,
            Geometry: geometry,
            IsWriteProtected: _header.WriteProtect || _isReadOnly,
            DeclaredImageSize: _header.DiskSize);
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted)
    {
        return ReadSector(cylinder, head, sector);
    }

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        if (_isReadOnly) throw new DiskImageException("Disk image is read-only");
        if (!_sectors.TryGetValue((cylinder, head, sector), out var d88Sector))
            throw new DiskImageException($"Sector not found: C={cylinder}, H={head}, R={sector}");
            
        d88Sector.Data = (byte[])data.Clone();
        d88Sector.ActualSize = (ushort)data.Length;
        _hasChanges = true;
    }

    public bool SectorExists(int cylinder, int head, int sector)
    {
        return _sectors.ContainsKey((cylinder, head, sector));
    }

    public IEnumerable<SectorInfo> GetAllSectors()
    {
        return _sectors.Values.Select(s => new SectorInfo(
            s.Cylinder, s.Head, s.Sector, s.ActualSize, s.Deleted, s.Status != 0));
    }

    private DiskGeometryInfo GetGeometry()
    {
        var sectors = _sectors.Values.ToList();
        if (sectors.Count == 0)
        {
            return new DiskGeometryInfo(
                Cylinders: _header.MediaType == DiskType.TwoHD ? 77 : 40,
                Heads: 2,
                SectorsPerTrack: GetMaxSectorsPerTrack(_header.MediaType),
                BytesPerSector: _header.MediaType == DiskType.TwoHD ? 1024 : 256);
        }

        return new DiskGeometryInfo(
            Cylinders: sectors.Max(s => (int)s.Cylinder) + 1,
            Heads: sectors.Max(s => (int)s.Head) + 1,
            SectorsPerTrack: sectors
                .GroupBy(s => (s.Cylinder, s.Head))
                .Select(g => g.Count())
                .DefaultIfEmpty(0)
                .Max(),
            BytesPerSector: sectors.Max(s => (int)s.ActualSize));
    }

    public void Save() => SaveAs(_filePath);

    public void SaveAs(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new DiskImageException("Cannot save a disk image without a file path");
        BuildImageData();
        File.WriteAllBytes(filePath, _imageData);
        _hasChanges = false;
        _filePath = filePath;
    }

    /// <summary>
    /// Returns the current D88 image bytes.
    /// </summary>
    public byte[] ToImageData()
    {
        if (_hasChanges || _imageData.Length == 0)
        {
            BuildImageData();
        }

        return (byte[])_imageData.Clone();
    }

    private void BuildImageData()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        
        // Write Name (17 bytes)
        var nameBytes = new byte[17];
        var nameData = Encoding.ASCII.GetBytes(_header.ImageName);
        Array.Copy(nameData, nameBytes, Math.Min(nameData.Length, 17));
        writer.Write(nameBytes);
        
        writer.Write(new byte[9]); // reserved
        writer.Write((byte)(_header.WriteProtect ? 0x10 : 0x00));
        writer.Write((byte)_header.MediaType);
        
        long diskSizePos = writer.BaseStream.Position;
        writer.Write((uint)0); // Placeholder for DiskSize

        long trackOffsetsPos = writer.BaseStream.Position;
        var trackOffsets = new uint[164];
        for (int i = 0; i < 164; i++) writer.Write((uint)0); // Placeholder for track offsets

        var currentOffset = 0x2b0u;
        for (int track = 0; track < 164; track++)
        {
            var cylinder = track / 2;
            var head = track % 2;
            var trackSectors = _sectors.Values
                .Where(s => s.Cylinder == cylinder && s.Head == head)
                .OrderBy(s => s.Sector)
                .ToList();

            if (trackSectors.Any())
            {
                trackOffsets[track] = currentOffset;
                foreach (var sector in trackSectors)
                {
                    writer.Write(sector.Cylinder);
                    writer.Write(sector.Head);
                    writer.Write(sector.Sector);
                    writer.Write(sector.SectorSizeN);
                    writer.Write((ushort)trackSectors.Count);
                    writer.Write(sector.Density);
                    writer.Write((byte)(sector.Deleted ? 0x10 : 0x00));
                    writer.Write(sector.Status);
                    writer.Write(new byte[5]);
                    writer.Write((ushort)sector.Data.Length);
                    writer.Write(sector.Data);
                    currentOffset += 16u + (uint)sector.Data.Length;
                }
            }
        }
        
        // Finalize header
        uint totalSize = (uint)writer.BaseStream.Length;
        writer.BaseStream.Seek(diskSizePos, SeekOrigin.Begin);
        writer.Write(totalSize);
        
        writer.BaseStream.Seek(trackOffsetsPos, SeekOrigin.Begin);
        for (int i = 0; i < 164; i++) writer.Write(trackOffsets[i]);
        
        _imageData = stream.ToArray();
    }

    private void CreateEmptyImage(DiskType diskType, string diskName, int? sectorsPerTrackParam = null, ushort? sectorSizeParam = null)
    {
        _header = new D88Header { ImageName = diskName, MediaType = diskType };
        _sectors.Clear();

        int maxCylinders = diskType == DiskType.TwoHD ? 77 : 40;
        int maxHeads = 2;
        int sectorsPerTrack = sectorsPerTrackParam ?? GetMaxSectorsPerTrack(diskType);
        ushort sectorSize = sectorSizeParam ?? (ushort)(diskType == DiskType.TwoHD ? 1024 : 256);
        byte sectorSizeN = (byte)(sectorSize == 1024 ? 3 : 1);

        for (int c = 0; c < maxCylinders; c++)
        {
            for (int h = 0; h < maxHeads; h++)
            {
                for (int s = 1; s <= sectorsPerTrack; s++)
                {
                    var d88Sector = new D88SectorData
                    {
                        Cylinder = (byte)c,
                        Head = (byte)h,
                        Sector = (byte)s,
                        SectorSizeN = sectorSizeN,
                        SectorCount = (ushort)sectorsPerTrack,
                        Density = (byte)(diskType == DiskType.TwoHD ? 0x01 : 0x00), // 0: Double, 1: High
                        Deleted = false,
                        Status = 0,
                        ActualSize = sectorSize,
                        Data = new byte[sectorSize]
                    };
                    _sectors[(c, h, s)] = d88Sector;
                }
            }
        }
    }

    private void SaveToFile() => Save();

    public void Dispose()
    {
        if (_hasChanges && !IsReadOnly && !string.IsNullOrEmpty(_filePath))
        {
            try { Save(); } catch { }
        }
        _disposed = true;
    }

}
