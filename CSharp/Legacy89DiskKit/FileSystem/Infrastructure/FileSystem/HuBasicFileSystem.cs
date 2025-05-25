using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Exception;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

public class HuBasicFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly HuBasicConfiguration _config;
    
    // メモリ制限定数
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB制限
    private const int MaxClusterChainLength = 1000; // 最大1000クラスタ
    private const int MaxDirectoryEntries = 500; // 最大500エントリ

    public IDiskContainer DiskContainer => _diskContainer;
    public bool IsFormatted => CheckIfFormatted();

    public HuBasicFileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer;
        _config = GetConfiguration(diskContainer.DiskType);
    }

    private static HuBasicConfiguration GetConfiguration(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => new HuBasicConfiguration
            {
                TotalTracks = 80,
                SectorsPerTrack = 16,
                SectorSize = 256,
                ClusterSize = 16 * 256, // 16 sectors per cluster
                TotalClusters = 80,
                ReservedClusters = 2,
                FatTrack = 0,
                FatSector = 15,
                FatSectors = 1,
                DirectoryTrack = 1,
                DirectorySector = 1,
                DirectorySectors = 16
            },
            DiskType.TwoDD => new HuBasicConfiguration
            {
                TotalTracks = 160,
                SectorsPerTrack = 16,
                SectorSize = 256,
                ClusterSize = 16 * 256,
                TotalClusters = 160,
                ReservedClusters = 2,
                FatTrack = 0, // Position not specified in docs
                FatSector = 15, // Assumed same as 2D
                FatSectors = 2, // Assumed based on cluster count
                DirectoryTrack = 1, // Assumed same as 2D
                DirectorySector = 1,
                DirectorySectors = 16 // Assumed same as 2D
            },
            DiskType.TwoHD => new HuBasicConfiguration
            {
                TotalTracks = 154,
                SectorsPerTrack = 26,
                SectorSize = 256,
                ClusterSize = 26 * 256, // 26 sectors per cluster
                TotalClusters = 250,
                ReservedClusters = 3,
                FatTrack = 1,
                FatSector = 3, // Physical sector 29
                FatSectors = 2,
                DirectoryTrack = 1,
                DirectorySector = 7, // Physical sector 33
                DirectorySectors = 20 // Assumed based on entry count (247)
            },
            _ => throw new ArgumentException($"Unsupported disk type: {diskType}")
        };
    }

    private bool CheckIfFormatted()
    {
        try
        {
            var bootSector = ReadBootSector();
            var fatData = ReadFat();
            
            // 基本構造チェック
            if (bootSector == null || fatData == null || fatData.Length == 0)
                return false;
            
            // FATシグネチャチェック
            if (!ValidateFatSignature(fatData))
                return false;
                
            // ディレクトリ構造チェック
            if (!ValidateDirectoryStructure())
                return false;
                
            return true;
        }
        catch (SectorNotFoundException)
        {
            return false;
        }
        catch (DiskImageException)
        {
            return false;
        }
        catch (Exception)
        {
            // 予期しないエラーもfalseとして扱う
            return false;
        }
    }

    private bool ValidateFatSignature(byte[] fatData)
    {
        try
        {
            if (_diskContainer.DiskType == DiskType.TwoHD)
            {
                return fatData.Length >= 3 && fatData[0] == 0x01 && 
                       fatData[1] == 0x8F && fatData[2] == 0x8F;
            }
            else
            {
                return fatData.Length >= 2 && fatData[0] == 0x01 && fatData[1] == 0x8F;
            }
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateDirectoryStructure()
    {
        try
        {
            var dirData = ReadDirectorySector(0);
            
            // ディレクトリの最初のセクタが完全に0xFFまたは有効なエントリで構成されているかチェック
            for (int entryOffset = 0; entryOffset < _config.SectorSize; entryOffset += 32)
            {
                var fileMode = dirData[entryOffset];
                
                // 有効なファイルモード値かチェック
                if (fileMode != 0x00 && fileMode != 0xFF && (fileMode & 0xF0) > 0xF0)
                    return false;
                    
                if (fileMode == 0xFF) // ディレクトリ終端
                    break;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot format read-only disk");

        FormatBootSector();
        FormatFat();
        FormatDirectory();
    }

    private void FormatBootSector()
    {
        var bootData = new byte[32];
        
        bootData[0] = 0x00; // Not bootable
        
        var sysBytes = System.Text.Encoding.ASCII.GetBytes("Sys");
        Array.Copy(sysBytes, 0, bootData, 0x0E, Math.Min(sysBytes.Length, 3));
        for (int i = sysBytes.Length; i < 3; i++)
            bootData[0x0E + i] = 0x20; // Space padding
            
        bootData[0x11] = 0x20; // Password (space)
        
        var now = DateTime.Now;
        WriteBcdDate(bootData, 0x18, now);
        
        var fullBootData = new byte[_config.SectorSize];
        Array.Copy(bootData, fullBootData, 32);
        Array.Fill(fullBootData, (byte)0xE5, 32, _config.SectorSize - 32);
        
        _diskContainer.WriteSector(0, 0, 1, fullBootData);
    }

    private void FormatFat()
    {
        var fatData = new byte[_config.FatSectors * _config.SectorSize];
        
        if (_diskContainer.DiskType == DiskType.TwoHD)
        {
            // 2HD uses special FAT format
            fatData[0] = 0x01;
            fatData[1] = 0x8F;
            fatData[2] = 0x8F;
        }
        else
        {
            // 2D/2DD format
            fatData[0] = 0x01;
            fatData[1] = 0x8F;
        }
        
        for (int sector = 0; sector < _config.FatSectors; sector++)
        {
            var sectorData = new byte[_config.SectorSize];
            Array.Copy(fatData, sector * _config.SectorSize, sectorData, 0, _config.SectorSize);
            
            var physicalSector = _config.FatSector + sector;
            var cylinder = _config.FatTrack / 2;
            var head = _config.FatTrack % 2;
            
            if (_diskContainer.DiskType == DiskType.TwoHD)
            {
                // For 2HD, calculate actual C/H/R
                var totalSector = physicalSector - 1;
                cylinder = totalSector / _config.SectorsPerTrack / 2;
                head = (totalSector / _config.SectorsPerTrack) % 2;
                var sectorNum = (totalSector % _config.SectorsPerTrack) + 1;
                _diskContainer.WriteSector(cylinder, head, sectorNum, sectorData);
            }
            else
            {
                _diskContainer.WriteSector(cylinder, head, physicalSector, sectorData);
            }
        }
    }

    private void FormatDirectory()
    {
        var dirData = new byte[_config.SectorSize];
        Array.Fill(dirData, (byte)0xFF); // Mark all entries as end-of-directory
        
        for (int sector = 0; sector < _config.DirectorySectors; sector++)
        {
            var physicalSector = _config.DirectorySector + sector;
            var cylinder = _config.DirectoryTrack / 2;
            var head = _config.DirectoryTrack % 2;
            
            if (_diskContainer.DiskType == DiskType.TwoHD)
            {
                var totalSector = physicalSector - 1;
                cylinder = totalSector / _config.SectorsPerTrack / 2;
                head = (totalSector / _config.SectorsPerTrack) % 2;
                var sectorNum = (totalSector % _config.SectorsPerTrack) + 1;
                _diskContainer.WriteSector(cylinder, head, sectorNum, dirData);
            }
            else
            {
                _diskContainer.WriteSector(cylinder, head, physicalSector, dirData);
            }
        }
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        if (!IsFormatted)
            throw new FileSystemNotFormattedException();

        var files = new List<FileEntry>();
        var entriesProcessed = 0;
        
        for (int sector = 0; sector < _config.DirectorySectors; sector++)
        {
            byte[] dirData;
            try
            {
                dirData = ReadDirectorySector(sector);
            }
            catch (SectorNotFoundException)
            {
                // ディレクトリセクタが見つからない場合はスキップ
                continue;
            }
            
            for (int entryOffset = 0; entryOffset < _config.SectorSize; entryOffset += 32)
            {
                // エントリ数制限チェック
                if (entriesProcessed >= MaxDirectoryEntries)
                    throw new FileSystemException($"Too many directory entries (max: {MaxDirectoryEntries})");
                
                var entryData = new byte[32];
                Array.Copy(dirData, entryOffset, entryData, 0, 32);
                
                var fileMode = entryData[0];
                if (fileMode == 0xFF) // End of directory
                    return files;
                if (fileMode == 0x00) // Deleted
                {
                    entriesProcessed++;
                    continue;
                }
                    
                try
                {
                    var fileEntry = ParseDirectoryEntry(entryData);
                    if (fileEntry != null)
                    {
                        files.Add(fileEntry);
                        entriesProcessed++;
                    }
                }
                catch (Exception ex)
                {
                    // 破損したエントリはスキップして続行
                    Console.WriteLine($"Warning: Skipping corrupted directory entry at sector {sector}, offset {entryOffset}: {ex.Message}");
                    entriesProcessed++;
                    continue;
                }
            }
        }
        
        return files;
    }

    private byte[] ReadDirectorySector(int sectorIndex)
    {
        var physicalSector = _config.DirectorySector + sectorIndex;
        var cylinder = _config.DirectoryTrack / 2;
        var head = _config.DirectoryTrack % 2;
        
        if (_diskContainer.DiskType == DiskType.TwoHD)
        {
            var totalSector = physicalSector - 1;
            cylinder = totalSector / _config.SectorsPerTrack / 2;
            head = (totalSector / _config.SectorsPerTrack) % 2;
            var sectorNum = (totalSector % _config.SectorsPerTrack) + 1;
            return _diskContainer.ReadSector(cylinder, head, sectorNum);
        }
        else
        {
            return _diskContainer.ReadSector(cylinder, head, physicalSector);
        }
    }

    private FileEntry? ParseDirectoryEntry(byte[] entryData)
    {
        var fileMode = entryData[0];
        
        var fileName = System.Text.Encoding.ASCII.GetString(entryData, 1, 13).TrimEnd(' ');
        var extension = System.Text.Encoding.ASCII.GetString(entryData, 0x0E, 3).TrimEnd(' ');
        var password = entryData[0x11];
        var size = BitConverter.ToUInt16(entryData, 0x12);
        var loadAddress = BitConverter.ToUInt16(entryData, 0x14);
        var executeAddress = BitConverter.ToUInt16(entryData, 0x16);
        var modifiedDate = ParseBcdDate(entryData, 0x18);
        
        var attributes = new HuBasicFileAttributes(
            IsDirectory: (fileMode & 0x80) != 0,
            IsReadOnly: (fileMode & 0x40) != 0,
            IsVerify: (fileMode & 0x20) != 0,
            IsHidden: (fileMode & 0x10) != 0,
            IsBinary: (fileMode & 0x01) != 0,
            IsBasic: (fileMode & 0x02) != 0,
            IsAscii: (fileMode & 0x04) != 0 || (fileMode & 0x08) != 0
        );
        
        var mode = GetHuBasicFileMode(fileMode);
        
        return new FileEntry(
            fileName,
            extension,
            mode,
            attributes,
            size,
            loadAddress,
            executeAddress,
            modifiedDate,
            password != 0x20
        );
    }

    private static HuBasicFileMode GetHuBasicFileMode(byte fileMode)
    {
        if ((fileMode & 0x01) != 0) return HuBasicFileMode.Binary;
        if ((fileMode & 0x02) != 0) return HuBasicFileMode.Basic;
        if ((fileMode & 0x04) != 0 || (fileMode & 0x08) != 0) return HuBasicFileMode.Ascii;
        return HuBasicFileMode.Binary; // Default
    }

    public FileEntry? GetFile(string fileName)
    {
        return GetFiles().FirstOrDefault(f => 
            string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
    }

    public byte[] ReadFile(string fileName)
    {
        return ReadFile(fileName, false);
    }

    public byte[] ReadFile(string fileName, bool allowPartialRead)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            
        var fileEntry = GetFile(fileName);
        if (fileEntry == null)
            throw new Domain.Exception.FileNotFoundException(fileName);
        
        // ファイルサイズの事前チェック
        if (fileEntry.Size > MaxFileSize)
            throw new FileSystemException($"File too large: {fileEntry.Size} bytes (max: {MaxFileSize:N0})");
            
        var startCluster = GetFileStartCluster(fileName);
        if (startCluster < 0)
            throw new FileSystemException($"Invalid start cluster for file: {fileName}");
        
        List<int> clusters;
        try
        {
            clusters = GetClusterChain(startCluster);
        }
        catch (FileSystemException ex) when (allowPartialRead)
        {
            Console.WriteLine($"Warning: Cluster chain error for {fileName}: {ex.Message}");
            Console.WriteLine("Attempting partial recovery...");
            clusters = GetPartialClusterChain(startCluster);
        }
        
        // クラスタチェーン長のチェック
        if (clusters.Count > MaxClusterChainLength)
        {
            if (allowPartialRead)
            {
                Console.WriteLine($"Warning: Truncating long cluster chain for {fileName} (length: {clusters.Count})");
                clusters = clusters.Take(MaxClusterChainLength).ToList();
            }
            else
            {
                throw new FileSystemException($"File cluster chain too long: {clusters.Count} (max: {MaxClusterChainLength})");
            }
        }
        
        try
        {
            using var fileDataStream = new MemoryStream();
            long totalBytesRead = 0;
            int corruptedClusters = 0;
            
            foreach (var cluster in clusters)
            {
                try
                {
                    var clusterData = ReadCluster(cluster, allowPartialRead);
                    
                    // 累積サイズチェック
                    totalBytesRead += clusterData.Length;
                    if (totalBytesRead > MaxFileSize)
                    {
                        if (allowPartialRead)
                        {
                            Console.WriteLine($"Warning: File data truncated at {totalBytesRead:N0} bytes due to size limit");
                            break;
                        }
                        throw new OutOfMemoryException($"File data exceeds maximum supported size: {totalBytesRead:N0} bytes");
                    }
                    
                    fileDataStream.Write(clusterData);
                    
                    // メモリ使用量の定期チェック
                    if (fileDataStream.Length > MaxFileSize)
                    {
                        if (allowPartialRead)
                            break;
                        throw new OutOfMemoryException("File data stream exceeds maximum size");
                    }
                }
                catch (Exception ex) when (allowPartialRead && (ex is SectorNotFoundException || ex is DiskImageException))
                {
                    Console.WriteLine($"Warning: Skipping corrupted cluster {cluster} in file {fileName}: {ex.Message}");
                    corruptedClusters++;
                    
                    // 代替データ（ゼロまたは警告パターン）を挿入
                    var replacementData = new byte[_config.ClusterSize];
                    Array.Fill(replacementData, (byte)0x00); // ゼロで埋める
                    fileDataStream.Write(replacementData);
                    totalBytesRead += replacementData.Length;
                }
            }
            
            if (allowPartialRead && corruptedClusters > 0)
            {
                Console.WriteLine($"Warning: File {fileName} partially recovered. {corruptedClusters} corrupted clusters replaced with zeros.");
            }
            
            var data = fileDataStream.ToArray();
            
            if (fileEntry.Mode == HuBasicFileMode.Ascii)
            {
                return ExtractAsciiData(data);
            }
            else
            {
                // サイズフィールドに基づく切り取り（境界チェック付き）
                var actualSize = Math.Min(fileEntry.Size, data.Length);
                return data.Take(actualSize).ToArray();
            }
        }
        catch (OutOfMemoryException)
        {
            throw new FileSystemException($"Insufficient memory to read file: {fileName}");
        }
    }

    private byte[] ExtractAsciiData(byte[] data)
    {
        var result = new List<byte>();
        
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == 0x0D && data[i + 1] == 0x1A)
                break;
            result.Add(data[i]);
        }
        
        return result.ToArray();
    }

    private int GetFileStartCluster(string fileName)
    {
        // Find directory entry and extract start cluster
        for (int sector = 0; sector < _config.DirectorySectors; sector++)
        {
            var dirData = ReadDirectorySector(sector);
            
            for (int entryOffset = 0; entryOffset < _config.SectorSize; entryOffset += 32)
            {
                var entryData = new byte[32];
                Array.Copy(dirData, entryOffset, entryData, 0, 32);
                
                var fileMode = entryData[0];
                if (fileMode == 0xFF) return -1;
                if (fileMode == 0x00) continue;
                
                var entryFileName = System.Text.Encoding.ASCII.GetString(entryData, 1, 13).TrimEnd(' ');
                if (string.Equals(entryFileName, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    var low = entryData[0x1D];
                    var middle = entryData[0x1E];
                    var high = entryData[0x1F];
                    
                    return (middle << 7) | (low & 0x7F);
                }
            }
        }
        
        return -1;
    }

    private List<int> GetClusterChain(int startCluster)
    {
        var chain = new List<int>();
        var visited = new HashSet<int>();
        var current = startCluster;
        var fatData = ReadFat();
        
        // 開始クラスタの妥当性チェック
        if (startCluster < _config.ReservedClusters || startCluster >= _config.TotalClusters)
            throw new FileSystemException($"Invalid start cluster: {startCluster} (valid range: {_config.ReservedClusters}-{_config.TotalClusters - 1})");
        
        while (current > 0 && current < _config.TotalClusters)
        {
            // 循環参照検出
            if (visited.Contains(current))
                throw new FileSystemException($"Circular reference detected in cluster chain at cluster {current}");
                
            visited.Add(current);
            chain.Add(current);
            
            var nextCluster = GetFatEntry(fatData, current);
            
            // ファイル終端チェック
            if (nextCluster >= 0x80 && nextCluster <= 0x8F)
                break;
            
            // 無効なクラスタ番号チェック
            if (nextCluster > 0 && (nextCluster < _config.ReservedClusters || nextCluster >= _config.TotalClusters))
                throw new FileSystemException($"Invalid cluster number in chain: {nextCluster} at cluster {current}");
                
            current = nextCluster;
            
            // チェーン長の上限チェック（無限ループ防止）
            if (chain.Count > _config.TotalClusters)
                throw new FileSystemException($"Cluster chain too long - possible corruption (max: {_config.TotalClusters}, current: {chain.Count})");
        }
        
        return chain;
    }

    private List<int> GetPartialClusterChain(int startCluster)
    {
        var chain = new List<int>();
        var visited = new HashSet<int>();
        var current = startCluster;
        var fatData = ReadFat();
        
        // 開始クラスタの妥当性チェック（緩和版）
        if (startCluster < 0 || startCluster >= _config.TotalClusters)
        {
            Console.WriteLine($"Warning: Invalid start cluster {startCluster}, attempting single cluster read");
            return new List<int> { Math.Max(startCluster, _config.ReservedClusters) };
        }
        
        while (current > 0 && current < _config.TotalClusters && chain.Count < MaxClusterChainLength)
        {
            // 循環参照検出（破損時は警告して終了）
            if (visited.Contains(current))
            {
                Console.WriteLine($"Warning: Circular reference detected at cluster {current}, stopping chain traversal");
                break;
            }
                
            visited.Add(current);
            chain.Add(current);
            
            try
            {
                var nextCluster = GetFatEntry(fatData, current);
                
                // ファイル終端チェック
                if (nextCluster >= 0x80 && nextCluster <= 0x8F)
                    break;
                
                // 無効なクラスタ番号の場合は警告して終了
                if (nextCluster > 0 && (nextCluster < _config.ReservedClusters || nextCluster >= _config.TotalClusters))
                {
                    Console.WriteLine($"Warning: Invalid next cluster {nextCluster} at cluster {current}, stopping chain traversal");
                    break;
                }
                    
                current = nextCluster;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error reading FAT entry for cluster {current}: {ex.Message}");
                break;
            }
        }
        
        if (chain.Count == 0)
        {
            Console.WriteLine($"Warning: No valid clusters found, returning start cluster {startCluster}");
            chain.Add(startCluster);
        }
        
        return chain;
    }

    private byte[] ReadFat()
    {
        var fatData = new byte[_config.FatSectors * _config.SectorSize];
        
        for (int sector = 0; sector < _config.FatSectors; sector++)
        {
            var physicalSector = _config.FatSector + sector;
            var cylinder = _config.FatTrack / 2;
            var head = _config.FatTrack % 2;
            
            byte[] sectorData;
            if (_diskContainer.DiskType == DiskType.TwoHD)
            {
                var totalSector = physicalSector - 1;
                cylinder = totalSector / _config.SectorsPerTrack / 2;
                head = (totalSector / _config.SectorsPerTrack) % 2;
                var sectorNum = (totalSector % _config.SectorsPerTrack) + 1;
                sectorData = _diskContainer.ReadSector(cylinder, head, sectorNum);
            }
            else
            {
                sectorData = _diskContainer.ReadSector(cylinder, head, physicalSector);
            }
            
            Array.Copy(sectorData, 0, fatData, sector * _config.SectorSize, _config.SectorSize);
        }
        
        return fatData;
    }

    private int GetFatEntry(byte[] fatData, int cluster)
    {
        if (_diskContainer.DiskType == DiskType.TwoHD)
        {
            // 2HD uses special FAT encoding
            if (cluster >= 128)
            {
                var sectorOffset = cluster - 128;
                var lowBit = fatData[0x100 + sectorOffset] & 0x7F;
                var highBit = fatData[0x180 + sectorOffset];
                return (highBit << 7) | lowBit;
            }
            else
            {
                var lowBit = fatData[cluster] & 0x7F;
                var highBit = fatData[0x80 + cluster];
                return (highBit << 7) | lowBit;
            }
        }
        else
        {
            return fatData[cluster];
        }
    }

    private byte[] ReadCluster(int cluster)
    {
        return ReadCluster(cluster, false);
    }

    private byte[] ReadCluster(int cluster, bool allowCorrupted)
    {
        var (cylinder, head) = GetClusterPosition(cluster);
        var clusterData = new byte[_config.ClusterSize];
        var sectorsInCluster = _config.ClusterSize / _config.SectorSize;
        
        for (int sector = 0; sector < sectorsInCluster; sector++)
        {
            try
            {
                var sectorData = _diskContainer.ReadSector(cylinder, head, sector + 1, allowCorrupted);
                Array.Copy(sectorData, 0, clusterData, sector * _config.SectorSize, Math.Min(sectorData.Length, _config.SectorSize));
            }
            catch (SectorNotFoundException ex) when (allowCorrupted)
            {
                Console.WriteLine($"Warning: Sector not found in cluster {cluster}: {ex.Message}");
                // 欠損セクタは特定のパターンで埋める
                var replacementPattern = new byte[_config.SectorSize];
                Array.Fill(replacementPattern, (byte)0xE5); // 通常のフォーマット時のパターン
                Array.Copy(replacementPattern, 0, clusterData, sector * _config.SectorSize, _config.SectorSize);
            }
            catch (DiskImageException ex) when (allowCorrupted)
            {
                Console.WriteLine($"Warning: Disk error reading sector in cluster {cluster}: {ex.Message}");
                // エラーセクタはゼロで埋める
                var zeroPattern = new byte[_config.SectorSize];
                Array.Copy(zeroPattern, 0, clusterData, sector * _config.SectorSize, _config.SectorSize);
            }
        }
        
        return clusterData;
    }

    private (int cylinder, int head) GetClusterPosition(int cluster)
    {
        if (_diskContainer.DiskType == DiskType.TwoD)
        {
            return (cluster / 2, cluster % 2);
        }
        else
        {
            // For 2DD and 2HD, need more complex mapping
            var track = cluster;
            return (track / 2, track % 2);
        }
    }

    public void WriteFile(string fileName, byte[] data, HuBasicFileAttributes attributes)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot write to read-only disk");
            
        if (GetFile(fileName) != null)
            throw new FileAlreadyExistsException(fileName);
            
        // This is a simplified implementation
        // Full implementation would need to allocate clusters, update FAT, etc.
        throw new NotImplementedException("WriteFile not yet implemented");
    }

    public void WriteFile(string fileName, byte[] data, bool isText = false, ushort loadAddress = 0, ushort execAddress = 0)
    {
        var attributes = new HuBasicFileAttributes(
            IsDirectory: false,
            IsReadOnly: false,
            IsVerify: false,
            IsHidden: false,
            IsBinary: !isText,
            IsBasic: false,
            IsAscii: isText);
            
        WriteFile(fileName, data, attributes);
    }

    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot delete from read-only disk");
            
        // Mark directory entry as deleted and free clusters in FAT
        throw new NotImplementedException("DeleteFile not yet implemented");
    }

    public BootSector GetBootSector()
    {
        var bootData = ReadBootSector();
        
        var isBootable = bootData[0] == 0x01;
        var label = System.Text.Encoding.ASCII.GetString(bootData, 1, 13).TrimEnd(' ');
        var extension = System.Text.Encoding.ASCII.GetString(bootData, 0x0E, 3).TrimEnd(' ');
        var size = BitConverter.ToUInt16(bootData, 0x12);
        var loadAddress = BitConverter.ToUInt16(bootData, 0x14);
        var executeAddress = BitConverter.ToUInt16(bootData, 0x16);
        var modifiedDate = ParseBcdDate(bootData, 0x18);
        var startSector = BitConverter.ToUInt16(bootData, 0x1E);
        
        return new BootSector(isBootable, label, extension, size, loadAddress, executeAddress, modifiedDate, startSector);
    }

    private byte[] ReadBootSector()
    {
        return _diskContainer.ReadSector(0, 0, 1);
    }

    public void WriteBootSector(BootSector bootSector)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot write to read-only disk");
            
        var bootData = new byte[32];
        
        bootData[0] = (byte)(bootSector.IsBootable ? 0x01 : 0x00);
        
        var labelBytes = System.Text.Encoding.ASCII.GetBytes(bootSector.Label);
        Array.Copy(labelBytes, 0, bootData, 1, Math.Min(labelBytes.Length, 13));
        for (int i = labelBytes.Length; i < 13; i++)
            bootData[1 + i] = 0x20;
            
        var extBytes = System.Text.Encoding.ASCII.GetBytes(bootSector.Extension);
        Array.Copy(extBytes, 0, bootData, 0x0E, Math.Min(extBytes.Length, 3));
        for (int i = extBytes.Length; i < 3; i++)
            bootData[0x0E + i] = 0x20;
            
        bootData[0x11] = 0x20; // Password
        
        BitConverter.GetBytes(bootSector.Size).CopyTo(bootData, 0x12);
        BitConverter.GetBytes(bootSector.LoadAddress).CopyTo(bootData, 0x14);
        BitConverter.GetBytes(bootSector.ExecuteAddress).CopyTo(bootData, 0x16);
        
        WriteBcdDate(bootData, 0x18, bootSector.ModifiedDate);
        
        bootData[0x1D] = 0x00; // Reserved
        BitConverter.GetBytes(bootSector.StartSector).CopyTo(bootData, 0x1E);
        
        var fullBootData = new byte[_config.SectorSize];
        Array.Copy(bootData, fullBootData, 32);
        Array.Fill(fullBootData, (byte)0xE5, 32, _config.SectorSize - 32);
        
        _diskContainer.WriteSector(0, 0, 1, fullBootData);
    }

    public HuBasicFileSystemInfo GetFileSystemInfo()
    {
        var fatData = ReadFat();
        var freeClusters = 0;
        
        for (int i = _config.ReservedClusters; i < _config.TotalClusters; i++)
        {
            if (GetFatEntry(fatData, i) == 0x00)
                freeClusters++;
        }
        
        return new HuBasicFileSystemInfo(
            _config.TotalClusters - _config.ReservedClusters,
            freeClusters,
            _config.ClusterSize,
            _config.SectorSize
        );
    }

    private static DateTime ParseBcdDate(byte[] data, int offset)
    {
        var year = BcdToByte(data[offset]);
        var monthDay = data[offset + 1];
        var month = (monthDay >> 4) & 0x0F;
        var day = BcdToByte(data[offset + 2]);
        var hour = BcdToByte(data[offset + 3]);
        var minute = BcdToByte(data[offset + 4]);
        var second = BcdToByte(data[offset + 5]);
        
        var fullYear = year < 80 ? 2000 + year : 1900 + year;
        
        try
        {
            return new DateTime(fullYear, month, day, hour, minute, second);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private static void WriteBcdDate(byte[] data, int offset, DateTime date)
    {
        data[offset] = ByteToBcd(date.Year % 100);
        data[offset + 1] = (byte)((date.Month << 4) | (int)date.DayOfWeek);
        data[offset + 2] = ByteToBcd(date.Day);
        data[offset + 3] = ByteToBcd(date.Hour);
        data[offset + 4] = ByteToBcd(date.Minute);
        data[offset + 5] = ByteToBcd(date.Second);
    }

    private static byte BcdToByte(byte bcd)
    {
        return (byte)((bcd >> 4) * 10 + (bcd & 0x0F));
    }

    private static byte ByteToBcd(int value)
    {
        return (byte)(((value / 10) << 4) | (value % 10));
    }

    private class HuBasicConfiguration
    {
        public int TotalTracks { get; set; }
        public int SectorsPerTrack { get; set; }
        public int SectorSize { get; set; }
        public int ClusterSize { get; set; }
        public int TotalClusters { get; set; }
        public int ReservedClusters { get; set; }
        public int FatTrack { get; set; }
        public int FatSector { get; set; }
        public int FatSectors { get; set; }
        public int DirectoryTrack { get; set; }
        public int DirectorySector { get; set; }
        public int DirectorySectors { get; set; }
    }
}