using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.DiskImage.Exception;
using System.Text;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Container;

public class D88DiskContainer : IDiskContainer, IDisposable
{
    private string _filePath;
    private bool _isReadOnly;
    private byte[] _imageData = Array.Empty<byte>();
    private D88Header _header = new D88Header();
    private readonly Dictionary<(int, int, int), D88Sector> _sectors;
    private bool _hasChanges = false;
    private bool _disposed = false;

    public string FilePath => _filePath;
    public bool IsReadOnly => _isReadOnly;
    public DiskType DiskType => _header.MediaType;

    public D88DiskContainer(string filePath, bool isReadOnly = false)
    {
        _filePath = filePath;
        _isReadOnly = isReadOnly;
        _sectors = new Dictionary<(int, int, int), D88Sector>();
        
        if (File.Exists(filePath))
        {
            LoadFromFile();
        }
        else
        {
            throw new FileNotFoundException($"D88 file not found: {filePath}");
        }
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

    private D88DiskContainer()
    {
        _filePath = "";
        _isReadOnly = false;
        _sectors = new Dictionary<(int, int, int), D88Sector>();
    }

    private void LoadFromFile()
    {
        try
        {
            _imageData = File.ReadAllBytes(_filePath);
            if (_imageData.Length < 0x2b0)
                throw new DiskImageException($"Invalid D88 file: too small ({_imageData.Length} bytes)");

            ParseHeader();
            ParseSectors();
        }
        catch (Exception ex) when (ex is not DiskImageException)
        {
            throw new DiskImageException($"Error loading D88 file: {ex.Message}", ex);
        }
    }

    private void ParseHeader()
    {
        using var stream = new MemoryStream(_imageData);
        using var reader = new BinaryReader(stream);

        var imageName = reader.ReadBytes(17);
        var diskName = Encoding.ASCII.GetString(imageName).TrimEnd('\0');
        
        reader.BaseStream.Seek(17 + 9, SeekOrigin.Begin);
        var protect = reader.ReadByte();
        var mediaTypeByte = reader.ReadByte();
        
        if (!Enum.IsDefined(typeof(DiskType), mediaTypeByte))
            throw new DiskImageException($"Invalid media type: 0x{mediaTypeByte:X2}");
        
        var mediaType = (DiskType)mediaTypeByte;
        var diskSize = reader.ReadUInt32();

        reader.BaseStream.Seek(0x20, SeekOrigin.Begin);
        var trackOffsets = new uint[164];
        for (int i = 0; i < 164; i++)
        {
            trackOffsets[i] = reader.ReadUInt32();
        }

        _header = new D88Header
        {
            ImageName = diskName,
            WriteProtect = protect != 0,
            MediaType = mediaType,
            DiskSize = diskSize,
            TrackOffsets = trackOffsets
        };
    }

    private void ParseSectors()
    {
        _sectors.Clear();
        for (int track = 0; track < 164; track++)
        {
            if (_header.TrackOffsets[track] == 0) continue;
            ParseTrack(track, _header.TrackOffsets[track]);
        }
    }

    private void ParseTrack(int trackIndex, uint offset)
    {
        using var stream = new MemoryStream(_imageData);
        using var reader = new BinaryReader(stream);
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        
        var sectorsInTrack = 0;
        var maxSectorsPerTrack = GetMaxSectorsPerTrack(_header.MediaType);
        
        while (reader.BaseStream.Position < _imageData.Length)
        {
            var sectorStart = reader.BaseStream.Position;
            if (reader.BaseStream.Position + 16 > _imageData.Length) break;
            
            var cylinder = reader.ReadByte();
            var head = reader.ReadByte();
            var sector = reader.ReadByte();
            var sectorSizeN = reader.ReadByte();
            var sectorCount = reader.ReadUInt16();
            var density = reader.ReadByte();
            var deleted = reader.ReadByte();
            var status = reader.ReadByte();
            reader.ReadBytes(5); // reserved
            var actualSize = reader.ReadUInt16();
            
            if (reader.BaseStream.Position + actualSize > _imageData.Length) break;
            var data = reader.ReadBytes(actualSize);
            
            var d88Sector = new D88Sector
            {
                Cylinder = cylinder,
                Head = head,
                Sector = sector,
                SectorSizeN = sectorSizeN,
                SectorCount = sectorCount,
                Density = density,
                Deleted = deleted != 0,
                Status = status,
                ActualSize = actualSize,
                Data = data
            };
            
            _sectors[(cylinder, head, sector)] = d88Sector;
            sectorsInTrack++;
            
            // Ported logic for sector count and track switching
            if (sectorsInTrack >= sectorCount) break;
            if (trackIndex < 163 && _header.TrackOffsets[trackIndex + 1] > 0)
            {
                if (reader.BaseStream.Position >= _header.TrackOffsets[trackIndex + 1]) break;
            }
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

    public void Save() => SaveAs(_filePath);

    public void SaveAs(string filePath)
    {
        BuildImageData();
        File.WriteAllBytes(filePath, _imageData);
        _hasChanges = false;
        _filePath = filePath;
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
                    var d88Sector = new D88Sector
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

    private class D88Header
    {
        public string ImageName { get; set; } = "";
        public bool WriteProtect { get; set; }
        public DiskType MediaType { get; set; }
        public uint DiskSize { get; set; }
        public uint[] TrackOffsets { get; set; } = new uint[164];
    }

    private class D88Sector
    {
        public byte Cylinder { get; set; }
        public byte Head { get; set; }
        public byte Sector { get; set; }
        public byte SectorSizeN { get; set; }
        public ushort SectorCount { get; set; }
        public byte Density { get; set; }
        public bool Deleted { get; set; }
        public byte Status { get; set; }
        public ushort ActualSize { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
