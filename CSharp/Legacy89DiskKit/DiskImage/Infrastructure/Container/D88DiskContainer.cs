using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Exception;

namespace Legacy89DiskKit.DiskImage.Infrastructure.Container;

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

    public static D88DiskContainer CreateNew(string filePath, DiskType diskType, string diskName = "")
    {
        var container = new D88DiskContainer();
        container._filePath = filePath;
        container._isReadOnly = false;
        container.CreateEmptyImage(diskType, diskName);
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
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"Access denied to file: {_filePath}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new FileNotFoundException($"Directory not found: {Path.GetDirectoryName(_filePath)}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"I/O error reading file: {_filePath}", ex);
        }
        catch (OutOfMemoryException ex)
        {
            throw new InvalidOperationException($"File too large to load: {_filePath}", ex);
        }
        
        if (_imageData.Length < 0x2b0)
            throw new InvalidDiskFormatException($"Invalid D88 file: too small ({_imageData.Length} bytes, minimum 688)");

        ParseHeader();
        ParseSectors();
    }

    private void ParseHeader()
    {
        try
        {
            using var stream = new MemoryStream(_imageData);
            using var reader = new BinaryReader(stream);

            // ディスク名の読み取り（17バイト）
            var imageName = reader.ReadBytes(17);
            var diskName = System.Text.Encoding.ASCII.GetString(imageName).TrimEnd('\0');
            
            // 予約領域をスキップ（9バイト）
            reader.ReadBytes(9);
            
            // プロテクトフラグ
            var protect = reader.ReadByte();
            
            // メディアタイプの妥当性検証
            var mediaTypeByte = reader.ReadByte();
            if (!Enum.IsDefined(typeof(DiskType), mediaTypeByte))
                throw new InvalidDiskFormatException($"Invalid media type: 0x{mediaTypeByte:X2}");
            var mediaType = (DiskType)mediaTypeByte;
            
            // ディスクサイズの妥当性検証
            var diskSize = reader.ReadUInt32();
            if (diskSize != _imageData.Length)
                throw new InvalidDiskFormatException($"Disk size mismatch: header={diskSize}, file={_imageData.Length}");
            if (diskSize > 2 * 1024 * 1024) // 2MB制限
                throw new InvalidDiskFormatException($"Disk size too large: {diskSize} bytes");

            // トラックオフセット配列の読み取りと検証
            reader.BaseStream.Seek(0x20, SeekOrigin.Begin);
            var trackOffsets = new uint[164];
            for (int i = 0; i < 164; i++)
            {
                trackOffsets[i] = reader.ReadUInt32();
                
                // オフセットの妥当性検証
                if (trackOffsets[i] > 0)
                {
                    if (trackOffsets[i] >= _imageData.Length)
                        throw new InvalidDiskFormatException($"Invalid track {i} offset: {trackOffsets[i]} (file size: {_imageData.Length})");
                    if (trackOffsets[i] < 0x2b0)
                        throw new InvalidDiskFormatException($"Track {i} offset {trackOffsets[i]} overlaps header");
                }
            }
            
            // トラックオフセットの順序検証
            ValidateTrackOffsetOrder(trackOffsets);

            _header = new D88Header
            {
                ImageName = diskName,
                WriteProtect = protect != 0,
                MediaType = mediaType,
                DiskSize = diskSize,
                TrackOffsets = trackOffsets
            };
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidDiskFormatException("Unexpected end of D88 file while parsing header", ex);
        }
        catch (Exception ex) when (!(ex is InvalidDiskFormatException))
        {
            throw new InvalidDiskFormatException("Error parsing D88 header", ex);
        }
    }

    private static void ValidateTrackOffsetOrder(uint[] trackOffsets)
    {
        uint lastValidOffset = 0x2b0;
        for (int i = 0; i < trackOffsets.Length; i++)
        {
            if (trackOffsets[i] > 0)
            {
                if (trackOffsets[i] < lastValidOffset)
                    throw new InvalidDiskFormatException($"Track {i} offset {trackOffsets[i]} is out of order (previous: {lastValidOffset})");
                lastValidOffset = trackOffsets[i];
            }
        }
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
        try
        {
            using var stream = new MemoryStream(_imageData);
            using var reader = new BinaryReader(stream);
            
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            
            var sectorsInTrack = 0;
            var maxSectorsPerTrack = GetMaxSectorsPerTrack(_header.MediaType);
            
            while (reader.BaseStream.Position < _imageData.Length)
            {
                var sectorStart = reader.BaseStream.Position;
                
                // セクタヘッダの境界チェック（16バイト必要）
                if (reader.BaseStream.Position + 16 > _imageData.Length)
                    throw new InvalidDiskFormatException($"Track {trackIndex}: insufficient data for sector header at offset {sectorStart}");
                
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
                
                // セクタデータの妥当性検証
                ValidateSectorHeader(trackIndex, cylinder, head, sector, sectorSizeN, sectorCount, density, actualSize);
                
                // セクタデータの境界チェック
                if (reader.BaseStream.Position + actualSize > _imageData.Length)
                    throw new InvalidDiskFormatException($"Track {trackIndex}: insufficient data for sector C={cylinder}, H={head}, R={sector} (need {actualSize} bytes)");
                
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
                    Data = data,
                    FileOffset = (uint)sectorStart
                };
                
                _sectors[(cylinder, head, sector)] = d88Sector;
                sectorsInTrack++;
                
                // セクタ数の妥当性チェック
                if (sectorsInTrack > maxSectorsPerTrack)
                    throw new InvalidDiskFormatException($"Track {trackIndex}: too many sectors ({sectorsInTrack}, max: {maxSectorsPerTrack})");
                
                // 次のトラックの開始位置チェック
                if (trackIndex < 163 && _header.TrackOffsets[trackIndex + 1] > 0)
                {
                    if (reader.BaseStream.Position >= _header.TrackOffsets[trackIndex + 1])
                        break;
                }
            }
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidDiskFormatException($"Unexpected end of file while parsing track {trackIndex}", ex);
        }
        catch (Exception ex) when (!(ex is InvalidDiskFormatException))
        {
            throw new InvalidDiskFormatException($"Error parsing track {trackIndex}", ex);
        }
    }

    private static void ValidateSectorHeader(int trackIndex, byte cylinder, byte head, byte sector, 
        byte sectorSizeN, ushort sectorCount, byte density, ushort actualSize)
    {
        // シリンダー/ヘッドの妥当性
        if (cylinder > 82)
            throw new InvalidDiskFormatException($"Track {trackIndex}: invalid cylinder {cylinder} (max: 82)");
        if (head > 1)
            throw new InvalidDiskFormatException($"Track {trackIndex}: invalid head {head} (max: 1)");
        
        // セクタ番号の妥当性
        if (sector == 0 || sector > 26)
            throw new InvalidDiskFormatException($"Track {trackIndex}: invalid sector number {sector} (valid: 1-26)");
        
        // セクタサイズの妥当性
        if (sectorSizeN > 3)
            throw new InvalidDiskFormatException($"Track {trackIndex}: invalid sector size N={sectorSizeN} (max: 3)");
        
        var expectedSectorSize = 128 << sectorSizeN;
        if (actualSize > expectedSectorSize * 2) // 2倍までの余裕を持たせる
            throw new InvalidDiskFormatException($"Track {trackIndex}: actual size {actualSize} too large for sector size N={sectorSizeN} (expected: {expectedSectorSize})");
        
        // 密度の妥当性
        if (density != 0x00 && density != 0x01 && density != 0x40)
            throw new InvalidDiskFormatException($"Track {trackIndex}: invalid density 0x{density:X2}");
        
        // セクタ数の妥当性
        if (sectorCount == 0 || sectorCount > 26)
            throw new InvalidDiskFormatException($"Track {trackIndex}: invalid sector count {sectorCount}");
    }

    private static int GetMaxSectorsPerTrack(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => 16,
            DiskType.TwoDD => 16,
            DiskType.TwoHD => 26,
            _ => 26 // 安全側に倒す
        };
    }

    public byte[] ReadSector(int cylinder, int head, int sector)
    {
        return ReadSector(cylinder, head, sector, false);
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted)
    {
        if (!_sectors.TryGetValue((cylinder, head, sector), out var d88Sector))
            throw new SectorNotFoundException(cylinder, head, sector);
            
        if (d88Sector.Status != 0)
        {
            if (allowCorrupted)
            {
                // 警告を出力して破損データを返す
                Console.WriteLine($"Warning: Reading corrupted sector C={cylinder}, H={head}, R={sector}, Status=0x{d88Sector.Status:X2}");
                return d88Sector.Data ?? new byte[256]; // nullの場合は空データを返す
            }
            else
            {
                throw new DiskImageException($"Sector has error status: 0x{d88Sector.Status:X2} at C={cylinder}, H={head}, R={sector}");
            }
        }
            
        return d88Sector.Data;
    }

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        if (_isReadOnly)
            throw new ReadOnlyDiskException();
            
        if (!_sectors.TryGetValue((cylinder, head, sector), out var d88Sector))
            throw new SectorNotFoundException(cylinder, head, sector);
            
        d88Sector.Data = data;
        d88Sector.ActualSize = (ushort)data.Length;
        
        _hasChanges = true;
        BuildImageData();
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

    private void CreateEmptyImage(DiskType diskType, string diskName)
    {
        var (tracks, sectorsPerTrack, sectorSize) = GetDiskGeometry(diskType);
        
        _header = new D88Header
        {
            ImageName = diskName,
            WriteProtect = false,
            MediaType = diskType,
            DiskSize = 0,
            TrackOffsets = new uint[164]
        };
        
        CreateEmptySectors(tracks, sectorsPerTrack, sectorSize, diskType);
        BuildImageData();
    }

    private static (int tracks, int sectorsPerTrack, int sectorSize) GetDiskGeometry(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => (80, 16, 256),
            DiskType.TwoDD => (160, 16, 256),
            DiskType.TwoHD => (154, 26, 256),
            _ => throw new ArgumentException($"Unsupported disk type: {diskType}")
        };
    }

    private void CreateEmptySectors(int tracks, int sectorsPerTrack, int sectorSize, DiskType diskType)
    {
        _sectors.Clear();
        
        var density = diskType == DiskType.TwoHD ? (byte)0x01 : (byte)0x00;
        
        for (int track = 0; track < tracks; track++)
        {
            var cylinder = track / 2;
            var head = track % 2;
            
            for (int sectorNum = 1; sectorNum <= sectorsPerTrack; sectorNum++)
            {
                var data = new byte[sectorSize];
                Array.Fill(data, (byte)0xE5);
                
                var sector = new D88Sector
                {
                    Cylinder = (byte)cylinder,
                    Head = (byte)head,
                    Sector = (byte)sectorNum,
                    SectorSizeN = 1, // 256 bytes = 128 << 1
                    SectorCount = (ushort)sectorsPerTrack,
                    Density = density,
                    Deleted = false,
                    Status = 0,
                    ActualSize = (ushort)sectorSize,
                    Data = data
                };
                
                _sectors[(cylinder, head, sectorNum)] = sector;
            }
        }
    }

    private void BuildImageData()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        
        WriteHeader(writer);
        
        var trackOffsets = new uint[164];
        var currentOffset = 0x2b0u;
        
        for (int track = 0; track < 164; track++)
        {
            var trackSectors = GetTrackSectors(track);
            if (trackSectors.Any())
            {
                trackOffsets[track] = currentOffset;
                foreach (var sector in trackSectors)
                {
                    WriteSector(writer, sector);
                    currentOffset += 16u + sector.ActualSize;
                }
            }
        }
        
        _header.TrackOffsets = trackOffsets;
        _header.DiskSize = (uint)stream.Length;
        
        stream.Seek(0x20, SeekOrigin.Begin);
        for (int i = 0; i < 164; i++)
        {
            writer.Write(trackOffsets[i]);
        }
        
        stream.Seek(0x1c, SeekOrigin.Begin);
        writer.Write(_header.DiskSize);
        
        _imageData = stream.ToArray();
    }

    private void WriteHeader(BinaryWriter writer)
    {
        var nameBytes = new byte[17];
        var nameData = System.Text.Encoding.ASCII.GetBytes(_header.ImageName);
        Array.Copy(nameData, nameBytes, Math.Min(nameData.Length, 16));
        writer.Write(nameBytes);
        
        writer.Write(new byte[9]); // reserved
        writer.Write((byte)(_header.WriteProtect ? 0x10 : 0x00));
        writer.Write((byte)_header.MediaType);
        writer.Write(_header.DiskSize);
        
        for (int i = 0; i < 164; i++)
        {
            writer.Write(_header.TrackOffsets[i]);
        }
    }

    private void WriteSector(BinaryWriter writer, D88Sector sector)
    {
        writer.Write(sector.Cylinder);
        writer.Write(sector.Head);
        writer.Write(sector.Sector);
        writer.Write(sector.SectorSizeN);
        writer.Write(sector.SectorCount);
        writer.Write(sector.Density);
        writer.Write((byte)(sector.Deleted ? 0x10 : 0x00));
        writer.Write(sector.Status);
        writer.Write(new byte[5]); // reserved
        writer.Write(sector.ActualSize);
        writer.Write(sector.Data);
    }

    private IEnumerable<D88Sector> GetTrackSectors(int track)
    {
        var cylinder = track / 2;
        var head = track % 2;
        
        return _sectors.Values
            .Where(s => s.Cylinder == cylinder && s.Head == head)
            .OrderBy(s => s.Sector);
    }

    public void Save()
    {
        if (_isReadOnly)
            throw new ReadOnlyDiskException();
            
        BuildImageData();
        SaveToFile(_filePath);
    }

    public void SaveAs(string filePath)
    {
        BuildImageData();
        SaveToFile(filePath);
    }

    private void SaveToFile(string filePath)
    {
        try
        {
            // ディレクトリの存在確認
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Directory does not exist: {directory}");
            }

            // 一時ファイルに書き込んでからリネーム（原子的操作）
            var tempFilePath = filePath + ".tmp";
            File.WriteAllBytes(tempFilePath, _imageData);
            
            // 既存ファイルが存在する場合はバックアップ
            if (File.Exists(filePath))
            {
                File.Replace(tempFilePath, filePath, null);
            }
            else
            {
                File.Move(tempFilePath, filePath);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"Access denied writing to: {filePath}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new DirectoryNotFoundException($"Directory not found: {Path.GetDirectoryName(filePath)}", ex);
        }
        catch (IOException ex) when (ex.HResult == -2147024784) // HRESULT for disk full
        {
            throw new InvalidOperationException($"Insufficient disk space to write: {filePath}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"I/O error writing file: {filePath}", ex);
        }
        catch (Exception ex)
        {
            // 一時ファイルのクリーンアップ
            var tempFilePath = filePath + ".tmp";
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
            throw new InvalidOperationException($"Unexpected error writing file: {filePath}", ex);
        }
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
        public uint FileOffset { get; set; }
    }

    private void SaveToFile()
    {
        if (_isReadOnly)
            throw new InvalidOperationException("Cannot save to read-only disk image");
            
        try
        {
            File.WriteAllBytes(_filePath, _imageData);
            _hasChanges = false;
        }
        catch (Exception ex)
        {
            throw new DiskImageException($"Failed to save disk image to {_filePath}: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Save changes if any
                if (_hasChanges && !_isReadOnly && !string.IsNullOrEmpty(_filePath))
                {
                    try
                    {
                        SaveToFile();
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't throw in Dispose
                        Console.WriteLine($"Warning: Failed to save changes during dispose: {ex.Message}");
                    }
                }
            }
            _disposed = true;
        }
    }

    ~D88DiskContainer()
    {
        Dispose(false);
    }
}