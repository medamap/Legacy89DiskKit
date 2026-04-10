using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

/// <summary>
/// MSX-DOS ファイルシステム実装
/// Fat12FileSystemを内包し、MSX固有の設定と動作を提供
/// </summary>
public class MsxDosFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly MsxDosConfiguration _msxConfig;
    private readonly Fat12FileSystem _baseFat12FileSystem;
    private MsxDosBootSector? _msxBootSector;
    private bool _isMsxDos10Mode = false;
    
    /// <summary>
    /// MSX-DOSファイルシステムを初期化
    /// </summary>
    /// <param name="diskContainer">ディスクコンテナ</param>
    public MsxDosFileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer ?? throw new ArgumentNullException(nameof(diskContainer));
        _msxConfig = MsxDosConfiguration.ForDiskType(diskContainer.DiskType);
        _baseFat12FileSystem = new Fat12FileSystem(diskContainer);
        DetectMsxDosVersion();
    }
    
    /// <summary>
    /// ディスクコンテナを取得
    /// </summary>
    public IDiskContainer DiskContainer => _diskContainer;
    
    /// <summary>
    /// ファイルシステムがフォーマット済みかどうか
    /// </summary>
    public bool IsFormatted
    {
        get
        {
            try
            {
                var bootSector = ReadMsxDosBootSector();
                
                // MSX-DOS 1.0形式の場合は、FATのメディア記述子バイトを確認
                if (_isMsxDos10Mode)
                {
                    return ValidateMsxDos10Format(bootSector);
                }
                
                // MSX-DOS 2.0形式の場合は、標準的なBPB検証
                return bootSector.IsValidFat12() && ValidateMsxConfiguration(bootSector);
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// ファイルシステムをフォーマット
    /// </summary>
    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new FileSystemException("読み取り専用ディスクはフォーマットできません");
        
        try
        {
            // MSX-DOSブートセクタを作成
            var volumeLabel = $"MSX_DISK_{DateTime.Now:yyyyMM}";
            _msxBootSector = MsxDosBootSector.FromConfiguration(_msxConfig, volumeLabel);
            
            // ブートセクタを書き込み
            WriteBootSector(_msxBootSector);
            
            // FATを初期化（MSX固有のメディア記述子バイト設定）
            InitializeMsxFat();
            
            // ルートディレクトリを初期化
            InitializeRootDirectory();
            
            Console.WriteLine($"MSX-DOSディスクをフォーマットしました: {_msxConfig}");
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"MSX-DOSフォーマットに失敗しました: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// ファイルシステム情報を取得
    /// </summary>
    /// <returns>ファイルシステム情報</returns>
    public HuBasicFileSystemInfo GetFileSystemInfo()
    {
        try
        {
            var bootSector = ReadMsxDosBootSector();
            var freeClusterCount = CountFreeClusters();
            
            return new HuBasicFileSystemInfo(
                TotalClusters: _msxConfig.TotalClusters,
                FreeClusters: freeClusterCount,
                ClusterSize: _msxConfig.ClusterSize,
                SectorSize: _msxConfig.SectorSize
            );
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"ファイルシステム情報の取得に失敗しました: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// ブートセクタを取得
    /// </summary>
    /// <returns>ブートセクタ</returns>
    public BootSector GetBootSector()
    {
        try
        {
            var msxBootSector = ReadMsxDosBootSector();
            
            return new BootSector(
                IsBootable: true,
                Label: msxBootSector.VolumeLabel,
                Extension: "",
                Size: 0,
                LoadAddress: 0,
                ExecuteAddress: 0,
                ModifiedDate: DateTime.MinValue,
                StartSector: 0
            );
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"ブートセクタの取得に失敗しました: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// ブートセクタを書き込み
    /// </summary>
    /// <param name="bootSector">ブートセクタ</param>
    public void WriteBootSector(BootSector bootSector)
    {
        if (_diskContainer.IsReadOnly)
            throw new FileSystemException("読み取り専用ディスクには書き込みできません");
        
        try
        {
            // MSX-DOSブートセクタに変換
            var msxBootSector = MsxDosBootSector.FromConfiguration(_msxConfig, bootSector.Label);
            
            WriteBootSector(msxBootSector);
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"ブートセクタの書き込みに失敗しました: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// ファイル一覧を取得
    /// </summary>
    /// <returns>ファイル一覧</returns>
    public IEnumerable<FileEntry> GetFiles()
    {
        // 基本的なFAT12のディレクトリ読み取りを使用
        return _baseFat12FileSystem.GetFiles();
    }
    
    /// <summary>
    /// 指定したファイルを取得
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>ファイルエントリ（見つからない場合はnull）</returns>
    public FileEntry? GetFile(string fileName)
    {
        ValidateFileName(fileName);
        return _baseFat12FileSystem.GetFile(fileName);
    }
    
    /// <summary>
    /// ファイルを読み取り
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>ファイルデータ</returns>
    public byte[] ReadFile(string fileName)
    {
        ValidateFileName(fileName);
        return _baseFat12FileSystem.ReadFile(fileName);
    }
    
    /// <summary>
    /// ファイルを読み取り
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <param name="allowPartialRead">部分読み取りを許可するか</param>
    /// <returns>ファイルデータ</returns>
    public byte[] ReadFile(string fileName, bool allowPartialRead)
    {
        ValidateFileName(fileName);
        return _baseFat12FileSystem.ReadFile(fileName, allowPartialRead);
    }
    
    /// <summary>
    /// ファイルを書き込み
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <param name="data">ファイルデータ</param>
    /// <param name="isText">テキストファイルかどうか</param>
    /// <param name="loadAddress">ロードアドレス</param>
    /// <param name="execAddress">実行アドレス</param>
    public void WriteFile(string fileName, byte[] data, bool isText = false, ushort loadAddress = 0, ushort execAddress = 0)
    {
        ValidateFileName(fileName);
        _baseFat12FileSystem.WriteFile(fileName, data, isText, loadAddress, execAddress);
    }
    
    /// <summary>
    /// ファイルを削除
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    public void DeleteFile(string fileName)
    {
        ValidateFileName(fileName);
        _baseFat12FileSystem.DeleteFile(fileName);
    }
    
    /// <summary>
    /// ファイル名を検証
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>検証結果</returns>
    private bool ValidateFileName(string fileName)
    {
        var result = MsxDosFileNameValidator.ValidateFileName(fileName);
        if (!result.IsValid)
        {
            throw new FileSystemException($"無効なファイル名: {result.ErrorMessage}");
        }
        return true;
    }
    
    /// <summary>
    /// MSX-DOSのバージョンを検出
    /// </summary>
    private void DetectMsxDosVersion()
    {
        try
        {
            // ブートセクタを読み取り
            var bootSectorData = _diskContainer.ReadSector(0, 0, 1);
            if (bootSectorData == null || bootSectorData.Length < 512)
            {
                _isMsxDos10Mode = true; // デフォルトはMSX-DOS 1.0モード
                return;
            }
            
            // BPBの存在確認
            var hasValidBpb = bootSectorData[0x0B] == 0x00 && bootSectorData[0x0C] == 0x02; // BytesPerSector = 512
            
            if (!hasValidBpb)
            {
                _isMsxDos10Mode = true;
                Console.WriteLine("MSX-DOS 1.0形式を検出しました（BPBなし）");
            }
            else
            {
                // FATのメディア記述子バイトを確認
                var fatData = _diskContainer.ReadSector(0, 0, 2); // 論理セクタ1（FAT開始）
                if (fatData != null && fatData.Length > 0)
                {
                    var fatMediaDescriptor = fatData[0];
                    var bpbMediaDescriptor = bootSectorData[0x15];
                    
                    if (fatMediaDescriptor != bpbMediaDescriptor)
                    {
                        _isMsxDos10Mode = true;
                        Console.WriteLine($"MSX-DOS 1.0互換モードを使用します（FAT媒体記述子: 0x{fatMediaDescriptor:X2}）");
                    }
                    else
                    {
                        Console.WriteLine("MSX-DOS 2.0形式を検出しました");
                    }
                }
            }
        }
        catch
        {
            _isMsxDos10Mode = true; // エラー時はMSX-DOS 1.0モード
        }
    }
    
    /// <summary>
    /// MSX-DOSブートセクタを読み取り
    /// </summary>
    /// <returns>MSX-DOSブートセクタ</returns>
    private MsxDosBootSector ReadMsxDosBootSector()
    {
        if (_msxBootSector != null)
            return _msxBootSector;
        
        var bootSectorData = _diskContainer.ReadSector(0, 0, 1);
        if (bootSectorData == null)
            throw new FileSystemException("ブートセクタの読み取りに失敗しました");
        
        _msxBootSector = MsxDosBootSector.Parse(bootSectorData);
        return _msxBootSector;
    }
    
    /// <summary>
    /// MSX-DOSブートセクタを書き込み
    /// </summary>
    /// <param name="bootSector">MSX-DOSブートセクタ</param>
    private void WriteBootSector(MsxDosBootSector bootSector)
    {
        var bootSectorData = bootSector.ToBytes();
        _diskContainer.WriteSector(0, 0, 1, bootSectorData);
        
        _msxBootSector = bootSector;
    }
    
    /// <summary>
    /// MSX-DOS 1.0形式の妥当性を検証
    /// </summary>
    /// <param name="bootSector">ブートセクタ</param>
    /// <returns>妥当な場合true</returns>
    private bool ValidateMsxDos10Format(MsxDosBootSector bootSector)
    {
        try
        {
            // FATのメディア記述子バイトを確認
            var fatData = _diskContainer.ReadSector(0, 0, 2); // FAT開始セクタ
            if (fatData == null || fatData.Length == 0)
                return false;
            
            var fatMediaDescriptor = fatData[0];
            
            // MSX-DOS 1.0は FATのメディア記述子バイトから設定を決定
            return IsValidMsxMediaDescriptor(fatMediaDescriptor);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// MSX設定の妥当性を検証
    /// </summary>
    /// <param name="bootSector">ブートセクタ</param>
    /// <returns>妥当な場合true</returns>
    private bool ValidateMsxConfiguration(MsxDosBootSector bootSector)
    {
        // MSX固有の設定値を確認
        return bootSector.SectorsPerCluster == _msxConfig.SectorsPerCluster &&
               bootSector.RootEntries == _msxConfig.RootDirectoryEntries &&
               IsValidMsxMediaDescriptor(bootSector.MediaDescriptor);
    }
    
    /// <summary>
    /// MSXメディア記述子の妥当性を確認
    /// </summary>
    /// <param name="mediaDescriptor">メディア記述子バイト</param>
    /// <returns>妥当な場合true</returns>
    private bool IsValidMsxMediaDescriptor(byte mediaDescriptor)
    {
        // MSXで使用される一般的なメディア記述子
        return mediaDescriptor == 0xF9 || // 720KB (標準)
               mediaDescriptor == 0xF8 || // HD
               mediaDescriptor == 0xF0;   // 1.44MB
    }
    
    /// <summary>
    /// MSX-DOS用FATを初期化
    /// </summary>
    private void InitializeMsxFat()
    {
        // FAT領域を初期化
        for (int fatNumber = 0; fatNumber < _msxConfig.NumberOfFats; fatNumber++)
        {
            var fatStartSector = _msxConfig.GetFatStartSector(fatNumber);
            
            for (int sectorOffset = 0; sectorOffset < _msxConfig.SectorsPerFat; sectorOffset++)
            {
                var sectorData = new byte[_msxConfig.SectorSize];
                
                // 最初のセクタの場合、メディア記述子バイトとEOCマーカーを設定
                if (sectorOffset == 0)
                {
                    // FAT12の最初の3バイト
                    sectorData[0] = _msxConfig.MediaDescriptor;  // メディア記述子
                    sectorData[1] = 0xFF;                       // EOC (0xFFF の下位8ビット)
                    sectorData[2] = 0xFF;                       // EOC (0xFFF の上位4ビット)
                }
                
                var (track, head, sector) = GetPhysicalAddress(fatStartSector + sectorOffset);
                _diskContainer.WriteSector(track, head, sector, sectorData);
            }
        }
    }
    
    /// <summary>
    /// ルートディレクトリを初期化
    /// </summary>
    private void InitializeRootDirectory()
    {
        var rootStartSector = _msxConfig.GetRootDirectoryStartSector();
        var rootSectorCount = _msxConfig.RootDirectorySectors;
        
        for (int sectorOffset = 0; sectorOffset < rootSectorCount; sectorOffset++)
        {
            var sectorData = new byte[_msxConfig.SectorSize];
            // 全て0でクリア（未使用ディレクトリエントリ）
            
            var (track, head, sector) = GetPhysicalAddress(rootStartSector + sectorOffset);
            _diskContainer.WriteSector(track, head, sector, sectorData);
        }
    }
    
    /// <summary>
    /// 空きクラスタ数をカウント
    /// </summary>
    /// <returns>空きクラスタ数</returns>
    private int CountFreeClusters()
    {
        int freeCount = 0;
        
        try
        {
            // FAT12エントリを走査（クラスタ2から開始）
            for (int cluster = 2; cluster < _msxConfig.TotalClusters + 2; cluster++)
            {
                var fatEntry = GetFatEntry(cluster);
                if (fatEntry == 0x000) // 空きクラスタ
                {
                    freeCount++;
                }
            }
        }
        catch
        {
            // エラー時は0を返す
        }
        
        return freeCount;
    }
    
    /// <summary>
    /// FATエントリを取得
    /// </summary>
    /// <param name="clusterNumber">クラスタ番号</param>
    /// <returns>FATエントリ値</returns>
    private ushort GetFatEntry(int clusterNumber)
    {
        // FAT12の12ビットエントリを読み取り
        var fatStartSector = _msxConfig.GetFatStartSector(0); // FAT1を使用
        var entryOffset = clusterNumber * 3 / 2; // 12ビット = 1.5バイト
        var sectorOffset = entryOffset / _msxConfig.SectorSize;
        var byteOffset = entryOffset % _msxConfig.SectorSize;
        
        var (track, head, sector) = GetPhysicalAddress(fatStartSector + sectorOffset);
        var sectorData = _diskContainer.ReadSector(track, head, sector);
        
        if (sectorData == null || byteOffset + 1 >= sectorData.Length)
            return 0xFFF; // 読み取りエラー時はEOC
        
        ushort fatEntry;
        if (clusterNumber % 2 == 0)
        {
            // 偶数クラスタ: 下位12ビット
            fatEntry = (ushort)(sectorData[byteOffset] | ((sectorData[byteOffset + 1] & 0x0F) << 8));
        }
        else
        {
            // 奇数クラスタ: 上位12ビット
            fatEntry = (ushort)(((sectorData[byteOffset] & 0xF0) >> 4) | (sectorData[byteOffset + 1] << 4));
        }
        
        return fatEntry;
    }
    
    /// <summary>
    /// 論理セクタ番号から物理アドレスを取得
    /// </summary>
    /// <param name="logicalSector">論理セクタ番号</param>
    /// <returns>物理アドレス（track, head, sector）</returns>
    private (int track, int head, int sector) GetPhysicalAddress(int logicalSector)
    {
        var sectorsPerTrack = _msxConfig.SectorsPerTrack;
        var numberOfHeads = _msxConfig.NumberOfHeads;
        
        var track = logicalSector / (sectorsPerTrack * numberOfHeads);
        var remainder = logicalSector % (sectorsPerTrack * numberOfHeads);
        var head = remainder / sectorsPerTrack;
        var sector = (remainder % sectorsPerTrack) + 1; // セクタ番号は1から開始
        
        return (track, head, sector);
    }
    
    public void Dispose()
    {
        // Fat12FileSystemはIDisposableを実装していないため、コメントアウト
        // _baseFat12FileSystem?.Dispose();
    }
}