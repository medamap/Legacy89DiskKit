using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// PC-8801 N88-BASIC ファイルシステム設定
/// </summary>
public class N88BasicConfiguration
{
    /// <summary>
    /// ディスクタイプ
    /// </summary>
    public DiskType DiskType { get; init; }
    
    // システムトラック位置
    /// <summary>
    /// システムトラック番号
    /// </summary>
    public int SystemTrack { get; init; }
    
    /// <summary>
    /// システムヘッド番号
    /// </summary>
    public int SystemHead { get; init; }
    
    // ディレクトリ・FAT・ID領域
    /// <summary>
    /// ディレクトリ開始セクタ番号
    /// </summary>
    public int DirectoryStartSector { get; init; } = 1;
    
    /// <summary>
    /// ディレクトリセクタ数
    /// </summary>
    public int DirectorySectorCount { get; init; } = 12;
    
    /// <summary>
    /// FAT開始セクタ番号
    /// </summary>
    public int FatStartSector { get; init; } = 14;
    
    /// <summary>
    /// FATセクタ数
    /// </summary>
    public int FatSectorCount { get; init; } = 3;
    
    /// <summary>
    /// IDセクタ番号
    /// </summary>
    public int IdSector { get; init; } = 13;
    
    // ディスク物理特性
    /// <summary>
    /// トラック数
    /// </summary>
    public int TotalTracks { get; init; }
    
    /// <summary>
    /// トラックあたりのセクタ数
    /// </summary>
    public int SectorsPerTrack { get; init; }
    
    /// <summary>
    /// セクタサイズ
    /// </summary>
    public int SectorSize { get; init; } = 256;
    
    // クラスタ定義
    /// <summary>
    /// クラスタあたりのセクタ数
    /// </summary>
    public int SectorsPerCluster { get; init; }
    
    /// <summary>
    /// クラスタサイズ (バイト)
    /// </summary>
    public int ClusterSize => SectorsPerCluster * SectorSize;
    
    /// <summary>
    /// 総クラスタ数
    /// </summary>
    public int TotalClusters { get; init; }
    
    // ファイルエントリ
    /// <summary>
    /// ディレクトリエントリサイズ
    /// </summary>
    public int EntrySize { get; init; } = 16;
    
    /// <summary>
    /// ファイル名最大長
    /// </summary>
    public int FileNameLength { get; init; } = 6;
    
    /// <summary>
    /// 拡張子最大長
    /// </summary>
    public int ExtensionLength { get; init; } = 3;
    
    /// <summary>
    /// セクタあたりのディレクトリエントリ数
    /// </summary>
    public int EntriesPerSector => SectorSize / EntrySize;
    
    /// <summary>
    /// 最大ディレクトリエントリ数
    /// </summary>
    public int MaxDirectoryEntries => DirectorySectorCount * EntriesPerSector;
    
    // FAT値定義
    /// <summary>
    /// 空きクラスタマーカー
    /// </summary>
    public byte FatFreeMarker { get; init; } = 0xFF;
    
    /// <summary>
    /// 終端クラスタマーカーベース (C0h + 使用セクタ数)
    /// </summary>
    public byte FatEofBase { get; init; } = 0xC0;
    
    /// <summary>
    /// システム予約クラスタマーカー
    /// </summary>
    public byte FatReservedMarker { get; init; } = 0xFE;
    
    /// <summary>
    /// 不良クラスタマーカー
    /// </summary>
    public byte FatBadClusterMarker { get; init; } = 0xFD;
    
    /// <summary>
    /// ディスクタイプ別の設定を生成
    /// </summary>
    /// <param name="diskType">ディスクタイプ</param>
    /// <returns>N88BasicConfiguration</returns>
    public static N88BasicConfiguration Create(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => new N88BasicConfiguration
            {
                DiskType = DiskType.TwoD,
                SystemTrack = 18,        // T18/H1
                SystemHead = 1,
                TotalTracks = 40,
                SectorsPerTrack = 16,
                SectorsPerCluster = 8,   // 2D: 8セクタ/クラスタ = 2KB
                TotalClusters = 80       // 概算値
            },
            
            DiskType.TwoDD => new N88BasicConfiguration
            {
                DiskType = DiskType.TwoDD,
                SystemTrack = 40,        // T40/H0
                SystemHead = 0,
                TotalTracks = 80,
                SectorsPerTrack = 16,
                SectorsPerCluster = 16,  // 2DD: 16セクタ/クラスタ = 4KB
                TotalClusters = 160      // 概算値
            },
            
            _ => throw new NotSupportedException($"N88-BASICはディスクタイプ {diskType} をサポートしていません")
        };
    }
    
    /// <summary>
    /// システムトラックの物理位置を取得
    /// </summary>
    /// <returns>(トラック, ヘッド)</returns>
    public (int track, int head) GetSystemLocation() => (SystemTrack, SystemHead);
    
    /// <summary>
    /// ディレクトリセクタの物理アドレスを取得
    /// </summary>
    /// <param name="sectorIndex">ディレクトリ内セクタインデックス (0-11)</param>
    /// <returns>(トラック, ヘッド, セクタ)</returns>
    public (int track, int head, int sector) GetDirectorySectorAddress(int sectorIndex)
    {
        if (sectorIndex < 0 || sectorIndex >= DirectorySectorCount)
            throw new ArgumentOutOfRangeException(nameof(sectorIndex), 
                $"ディレクトリセクタインデックスは0-{DirectorySectorCount - 1}の範囲である必要があります");
        
        return (SystemTrack, SystemHead, DirectoryStartSector + sectorIndex);
    }
    
    /// <summary>
    /// FATセクタの物理アドレスを取得
    /// </summary>
    /// <param name="sectorIndex">FAT内セクタインデックス (0-2)</param>
    /// <returns>(トラック, ヘッド, セクタ)</returns>
    public (int track, int head, int sector) GetFatSectorAddress(int sectorIndex)
    {
        if (sectorIndex < 0 || sectorIndex >= FatSectorCount)
            throw new ArgumentOutOfRangeException(nameof(sectorIndex), 
                $"FATセクタインデックスは0-{FatSectorCount - 1}の範囲である必要があります");
        
        return (SystemTrack, SystemHead, FatStartSector + sectorIndex);
    }
    
    /// <summary>
    /// IDセクタの物理アドレスを取得
    /// </summary>
    /// <returns>(トラック, ヘッド, セクタ)</returns>
    public (int track, int head, int sector) GetIdSectorAddress()
    {
        return (SystemTrack, SystemHead, IdSector);
    }
    
    /// <summary>
    /// FAT値が終端マーカーかどうかを判定
    /// </summary>
    /// <param name="fatValue">FAT値</param>
    /// <param name="usedSectors">使用セクタ数 (省略可)</param>
    /// <returns>終端マーカーかどうか</returns>
    public bool IsEofMarker(byte fatValue, int usedSectors = 0)
    {
        // C0h + 使用セクタ数の形式
        if (usedSectors > 0)
        {
            var expectedValue = FatEofBase | (usedSectors & 0x3F);
            return fatValue == expectedValue;
        }
        
        // 使用セクタ数不明の場合、C0h-FFhの範囲で判定
        return fatValue >= FatEofBase;
    }
    
    /// <summary>
    /// 終端マーカーを生成
    /// </summary>
    /// <param name="usedSectors">使用セクタ数</param>
    /// <returns>終端マーカー値</returns>
    public byte CreateEofMarker(int usedSectors)
    {
        if (usedSectors < 0 || usedSectors > 63)
            throw new ArgumentOutOfRangeException(nameof(usedSectors), 
                "使用セクタ数は0-63の範囲である必要があります");
        
        return (byte)(FatEofBase | (usedSectors & 0x3F));
    }
    
    /// <summary>
    /// FAT値が空きクラスタかどうかを判定
    /// </summary>
    /// <param name="fatValue">FAT値</param>
    /// <returns>空きクラスタかどうか</returns>
    public bool IsFreeCluster(byte fatValue) => fatValue == FatFreeMarker;
    
    /// <summary>
    /// FAT値がシステム予約クラスタかどうかを判定
    /// </summary>
    /// <param name="fatValue">FAT値</param>
    /// <returns>システム予約クラスタかどうか</returns>
    public bool IsReservedCluster(byte fatValue) => fatValue == FatReservedMarker;
    
    /// <summary>
    /// FAT値が不良クラスタかどうかを判定
    /// </summary>
    /// <param name="fatValue">FAT値</param>
    /// <returns>不良クラスタかどうか</returns>
    public bool IsBadCluster(byte fatValue) => fatValue == FatBadClusterMarker;
    
    /// <summary>
    /// FAT値が有効なクラスタ番号かどうかを判定
    /// </summary>
    /// <param name="fatValue">FAT値</param>
    /// <returns>有効なクラスタ番号かどうか</returns>
    public bool IsValidClusterNumber(byte fatValue)
    {
        return fatValue < TotalClusters && 
               !IsFreeCluster(fatValue) && 
               !IsEofMarker(fatValue) && 
               !IsReservedCluster(fatValue) && 
               !IsBadCluster(fatValue);
    }
    
    /// <summary>
    /// クラスタ番号から物理セクタ位置を計算
    /// </summary>
    /// <param name="clusterNumber">クラスタ番号</param>
    /// <param name="sectorOffset">クラスタ内セクタオフセット (0-based)</param>
    /// <returns>(トラック, ヘッド, セクタ)</returns>
    public (int track, int head, int sector) ClusterToPhysicalSector(int clusterNumber, int sectorOffset = 0)
    {
        if (clusterNumber < 0 || clusterNumber >= TotalClusters)
            throw new ArgumentOutOfRangeException(nameof(clusterNumber));
        
        if (sectorOffset < 0 || sectorOffset >= SectorsPerCluster)
            throw new ArgumentOutOfRangeException(nameof(sectorOffset));
        
        // データ領域はシステムトラックを除外して配置
        var absoluteSector = clusterNumber * SectorsPerCluster + sectorOffset;
        
        // システムトラックをスキップ
        var systemTrackSectors = SectorsPerTrack * 2; // 両面
        if (absoluteSector >= SystemTrack * systemTrackSectors)
        {
            absoluteSector += systemTrackSectors;
        }
        
        var track = absoluteSector / (SectorsPerTrack * 2);
        var remainingSectors = absoluteSector % (SectorsPerTrack * 2);
        var head = remainingSectors / SectorsPerTrack;
        var sector = (remainingSectors % SectorsPerTrack) + 1; // セクタ番号は1-based
        
        return (track, head, sector);
    }
    
    public override string ToString()
    {
        return $"N88-BASIC {DiskType} " +
               $"({TotalTracks}T×{SectorsPerTrack}S, {SectorsPerCluster}S/cluster, " +
               $"System=T{SystemTrack}/H{SystemHead})";
    }
}