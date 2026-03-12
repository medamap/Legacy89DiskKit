using System.Text;

namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// MSX-DOS ブートセクタ（BPB含む）
/// </summary>
public class MsxDosBootSector
{
    /// <summary>
    /// セクタあたりのバイト数
    /// </summary>
    public ushort BytesPerSector { get; set; } = 512;
    
    /// <summary>
    /// クラスタあたりのセクタ数
    /// </summary>
    public byte SectorsPerCluster { get; set; } = 2;  // MSX標準
    
    /// <summary>
    /// 予約セクタ数
    /// </summary>
    public ushort ReservedSectors { get; set; } = 1;
    
    /// <summary>
    /// FAT数
    /// </summary>
    public byte NumberOfFats { get; set; } = 2;
    
    /// <summary>
    /// ルートディレクトリエントリ数
    /// </summary>
    public ushort RootEntries { get; set; } = 112;  // MSX標準
    
    /// <summary>
    /// 総セクタ数（16ビット）
    /// </summary>
    public ushort TotalSectors16 { get; set; }
    
    /// <summary>
    /// メディア記述子バイト
    /// </summary>
    public byte MediaDescriptor { get; set; } = 0xF9;  // MSX 720KB標準
    
    /// <summary>
    /// FATあたりのセクタ数
    /// </summary>
    public ushort SectorsPerFat { get; set; } = 9;
    
    /// <summary>
    /// トラックあたりのセクタ数
    /// </summary>
    public ushort SectorsPerTrack { get; set; } = 9;
    
    /// <summary>
    /// ヘッド数
    /// </summary>
    public ushort NumberOfHeads { get; set; } = 2;
    
    /// <summary>
    /// 隠しセクタ数
    /// </summary>
    public uint HiddenSectors { get; set; } = 0;
    
    /// <summary>
    /// 総セクタ数（32ビット）
    /// </summary>
    public uint TotalSectors32 { get; set; } = 0;
    
    /// <summary>
    /// ボリュームラベル
    /// </summary>
    public string VolumeLabel { get; set; } = "";
    
    /// <summary>
    /// ファイルシステムタイプ
    /// </summary>
    public string FileSystemType { get; set; } = "FAT12   ";
    
    /// <summary>
    /// ブートセクタの妥当性を確認
    /// </summary>
    public bool IsValid => BytesPerSector == 512 && NumberOfFats > 0 && RootEntries > 0;
    
    /// <summary>
    /// FAT12として妥当か確認
    /// </summary>
    public bool IsValidFat12() => IsValid && SectorsPerFat > 0 && 
        (TotalSectors16 > 0 || TotalSectors32 > 0);
    
    /// <summary>
    /// 実際の総セクタ数を取得
    /// </summary>
    public uint GetTotalSectors() => TotalSectors16 > 0 ? TotalSectors16 : TotalSectors32;
    
    /// <summary>
    /// データ領域の開始セクタを計算
    /// </summary>
    public int FirstDataSector => ReservedSectors + (NumberOfFats * SectorsPerFat) + 
                                 ((RootEntries * 32 + BytesPerSector - 1) / BytesPerSector);
    
    /// <summary>
    /// MSX-DOS 1.0互換性チェック
    /// </summary>
    /// <param name="fatMediaDescriptor">FATの最初のバイト（メディア記述子）</param>
    /// <returns>MSX-DOS 1.0形式の場合true</returns>
    public bool IsMsxDos10Compatible(byte fatMediaDescriptor)
    {
        // MSX-DOS 1.0はBPBを無視し、FATのメディア記述子バイトを優先する
        return fatMediaDescriptor == MediaDescriptor;
    }
    
    /// <summary>
    /// バイトデータからMSX-DOSブートセクタを解析
    /// </summary>
    /// <param name="data">512バイトのブートセクタデータ</param>
    /// <returns>解析されたブートセクタ</returns>
    public static MsxDosBootSector Parse(byte[] data)
    {
        if (data.Length < 512)
            throw new ArgumentException("ブートセクタデータが不足しています（512バイト必要）");
        
        var bootSector = new MsxDosBootSector();
        
        // BPB解析（オフセット0x0B～）
        bootSector.BytesPerSector = BitConverter.ToUInt16(data, 0x0B);
        bootSector.SectorsPerCluster = data[0x0D];
        bootSector.ReservedSectors = BitConverter.ToUInt16(data, 0x0E);
        bootSector.NumberOfFats = data[0x10];
        bootSector.RootEntries = BitConverter.ToUInt16(data, 0x11);
        bootSector.TotalSectors16 = BitConverter.ToUInt16(data, 0x13);
        bootSector.MediaDescriptor = data[0x15];
        bootSector.SectorsPerFat = BitConverter.ToUInt16(data, 0x16);
        bootSector.SectorsPerTrack = BitConverter.ToUInt16(data, 0x18);
        bootSector.NumberOfHeads = BitConverter.ToUInt16(data, 0x1A);
        bootSector.HiddenSectors = BitConverter.ToUInt32(data, 0x1C);
        bootSector.TotalSectors32 = BitConverter.ToUInt32(data, 0x20);
        
        // 拡張ブートレコード（MSX-DOS 2.0以降）
        if (data.Length >= 54)
        {
            // ボリュームラベル（オフセット0x2B、11バイト）
            bootSector.VolumeLabel = Encoding.ASCII.GetString(data, 0x2B, 11).Trim('\0', ' ');
            
            // ファイルシステムタイプ（オフセット0x36、8バイト）
            bootSector.FileSystemType = Encoding.ASCII.GetString(data, 0x36, 8).Trim('\0', ' ');
        }
        
        return bootSector;
    }
    
    /// <summary>
    /// ブートセクタを512バイトのバイト配列に変換
    /// </summary>
    /// <returns>512バイトのブートセクタデータ</returns>
    public byte[] ToBytes()
    {
        var data = new byte[512];
        
        // ジャンプ命令（MSX-DOS標準）
        data[0] = 0xEB;
        data[1] = 0x3C;
        data[2] = 0x90;
        
        // OEMネーム（MSX-DOS）
        var oemName = Encoding.ASCII.GetBytes("MSX-DOS ");
        Array.Copy(oemName, 0, data, 3, Math.Min(8, oemName.Length));
        
        // BPB設定
        BitConverter.GetBytes(BytesPerSector).CopyTo(data, 0x0B);
        data[0x0D] = SectorsPerCluster;
        BitConverter.GetBytes(ReservedSectors).CopyTo(data, 0x0E);
        data[0x10] = NumberOfFats;
        BitConverter.GetBytes(RootEntries).CopyTo(data, 0x11);
        BitConverter.GetBytes(TotalSectors16).CopyTo(data, 0x13);
        data[0x15] = MediaDescriptor;
        BitConverter.GetBytes(SectorsPerFat).CopyTo(data, 0x16);
        BitConverter.GetBytes(SectorsPerTrack).CopyTo(data, 0x18);
        BitConverter.GetBytes(NumberOfHeads).CopyTo(data, 0x1A);
        BitConverter.GetBytes(HiddenSectors).CopyTo(data, 0x1C);
        BitConverter.GetBytes(TotalSectors32).CopyTo(data, 0x20);
        
        // 拡張ブートレコード（MSX-DOS 2.0）
        data[0x24] = 0x80; // ドライブ番号
        data[0x25] = 0x00; // 予約
        data[0x26] = 0x29; // 拡張ブートシグネチャ
        
        // ボリュームID（仮の値）
        BitConverter.GetBytes(0x12345678).CopyTo(data, 0x27);
        
        // ボリュームラベル
        var volLabel = Encoding.ASCII.GetBytes(VolumeLabel.PadRight(11));
        Array.Copy(volLabel, 0, data, 0x2B, Math.Min(11, volLabel.Length));
        
        // ファイルシステムタイプ
        var fsType = Encoding.ASCII.GetBytes(FileSystemType.PadRight(8));
        Array.Copy(fsType, 0, data, 0x36, Math.Min(8, fsType.Length));
        
        // ブートシグネチャ
        data[510] = 0x55;
        data[511] = 0xAA;
        
        return data;
    }
    
    /// <summary>
    /// MSX-DOS設定からブートセクタを作成
    /// </summary>
    /// <param name="config">MSX-DOS設定</param>
    /// <param name="volumeLabel">ボリュームラベル</param>
    /// <returns>ブートセクタ</returns>
    public static MsxDosBootSector FromConfiguration(MsxDosConfiguration config, string volumeLabel = "")
    {
        return new MsxDosBootSector
        {
            BytesPerSector = (ushort)config.SectorSize,
            SectorsPerCluster = (byte)config.SectorsPerCluster,
            ReservedSectors = (ushort)config.ReservedSectors,
            NumberOfFats = (byte)config.NumberOfFats,
            RootEntries = (ushort)config.RootDirectoryEntries,
            TotalSectors16 = (ushort)config.TotalSectors,
            MediaDescriptor = config.MediaDescriptor,
            SectorsPerFat = (ushort)config.SectorsPerFat,
            SectorsPerTrack = (ushort)config.SectorsPerTrack,
            NumberOfHeads = (ushort)config.NumberOfHeads,
            VolumeLabel = volumeLabel,
            FileSystemType = "FAT12   "
        };
    }
    
    public override string ToString()
    {
        return $"MSX-DOS BootSector: {GetTotalSectors()}sectors, " +
               $"{SectorsPerCluster}sec/cluster, {RootEntries}rootents, " +
               $"Media=0x{MediaDescriptor:X2}, Label='{VolumeLabel}'";
    }
}