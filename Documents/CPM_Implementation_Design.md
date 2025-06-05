# CP/M ファイルシステム実装設計書

## 概要
本ドキュメントは、Legacy89DiskKitにCP/Mファイルシステムサポートを追加するための設計指針を記載します。

## アーキテクチャ概要

CP/Mサポートは、既存のDDD（ドメイン駆動設計）アーキテクチャに従って実装されます。

### 影響を受けるドメイン

1. **FileSystemドメイン** - 主要な変更
2. **CharacterEncodingドメイン** - CP/M用エンコーダーの追加
3. **DiskImageドメイン** - 変更不要（既存のD88/DSKで対応可能）

## FileSystemドメインへの追加

### 新規作成ファイル

#### 1. Domain層
```
FileSystem/Domain/Model/
├── CpmConfiguration.cs      # CP/Mディスクパラメータ（DPB相当）
└── CpmFileEntry.cs         # CP/Mディレクトリエントリ構造
```

#### 2. Infrastructure層
```
FileSystem/Infrastructure/
├── FileSystem/
│   └── CpmFileSystem.cs    # IFileSystemインターフェースの実装
└── Utility/
    └── CpmFileNameValidator.cs  # 8.3形式ファイル名検証
```

### CpmConfiguration設計

```csharp
public class CpmConfiguration
{
    // ディスクパラメータブロック（DPB）相当の設定
    public int BlockSize { get; init; }        // アロケーションブロックサイズ
    public int DirectoryEntries { get; init; } // ディレクトリエントリ数
    public int TotalBlocks { get; init; }      // 総ブロック数
    public int ReservedTracks { get; init; }   // システム予約トラック数
    public int SectorsPerTrack { get; init; }  // トラックあたりセクタ数
    public int SectorSize { get; init; }       // セクタサイズ（通常128バイト）
    public int ExtentMask { get; init; }       // エクステントマスク
    public int BlockShift { get; init; }       // ブロックシフト
    public int BlockMask { get; init; }        // ブロックマスク
}
```

### CpmFileEntry設計

```csharp
public record CpmFileEntry(
    byte UserNumber,           // ユーザー番号（0-15、0xE5=削除）
    string FileName,           // ファイル名（8文字、空白パディング）
    string Extension,          // 拡張子（3文字、空白パディング）
    byte ExtentNumber,         // エクステント番号（下位）
    byte ExtentHigh,           // エクステント番号（上位）
    byte RecordCount,          // 128バイトレコード数
    ushort[] AllocationBlocks, // アロケーションブロック配列
    bool IsReadOnly,           // 読み取り専用フラグ（bit7）
    bool IsSystem,             // システムファイルフラグ（bit7）
    bool IsArchived           // アーカイブフラグ（bit7）
);
```

### CpmFileSystem実装概要

```csharp
public class CpmFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly CpmConfiguration _config;
    private readonly ICharacterEncoder _encoder;
    
    // CP/M固有メソッド
    private List<CpmFileEntry> ReadDirectory();
    private byte[] ReadAllocationBlock(int blockNumber);
    private void WriteAllocationBlock(int blockNumber, byte[] data);
    private bool IsValidEntry(byte[] entryData);
    private CpmFileEntry ParseDirectoryEntry(byte[] entryData);
    
    // IFileSystemインターフェース実装
    public IEnumerable<FileEntry> GetFiles();
    public void ImportFile(string sourceFilePath, string destinationFileName);
    public void ExportFile(string fileName, string destinationFilePath);
    public void DeleteFile(string fileName);
    public bool FileExists(string fileName);
    public long GetFileSize(string fileName);
    // ...
}
```

### FileSystemFactory更新

```csharp
// FileSystemFactory.GuessFileSystemTypeメソッドに追加
private bool CheckCpmSignature(IDiskContainer container)
{
    try
    {
        // CP/Mディレクトリの特徴的なパターンを検出
        var directorySector = container.ReadSector(2, 0, 1); // Track 2から開始が一般的
        
        // ディレクトリエントリのパターンマッチング
        for (int offset = 0; offset < directorySector.Length; offset += 32)
        {
            var userNumber = directorySector[offset];
            if (userNumber <= 15 || userNumber == 0xE5) // 有効なユーザー番号
            {
                // ファイル名がASCII範囲内かチェック
                bool isValidFileName = true;
                for (int i = 1; i <= 11; i++)
                {
                    var ch = directorySector[offset + i];
                    if (ch != 0x20 && (ch < 0x21 || ch > 0x7E))
                    {
                        isValidFileName = false;
                        break;
                    }
                }
                if (isValidFileName) return true;
            }
        }
    }
    catch { }
    
    return false;
}
```

## CharacterEncodingドメインへの追加

### MachineType列挙型の拡張

```csharp
public enum MachineType
{
    // 既存の値
    X1,
    Pc8801,
    Msx1,
    
    // CP/M用追加
    CpmGeneric,    // 汎用CP/M（ASCIIのみ）
    CpmPc8801,     // PC-8801のCP/M
    CpmX1,         // X1のCP/M
    CpmMsxDos      // MSX-DOS（Shift-JIS対応）
}
```

### エンコーダー実装クラス

1. **CpmCharacterEncoder** - 標準ASCII（7ビット）のみ
2. **CpmPc8801CharacterEncoder** - PC-8801文字コード対応
3. **CpmX1CharacterEncoder** - X1文字コード対応
4. **CpmMsxDosCharacterEncoder** - Shift-JIS対応

## CP/M固有の考慮事項

### 1. ユーザーエリア（User Area）
- CP/Mは0-15のユーザーエリアをサポート
- 各ユーザーエリアは独立したファイル空間
- 既存の`IFileSystem`インターフェースでの対応方法を検討

### 2. エクステント管理
- 16KBを超えるファイルは複数のディレクトリエントリ（エクステント）に分割
- エクステント番号の管理が必要

### 3. ファイル名制限
- 8.3形式（8文字のファイル名 + 3文字の拡張子）
- 大文字のみ（小文字は自動的に大文字に変換）
- 特殊文字の制限

### 4. ディスクパラメータの機種依存性
- 各機種でDPB（Disk Parameter Block）が異なる
- 一般的な構成：
  - 2DD: 40トラック、16セクタ/トラック、128バイト/セクタ
  - 2D: 40トラック、16セクタ/トラック、128バイト/セクタ
  - 2HD: 77-80トラック、可変セクタ数

## 実装優先順位

1. **Phase 1: 読み取り専用実装**
   - ディレクトリ読み取り
   - ファイル一覧表示
   - ファイルエクスポート

2. **Phase 2: 書き込み機能**
   - ファイルインポート
   - ファイル削除
   - ディレクトリエントリ管理

3. **Phase 3: 高度な機能**
   - ユーザーエリア対応
   - エクステント最適化
   - スパースファイル対応

## テスト戦略

1. **単体テスト**
   - CpmFileNameValidatorのテスト
   - ディレクトリエントリのパース/生成テスト
   - エンコーダーテスト

2. **統合テスト**
   - 実際のCP/Mディスクイメージを使用
   - 各機種のCP/Mディスクとの互換性確認
   - ファイルのインポート/エクスポート確認

## 参考資料

- CP/M 2.2 System Interface Guide
- 各機種のCP/M実装ドキュメント
- 既存のN88Basic、HuBasic実装パターン