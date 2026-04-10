namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// PC-8801 N88-BASIC ファイルエントリ (16バイト構造)
/// </summary>
public class N88BasicFileEntry
{
    /// <summary>
    /// ファイル名 (最大6文字)
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// 拡張子 (最大3文字)
    /// </summary>
    public string Extension { get; set; } = string.Empty;
    
    /// <summary>
    /// ファイル属性バイト
    /// </summary>
    public byte Attributes { get; set; }
    
    /// <summary>
    /// 開始クラスタ番号
    /// </summary>
    public byte StartCluster { get; set; }
    
    /// <summary>
    /// エントリ状態
    /// </summary>
    public N88BasicEntryStatus Status { get; set; } = N88BasicEntryStatus.Active;
    
    /// <summary>
    /// 計算されたファイルサイズ (FATチェーンから算出)
    /// </summary>
    public int CalculatedSize { get; set; }
    
    // 属性ビット操作プロパティ
    
    /// <summary>
    /// バイナリフォーマット (機械語) - ビット0
    /// </summary>
    public bool IsBinary
    {
        get => (Attributes & 0x01) != 0;
        set => Attributes = (byte)(value ? (Attributes | 0x01) : (Attributes & ~0x01));
    }
    
    /// <summary>
    /// 書き込み禁止 - ビット4
    /// </summary>
    public bool IsWriteProtected
    {
        get => (Attributes & 0x10) != 0;
        set => Attributes = (byte)(value ? (Attributes | 0x10) : (Attributes & ~0x10));
    }
    
    /// <summary>
    /// 編集禁止 (Pオプション) - ビット5
    /// </summary>
    public bool IsEditProtected
    {
        get => (Attributes & 0x20) != 0;
        set => Attributes = (byte)(value ? (Attributes | 0x20) : (Attributes & ~0x20));
    }
    
    /// <summary>
    /// 書き込み後読み取り検証 - ビット6
    /// </summary>
    public bool IsVerifyAfterWrite
    {
        get => (Attributes & 0x40) != 0;
        set => Attributes = (byte)(value ? (Attributes | 0x40) : (Attributes & ~0x40));
    }
    
    /// <summary>
    /// トークン化BASICプログラム - ビット7 (0=ASCII, 1=トークン化)
    /// </summary>
    public bool IsTokenizedBasic
    {
        get => (Attributes & 0x80) != 0;
        set => Attributes = (byte)(value ? (Attributes | 0x80) : (Attributes & ~0x80));
    }
    
    /// <summary>
    /// ASCIIテキストファイル (ビット7=0)
    /// </summary>
    public bool IsAsciiText => !IsTokenizedBasic;
    
    /// <summary>
    /// フルファイル名を取得 (ファイル名.拡張子)
    /// </summary>
    public string FullName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Extension))
                return FileName;
            return $"{FileName}.{Extension}";
        }
    }
    
    /// <summary>
    /// ファイルタイプを取得
    /// </summary>
    public N88BasicFileType FileType
    {
        get
        {
            if (IsBinary) return N88BasicFileType.Binary;
            if (IsTokenizedBasic) return N88BasicFileType.TokenizedBasic;
            return N88BasicFileType.AsciiText;
        }
    }
    
    /// <summary>
    /// エントリが有効かどうか
    /// </summary>
    public bool IsValid => Status == N88BasicEntryStatus.Active;
    
    /// <summary>
    /// エントリが削除済みかどうか
    /// </summary>
    public bool IsDeleted => Status == N88BasicEntryStatus.Deleted;
    
    /// <summary>
    /// エントリが未使用かどうか
    /// </summary>
    public bool IsEmpty => Status == N88BasicEntryStatus.Empty;
    
    /// <summary>
    /// 16バイトのRAWデータからN88BasicFileEntryを作成
    /// </summary>
    /// <param name="data">16バイトのディレクトリエントリデータ</param>
    /// <returns>N88BasicFileEntry</returns>
    public static N88BasicFileEntry FromBytes(byte[] data)
    {
        if (data.Length != 16)
            throw new ArgumentException("N88-BASICディレクトリエントリは16バイトである必要があります");
        
        var entry = new N88BasicFileEntry();
        
        // エントリ状態判定
        if (data[0] == 0xFF)
        {
            entry.Status = N88BasicEntryStatus.Empty;
            return entry;
        }
        if (data[0] == 0x00)
        {
            entry.Status = N88BasicEntryStatus.Deleted;
            return entry;
        }
        
        // ファイル名 (バイト0-5, 6文字)
        entry.FileName = System.Text.Encoding.ASCII.GetString(data, 0, 6).TrimEnd('\0', ' ');
        
        // 拡張子 (バイト6-8, 3文字)
        entry.Extension = System.Text.Encoding.ASCII.GetString(data, 6, 3).TrimEnd('\0', ' ');
        
        // ファイル属性 (バイト9)
        entry.Attributes = data[9];
        
        // 開始クラスタ番号 (バイト10)
        entry.StartCluster = data[10];
        
        // バイト11-15は予約領域
        
        entry.Status = N88BasicEntryStatus.Active;
        return entry;
    }
    
    /// <summary>
    /// N88BasicFileEntryを16バイトのRAWデータに変換
    /// </summary>
    /// <returns>16バイトのディレクトリエントリデータ</returns>
    public byte[] ToBytes()
    {
        var data = new byte[16];
        
        // エントリ状態に応じた処理
        switch (Status)
        {
            case N88BasicEntryStatus.Empty:
                data[0] = 0xFF;
                return data;
                
            case N88BasicEntryStatus.Deleted:
                data[0] = 0x00;
                return data;
                
            case N88BasicEntryStatus.Active:
                break;
                
            default:
                throw new InvalidOperationException($"不正なエントリ状態: {Status}");
        }
        
        // ファイル名 (バイト0-5, 6文字, 不足分はスペース埋め)
        var fileNameBytes = System.Text.Encoding.ASCII.GetBytes(FileName.PadRight(6));
        Array.Copy(fileNameBytes, 0, data, 0, Math.Min(6, fileNameBytes.Length));
        
        // 拡張子 (バイト6-8, 3文字, 不足分はスペース埋め)
        var extensionBytes = System.Text.Encoding.ASCII.GetBytes(Extension.PadRight(3));
        Array.Copy(extensionBytes, 0, data, 6, Math.Min(3, extensionBytes.Length));
        
        // ファイル属性 (バイト9)
        data[9] = Attributes;
        
        // 開始クラスタ番号 (バイト10)
        data[10] = StartCluster;
        
        // バイト11-15は予約領域 (0で初期化済み)
        
        return data;
    }
    
    public override string ToString()
    {
        if (!IsValid) return $"[{Status}]";
        
        var typeStr = FileType switch
        {
            N88BasicFileType.Binary => "BIN",
            N88BasicFileType.TokenizedBasic => "BAS",
            N88BasicFileType.AsciiText => "TXT",
            _ => "UNK"
        };
        
        var protectionFlags = "";
        if (IsWriteProtected) protectionFlags += "W";
        if (IsEditProtected) protectionFlags += "E";
        if (IsVerifyAfterWrite) protectionFlags += "V";
        
        return $"{FullName} [{typeStr}] {CalculatedSize}bytes" + 
               (protectionFlags.Length > 0 ? $" ({protectionFlags})" : "");
    }
}

/// <summary>
/// N88-BASICファイルエントリの状態
/// </summary>
public enum N88BasicEntryStatus
{
    /// <summary>有効なエントリ</summary>
    Active,
    
    /// <summary>削除済みエントリ (バイト0 = 0x00)</summary>
    Deleted,
    
    /// <summary>未使用エントリ (バイト0 = 0xFF)</summary>
    Empty
}

/// <summary>
/// N88-BASICファイルタイプ
/// </summary>
public enum N88BasicFileType
{
    /// <summary>ASCIIテキストファイル</summary>
    AsciiText,
    
    /// <summary>トークン化BASICプログラム</summary>
    TokenizedBasic,
    
    /// <summary>バイナリ/機械語ファイル</summary>
    Binary
}