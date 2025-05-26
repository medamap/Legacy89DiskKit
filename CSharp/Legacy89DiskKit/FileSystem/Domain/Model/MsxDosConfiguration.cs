using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// MSX-DOS ファイルシステム設定
/// </summary>
public class MsxDosConfiguration
{
    /// <summary>
    /// セクタサイズ（バイト）
    /// </summary>
    public int SectorSize { get; set; } = 512;
    
    /// <summary>
    /// クラスタあたりのセクタ数
    /// </summary>
    public int SectorsPerCluster { get; set; }
    
    /// <summary>
    /// 予約セクタ数
    /// </summary>
    public int ReservedSectors { get; set; } = 1;
    
    /// <summary>
    /// FAT数
    /// </summary>
    public int NumberOfFats { get; set; } = 2;
    
    /// <summary>
    /// ルートディレクトリエントリ数
    /// </summary>
    public int RootDirectoryEntries { get; set; }
    
    /// <summary>
    /// FATあたりのセクタ数
    /// </summary>
    public int SectorsPerFat { get; set; }
    
    /// <summary>
    /// トラックあたりのセクタ数
    /// </summary>
    public int SectorsPerTrack { get; set; }
    
    /// <summary>
    /// ヘッド数（面数）
    /// </summary>
    public int NumberOfHeads { get; set; }
    
    /// <summary>
    /// 総セクタ数
    /// </summary>
    public int TotalSectors { get; set; }
    
    /// <summary>
    /// メディア記述子バイト
    /// </summary>
    public byte MediaDescriptor { get; set; }
    
    /// <summary>
    /// クラスタサイズ（バイト）
    /// </summary>
    public int ClusterSize => SectorSize * SectorsPerCluster;
    
    /// <summary>
    /// データ領域の開始セクタ
    /// </summary>
    public int FirstDataSector => ReservedSectors + (NumberOfFats * SectorsPerFat) + 
                                 ((RootDirectoryEntries * 32 + SectorSize - 1) / SectorSize);
    
    /// <summary>
    /// 総クラスタ数
    /// </summary>
    public int TotalClusters => (TotalSectors - FirstDataSector) / SectorsPerCluster;
    
    /// <summary>
    /// ルートディレクトリのセクタ数
    /// </summary>
    public int RootDirectorySectors => (RootDirectoryEntries * 32 + SectorSize - 1) / SectorSize;
    
    /// <summary>
    /// ディスクタイプに応じたMSX-DOS設定を取得
    /// </summary>
    /// <param name="diskType">ディスクタイプ</param>
    /// <returns>MSX-DOS設定</returns>
    public static MsxDosConfiguration ForDiskType(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoDD => new MsxDosConfiguration
            {
                // MSX 720KB 標準設定
                SectorsPerCluster = 2,        // MSX固有（PC/ATは通常1）
                RootDirectoryEntries = 112,   // MSX固有（PC/ATは通常224）
                SectorsPerFat = 9,
                SectorsPerTrack = 9,
                NumberOfHeads = 2,
                TotalSectors = 1440,         // 80 × 2 × 9
                MediaDescriptor = 0xF9       // MSX 720KB標準
            },
            DiskType.TwoD => new MsxDosConfiguration
            {
                // MSX 360KB 設定
                SectorsPerCluster = 2,
                RootDirectoryEntries = 112,
                SectorsPerFat = 5,
                SectorsPerTrack = 9,
                NumberOfHeads = 1,
                TotalSectors = 720,          // 80 × 1 × 9
                MediaDescriptor = 0xF9
            },
            DiskType.TwoHD => new MsxDosConfiguration
            {
                // MSX 1.44MB 設定（一部MSXで対応）
                SectorsPerCluster = 1,
                RootDirectoryEntries = 224,
                SectorsPerFat = 9,
                SectorsPerTrack = 18,
                NumberOfHeads = 2,
                TotalSectors = 2880,         // 80 × 2 × 18
                MediaDescriptor = 0xF0       // HD用
            },
            _ => throw new ArgumentException($"MSX-DOSでサポートされていないディスクタイプ: {diskType}")
        };
    }
    
    /// <summary>
    /// 設定の妥当性を検証
    /// </summary>
    /// <returns>妥当な場合true</returns>
    public bool IsValid()
    {
        return SectorSize > 0 &&
               SectorsPerCluster > 0 &&
               ReservedSectors > 0 &&
               NumberOfFats > 0 &&
               RootDirectoryEntries > 0 &&
               SectorsPerFat > 0 &&
               SectorsPerTrack > 0 &&
               NumberOfHeads > 0 &&
               TotalSectors > 0;
    }
    
    /// <summary>
    /// クラスタ番号から論理セクタアドレス（LBA）を計算
    /// </summary>
    /// <param name="clusterNumber">クラスタ番号（2以上）</param>
    /// <returns>論理セクタアドレス</returns>
    public int ClusterToLba(int clusterNumber)
    {
        if (clusterNumber < 2)
            throw new ArgumentException("クラスタ番号は2以上である必要があります");
        
        return FirstDataSector + (clusterNumber - 2) * SectorsPerCluster;
    }
    
    /// <summary>
    /// FAT領域の開始セクタを取得
    /// </summary>
    /// <param name="fatNumber">FAT番号（0または1）</param>
    /// <returns>FAT開始セクタ</returns>
    public int GetFatStartSector(int fatNumber)
    {
        if (fatNumber < 0 || fatNumber >= NumberOfFats)
            throw new ArgumentException($"不正なFAT番号: {fatNumber}");
        
        return ReservedSectors + (fatNumber * SectorsPerFat);
    }
    
    /// <summary>
    /// ルートディレクトリの開始セクタを取得
    /// </summary>
    /// <returns>ルートディレクトリ開始セクタ</returns>
    public int GetRootDirectoryStartSector()
    {
        return ReservedSectors + (NumberOfFats * SectorsPerFat);
    }
    
    public override string ToString()
    {
        return $"MSX-DOS: {TotalSectors}sectors, {SectorsPerCluster}sec/cluster, " +
               $"{RootDirectoryEntries}rootents, Media=0x{MediaDescriptor:X2}";
    }
}