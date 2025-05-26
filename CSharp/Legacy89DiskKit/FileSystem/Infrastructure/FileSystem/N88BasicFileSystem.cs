using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Exception;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

/// <summary>
/// PC-8801 N88-BASIC ファイルシステム実装
/// </summary>
public class N88BasicFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly N88BasicConfiguration _config;
    
    // メモリ制限定数
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB制限
    private const int MaxClusterChainLength = 1000; // 最大1000クラスタ
    private const int MaxDirectoryEntries = 500; // 最大500エントリ
    
    public IDiskContainer DiskContainer => _diskContainer;
    public bool IsFormatted => CheckIfFormatted();
    
    public N88BasicFileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer ?? throw new ArgumentNullException(nameof(diskContainer));
        _config = N88BasicConfiguration.Create(diskContainer.DiskType);
    }
    
    #region フォーマット状態チェック
    
    /// <summary>
    /// ディスクがN88-BASICでフォーマットされているかを確認
    /// </summary>
    private bool CheckIfFormatted()
    {
        try
        {
            // システムトラック存在チェック
            if (!CheckSystemTrackExists())
                return false;
            
            // FAT構造チェック
            if (!ValidateFatStructure())
                return false;
            
            // ディレクトリ構造チェック
            if (!ValidateDirectoryStructure())
                return false;
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// システムトラックの存在を確認
    /// </summary>
    private bool CheckSystemTrackExists()
    {
        try
        {
            var (track, head) = _config.GetSystemLocation();
            
            // ディレクトリ領域の最初のセクタを読み取り試行
            var (dirTrack, dirHead, dirSector) = _config.GetDirectorySectorAddress(0);
            var sectorData = _diskContainer.ReadSector(dirTrack, dirHead, dirSector);
            
            return sectorData != null && sectorData.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// FAT構造の妥当性を検証
    /// </summary>
    private bool ValidateFatStructure()
    {
        try
        {
            var fatData = ReadFatData();
            if (fatData == null || fatData.Length == 0)
                return false;
            
            // 基本的なFAT値の存在確認
            var hasValidEntries = false;
            var hasEofMarkers = false;
            
            foreach (var fatValue in fatData)
            {
                if (_config.IsFreeCluster(fatValue))
                    continue;
                
                if (_config.IsEofMarker(fatValue))
                {
                    hasEofMarkers = true;
                    continue;
                }
                
                if (_config.IsValidClusterNumber(fatValue))
                {
                    hasValidEntries = true;
                }
            }
            
            // 少なくとも空きクラスタまたは使用中クラスタが存在すること
            return true; // 基本構造が読めれば有効とみなす
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// ディレクトリ構造の妥当性を検証
    /// </summary>
    private bool ValidateDirectoryStructure()
    {
        try
        {
            var entries = ReadDirectoryEntries();
            
            // 最低限ディレクトリエントリが読み取れることを確認
            return entries != null;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion
    
    #region 低レベルディスクアクセス
    
    /// <summary>
    /// FATデータ全体を読み取り
    /// </summary>
    private byte[]? ReadFatData()
    {
        try
        {
            var allFatData = new List<byte>();
            
            // 全FATセクタを読み取り
            for (int i = 0; i < _config.FatSectorCount; i++)
            {
                var (track, head, sector) = _config.GetFatSectorAddress(i);
                var sectorData = _diskContainer.ReadSector(track, head, sector);
                
                if (sectorData == null)
                    return null;
                
                allFatData.AddRange(sectorData);
            }
            
            return allFatData.ToArray();
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"FAT読み取りエラー: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// ディレクトリエントリ全体を読み取り
    /// </summary>
    private List<N88BasicFileEntry> ReadDirectoryEntries()
    {
        try
        {
            var entries = new List<N88BasicFileEntry>();
            
            // 全ディレクトリセクタを読み取り
            for (int i = 0; i < _config.DirectorySectorCount; i++)
            {
                var (track, head, sector) = _config.GetDirectorySectorAddress(i);
                var sectorData = _diskContainer.ReadSector(track, head, sector);
                
                if (sectorData == null)
                    continue;
                
                // セクタ内の各エントリを解析
                for (int offset = 0; offset < sectorData.Length; offset += _config.EntrySize)
                {
                    if (offset + _config.EntrySize > sectorData.Length)
                        break;
                    
                    var entryBytes = new byte[_config.EntrySize];
                    Array.Copy(sectorData, offset, entryBytes, 0, _config.EntrySize);
                    
                    var entry = N88BasicFileEntry.FromBytes(entryBytes);
                    entries.Add(entry);
                }
            }
            
            return entries;
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"ディレクトリ読み取りエラー: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 指定クラスタのデータを読み取り
    /// </summary>
    private byte[] ReadClusterData(int clusterNumber)
    {
        try
        {
            var clusterData = new List<byte>();
            
            // クラスタ内の全セクタを読み取り
            for (int sectorOffset = 0; sectorOffset < _config.SectorsPerCluster; sectorOffset++)
            {
                var (track, head, sector) = _config.ClusterToPhysicalSector(clusterNumber, sectorOffset);
                var sectorData = _diskContainer.ReadSector(track, head, sector);
                
                if (sectorData != null)
                {
                    clusterData.AddRange(sectorData);
                }
            }
            
            return clusterData.ToArray();
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"クラスタ{clusterNumber}読み取りエラー: {ex.Message}", ex);
        }
    }
    
    #endregion
    
    #region IFileSystem実装
    
    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new FileSystemException("読み取り専用ディスクはフォーマットできません");
        
        try
        {
            // ディレクトリ領域を初期化
            InitializeDirectoryArea();
            
            // FAT領域を初期化
            InitializeFatArea();
            
            // IDセクタを初期化
            InitializeIdSector();
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"N88-BASICフォーマットエラー: {ex.Message}", ex);
        }
    }
    
    public List<FileEntry> ListFiles()
    {
        try
        {
            var entries = ReadDirectoryEntries();
            var fileEntries = new List<FileEntry>();
            
            foreach (var entry in entries)
            {
                if (!entry.IsValid)
                    continue;
                
                // FATチェーンからファイルサイズを計算
                var size = CalculateFileSize(entry.StartCluster);
                entry.CalculatedSize = size;
                
                // FileEntryに変換
                var fileEntry = new FileEntry
                {
                    FileName = entry.FileName,
                    Extension = entry.Extension,
                    Size = size,
                    IsDirectory = false,
                    Attributes = entry.Attributes,
                    CreatedDate = DateTime.MinValue, // N88-BASICは日付なし
                    ModifiedDate = DateTime.MinValue
                };
                
                fileEntries.Add(fileEntry);
            }
            
            return fileEntries;
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"ファイル一覧取得エラー: {ex.Message}", ex);
        }
    }
    
    public byte[] ReadFile(string fileName, bool allowPartialRead = false)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("ファイル名が指定されていません");
        
        try
        {
            var entry = FindFileEntry(fileName);
            if (entry == null)
                throw new FileSystemException($"ファイルが見つかりません: {fileName}");
            
            return ReadFileData(entry, allowPartialRead);
        }
        catch (Exception ex) when (!(ex is FileSystemException))
        {
            throw new FileSystemException($"ファイル読み取りエラー: {fileName} - {ex.Message}", ex);
        }
    }
    
    public void WriteFile(string fileName, byte[] data)
    {
        if (_diskContainer.IsReadOnly)
            throw new FileSystemException("読み取り専用ディスクには書き込みできません");
        
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("ファイル名が指定されていません");
        
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        
        if (data.Length > MaxFileSize)
            throw new FileSystemException($"ファイルサイズが制限を超えています: {data.Length} > {MaxFileSize}");
        
        // ファイル名検証
        var validation = N88BasicFileNameValidator.ValidateFileName(fileName);
        if (!validation.IsValid)
            throw new FileSystemException($"不正なファイル名: {validation.ErrorMessage}");
        
        try
        {
            WriteFileData(validation.BaseName, validation.Extension, data);
        }
        catch (Exception ex) when (!(ex is FileSystemException))
        {
            throw new FileSystemException($"ファイル書き込みエラー: {fileName} - {ex.Message}", ex);
        }
    }
    
    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new FileSystemException("読み取り専用ディスクからは削除できません");
        
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("ファイル名が指定されていません");
        
        try
        {
            var entry = FindFileEntry(fileName);
            if (entry == null)
                throw new FileSystemException($"ファイルが見つかりません: {fileName}");
            
            DeleteFileData(entry);
        }
        catch (Exception ex) when (!(ex is FileSystemException))
        {
            throw new FileSystemException($"ファイル削除エラー: {fileName} - {ex.Message}", ex);
        }
    }
    
    public FileEntry? GetFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;
        
        try
        {
            var entry = FindFileEntry(fileName);
            if (entry == null)
                return null;
            
            var size = CalculateFileSize(entry.StartCluster);
            
            return new FileEntry
            {
                FileName = entry.FileName,
                Extension = entry.Extension,
                Size = size,
                IsDirectory = false,
                Attributes = entry.Attributes,
                CreatedDate = DateTime.MinValue,
                ModifiedDate = DateTime.MinValue
            };
        }
        catch
        {
            return null;
        }
    }
    
    #endregion
    
    #region ヘルパーメソッド
    
    /// <summary>
    /// ファイル名でディレクトリエントリを検索
    /// </summary>
    private N88BasicFileEntry? FindFileEntry(string fileName)
    {
        // ファイル名を正規化
        var validation = N88BasicFileNameValidator.ValidateFileName(fileName);
        if (!validation.IsValid)
            return null;
        
        var entries = ReadDirectoryEntries();
        
        return entries.FirstOrDefault(entry => 
            entry.IsValid && 
            string.Equals(entry.FileName, validation.BaseName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(entry.Extension, validation.Extension, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// FATチェーンを追跡してファイルサイズを計算
    /// </summary>
    private int CalculateFileSize(byte startCluster)
    {
        try
        {
            var fatData = ReadFatData();
            if (fatData == null)
                return 0;
            
            var totalSectors = 0;
            var currentCluster = (int)startCluster;
            var visitedClusters = new HashSet<int>();
            
            while (currentCluster < fatData.Length && 
                   !_config.IsEofMarker(fatData[currentCluster]) &&
                   visitedClusters.Count < MaxClusterChainLength)
            {
                if (visitedClusters.Contains(currentCluster))
                {
                    // 循環参照検出
                    throw new FileSystemException($"FATチェーンで循環参照を検出: クラスタ{currentCluster}");
                }
                
                visitedClusters.Add(currentCluster);
                
                if (_config.IsEofMarker(fatData[currentCluster]))
                {
                    // 終端マーカーから使用セクタ数を取得
                    var eofValue = fatData[currentCluster];
                    var usedSectors = eofValue & 0x3F; // 下位6ビットが使用セクタ数
                    totalSectors += usedSectors;
                    break;
                }
                
                // 通常クラスタの場合、全セクタを使用
                totalSectors += _config.SectorsPerCluster;
                currentCluster = fatData[currentCluster];
            }
            
            return totalSectors * _config.SectorSize;
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"ファイルサイズ計算エラー: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// ファイルデータを読み取り
    /// </summary>
    private byte[] ReadFileData(N88BasicFileEntry entry, bool allowPartialRead)
    {
        try
        {
            var fatData = ReadFatData();
            if (fatData == null)
                throw new FileSystemException("FAT読み取りに失敗しました");
            
            var fileData = new List<byte>();
            var currentCluster = (int)entry.StartCluster;
            var visitedClusters = new HashSet<int>();
            
            while (currentCluster < fatData.Length && 
                   visitedClusters.Count < MaxClusterChainLength)
            {
                if (visitedClusters.Contains(currentCluster))
                {
                    if (allowPartialRead)
                        break;
                    throw new FileSystemException($"FATチェーンで循環参照を検出: クラスタ{currentCluster}");
                }
                
                visitedClusters.Add(currentCluster);
                
                try
                {
                    var clusterData = ReadClusterData(currentCluster);
                    
                    if (_config.IsEofMarker(fatData[currentCluster]))
                    {
                        // 最終クラスタ：使用セクタ数分のみ追加
                        var eofValue = fatData[currentCluster];
                        var usedSectors = eofValue & 0x3F;
                        var usedBytes = usedSectors * _config.SectorSize;
                        
                        if (usedBytes <= clusterData.Length)
                        {
                            fileData.AddRange(clusterData.Take(usedBytes));
                        }
                        else
                        {
                            fileData.AddRange(clusterData);
                        }
                        break;
                    }
                    
                    // 通常クラスタ：全データを追加
                    fileData.AddRange(clusterData);
                    currentCluster = fatData[currentCluster];
                }
                catch (Exception ex)
                {
                    if (allowPartialRead)
                        break;
                    throw new FileSystemException($"クラスタ{currentCluster}読み取りエラー: {ex.Message}", ex);
                }
            }
            
            if (fileData.Count > MaxFileSize)
            {
                if (allowPartialRead)
                    return fileData.Take(MaxFileSize).ToArray();
                throw new FileSystemException($"ファイルサイズが制限を超えています: {fileData.Count}");
            }
            
            return fileData.ToArray();
        }
        catch (Exception ex) when (!(ex is FileSystemException))
        {
            throw new FileSystemException($"ファイルデータ読み取りエラー: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// ファイルデータを書き込み
    /// </summary>
    private void WriteFileData(string baseName, string extension, byte[] data)
    {
        // 既存ファイルチェック
        var existingEntry = FindFileEntry($"{baseName}.{extension}");
        if (existingEntry != null)
        {
            // 既存ファイルを削除
            DeleteFileData(existingEntry);
        }
        
        // 必要クラスタ数を計算
        var requiredClusters = (data.Length + _config.ClusterSize - 1) / _config.ClusterSize;
        if (requiredClusters == 0) requiredClusters = 1;
        
        // 空きクラスタを検索・確保
        var allocatedClusters = AllocateClusters(requiredClusters);
        if (allocatedClusters.Count == 0)
            throw new FileSystemException("空きクラスタが不足しています");
        
        try
        {
            // データをクラスタに書き込み
            WriteDataToClusters(allocatedClusters, data);
            
            // FATチェーンを構築
            BuildFatChain(allocatedClusters, data.Length);
            
            // ディレクトリエントリを作成
            CreateDirectoryEntry(baseName, extension, allocatedClusters[0]);
        }
        catch
        {
            // エラー時はクラスタを解放
            FreeClusters(allocatedClusters);
            throw;
        }
    }
    
    /// <summary>
    /// ファイルを削除
    /// </summary>
    private void DeleteFileData(N88BasicFileEntry entry)
    {
        // FATチェーンを追跡してクラスタを解放
        var clustersToFree = GetFileClusterChain(entry.StartCluster);
        FreeClusters(clustersToFree);
        
        // ディレクトリエントリを削除マーク
        MarkDirectoryEntryAsDeleted(entry);
    }
    
    /// <summary>
    /// 空きクラスタを確保
    /// </summary>
    private List<int> AllocateClusters(int count)
    {
        var fatData = ReadFatData();
        if (fatData == null)
            throw new FileSystemException("FAT読み取りに失敗しました");
        
        var allocatedClusters = new List<int>();
        
        for (int cluster = 0; cluster < fatData.Length && allocatedClusters.Count < count; cluster++)
        {
            if (_config.IsFreeCluster(fatData[cluster]))
            {
                allocatedClusters.Add(cluster);
            }
        }
        
        if (allocatedClusters.Count < count)
            throw new FileSystemException($"空きクラスタが不足しています (必要: {count}, 利用可能: {allocatedClusters.Count})");
        
        return allocatedClusters;
    }
    
    /// <summary>
    /// ファイルのクラスタチェーンを取得
    /// </summary>
    private List<int> GetFileClusterChain(byte startCluster)
    {
        var fatData = ReadFatData();
        if (fatData == null)
            return new List<int>();
        
        var clusters = new List<int>();
        var currentCluster = (int)startCluster;
        var visitedClusters = new HashSet<int>();
        
        while (currentCluster < fatData.Length && 
               visitedClusters.Count < MaxClusterChainLength)
        {
            if (visitedClusters.Contains(currentCluster))
                break; // 循環参照
            
            clusters.Add(currentCluster);
            visitedClusters.Add(currentCluster);
            
            if (_config.IsEofMarker(fatData[currentCluster]))
                break;
            
            currentCluster = fatData[currentCluster];
        }
        
        return clusters;
    }
    
    #endregion
    
    #region 初期化・フォーマット
    
    /// <summary>
    /// ディレクトリ領域を初期化
    /// </summary>
    private void InitializeDirectoryArea()
    {
        var emptyData = new byte[_config.SectorSize];
        Array.Fill(emptyData, (byte)0xFF); // 未使用マーカーで埋める
        
        for (int i = 0; i < _config.DirectorySectorCount; i++)
        {
            var (track, head, sector) = _config.GetDirectorySectorAddress(i);
            _diskContainer.WriteSector(track, head, sector, emptyData);
        }
    }
    
    /// <summary>
    /// FAT領域を初期化
    /// </summary>
    private void InitializeFatArea()
    {
        var fatData = new byte[_config.SectorSize];
        Array.Fill(fatData, _config.FatFreeMarker); // 空きマーカーで埋める
        
        for (int i = 0; i < _config.FatSectorCount; i++)
        {
            var (track, head, sector) = _config.GetFatSectorAddress(i);
            _diskContainer.WriteSector(track, head, sector, fatData);
        }
    }
    
    /// <summary>
    /// IDセクタを初期化
    /// </summary>
    private void InitializeIdSector()
    {
        var idData = new byte[_config.SectorSize];
        
        // IDセクタの基本構造を設定
        idData[0] = (byte)_config.DiskType; // ディスクメディア属性
        idData[1] = 0xFF; // プロンプトスキップ
        
        // バイト2以降はBASICコード領域として空のまま
        
        var (track, head, sector) = _config.GetIdSectorAddress();
        _diskContainer.WriteSector(track, head, sector, idData);
    }
    
    #endregion
    
    #region 低レベル書き込み操作
    
    /// <summary>
    /// データをクラスタに書き込み
    /// </summary>
    private void WriteDataToClusters(List<int> clusters, byte[] data)
    {
        int dataOffset = 0;
        
        foreach (var cluster in clusters)
        {
            var clusterData = new byte[_config.ClusterSize];
            var remainingData = data.Length - dataOffset;
            var bytesToWrite = Math.Min(remainingData, _config.ClusterSize);
            
            if (bytesToWrite > 0)
            {
                Array.Copy(data, dataOffset, clusterData, 0, bytesToWrite);
                dataOffset += bytesToWrite;
            }
            
            // クラスタの各セクタに書き込み
            for (int sectorOffset = 0; sectorOffset < _config.SectorsPerCluster; sectorOffset++)
            {
                var (track, head, sector) = _config.ClusterToPhysicalSector(cluster, sectorOffset);
                var sectorData = new byte[_config.SectorSize];
                var sectorDataOffset = sectorOffset * _config.SectorSize;
                
                if (sectorDataOffset < clusterData.Length)
                {
                    var sectorBytesToWrite = Math.Min(_config.SectorSize, clusterData.Length - sectorDataOffset);
                    Array.Copy(clusterData, sectorDataOffset, sectorData, 0, sectorBytesToWrite);
                }
                
                _diskContainer.WriteSector(track, head, sector, sectorData);
            }
        }
    }
    
    /// <summary>
    /// FATチェーンを構築
    /// </summary>
    private void BuildFatChain(List<int> clusters, int dataSize)
    {
        var fatData = ReadFatData();
        if (fatData == null)
            throw new FileSystemException("FAT読み取りに失敗しました");
        
        for (int i = 0; i < clusters.Count; i++)
        {
            var cluster = clusters[i];
            
            if (i == clusters.Count - 1)
            {
                // 最終クラスタ：使用セクタ数を計算
                var remainingBytes = dataSize - (i * _config.ClusterSize);
                var usedSectors = (remainingBytes + _config.SectorSize - 1) / _config.SectorSize;
                fatData[cluster] = _config.CreateEofMarker(usedSectors);
            }
            else
            {
                // 中間クラスタ：次のクラスタ番号を設定
                fatData[cluster] = (byte)clusters[i + 1];
            }
        }
        
        // FAT書き戻し
        WriteFatData(fatData);
    }
    
    /// <summary>
    /// FATデータを書き戻し
    /// </summary>
    private void WriteFatData(byte[] fatData)
    {
        int dataOffset = 0;
        
        for (int i = 0; i < _config.FatSectorCount; i++)
        {
            var sectorData = new byte[_config.SectorSize];
            var bytesToWrite = Math.Min(_config.SectorSize, fatData.Length - dataOffset);
            
            if (bytesToWrite > 0)
            {
                Array.Copy(fatData, dataOffset, sectorData, 0, bytesToWrite);
                dataOffset += bytesToWrite;
            }
            
            var (track, head, sector) = _config.GetFatSectorAddress(i);
            _diskContainer.WriteSector(track, head, sector, sectorData);
        }
    }
    
    /// <summary>
    /// クラスタを解放
    /// </summary>
    private void FreeClusters(List<int> clusters)
    {
        var fatData = ReadFatData();
        if (fatData == null)
            return;
        
        foreach (var cluster in clusters)
        {
            if (cluster < fatData.Length)
            {
                fatData[cluster] = _config.FatFreeMarker;
            }
        }
        
        WriteFatData(fatData);
    }
    
    /// <summary>
    /// ディレクトリエントリを作成
    /// </summary>
    private void CreateDirectoryEntry(string baseName, string extension, int startCluster)
    {
        var entries = ReadDirectoryEntries();
        
        // 空きエントリを検索
        var emptyEntryIndex = -1;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].IsEmpty || entries[i].IsDeleted)
            {
                emptyEntryIndex = i;
                break;
            }
        }
        
        if (emptyEntryIndex == -1)
            throw new FileSystemException("ディレクトリが満杯です");
        
        // 新しいエントリを作成
        var newEntry = new N88BasicFileEntry
        {
            FileName = baseName,
            Extension = extension,
            StartCluster = (byte)startCluster,
            Status = N88BasicEntryStatus.Active
        };
        
        // エントリを書き戻し
        WriteDirectoryEntry(emptyEntryIndex, newEntry);
    }
    
    /// <summary>
    /// ディレクトリエントリを削除マーク
    /// </summary>
    private void MarkDirectoryEntryAsDeleted(N88BasicFileEntry entry)
    {
        var entries = ReadDirectoryEntries();
        
        for (int i = 0; i < entries.Count; i++)
        {
            var currentEntry = entries[i];
            if (currentEntry.IsValid &&
                string.Equals(currentEntry.FileName, entry.FileName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(currentEntry.Extension, entry.Extension, StringComparison.OrdinalIgnoreCase))
            {
                currentEntry.Status = N88BasicEntryStatus.Deleted;
                WriteDirectoryEntry(i, currentEntry);
                break;
            }
        }
    }
    
    /// <summary>
    /// 指定位置にディレクトリエントリを書き込み
    /// </summary>
    private void WriteDirectoryEntry(int entryIndex, N88BasicFileEntry entry)
    {
        var sectorIndex = entryIndex / _config.EntriesPerSector;
        var entryOffset = (entryIndex % _config.EntriesPerSector) * _config.EntrySize;
        
        var (track, head, sector) = _config.GetDirectorySectorAddress(sectorIndex);
        var sectorData = _diskContainer.ReadSector(track, head, sector) ?? new byte[_config.SectorSize];
        
        var entryBytes = entry.ToBytes();
        Array.Copy(entryBytes, 0, sectorData, entryOffset, _config.EntrySize);
        
        _diskContainer.WriteSector(track, head, sector, sectorData);
    }
    
    #endregion
}