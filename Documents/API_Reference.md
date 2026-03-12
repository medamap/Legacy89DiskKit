# Legacy89DiskKit API リファレンス

**バージョン**: v1.2.0  
**最終更新**: 2025年5月26日

## 概要

Legacy89DiskKitは、DDD（ドメイン駆動設計）アーキテクチャに基づいて設計されたライブラリです。依存性注入（DI）による疎結合な設計により、テスタビリティと拡張性を確保しています。

## 🏗️ アーキテクチャ構成

### ドメイン分離

```
Legacy89DiskKit/
├── DiskImage/               # ディスクイメージドメイン
├── FileSystem/             # ファイルシステムドメイン
└── CharacterEncoding/      # 文字エンコーディングドメイン
```

### レイヤー構成

各ドメインは以下のレイヤーで構成されています：

- **Domain**: インターフェース、ドメインモデル、例外
- **Infrastructure**: 具象実装
- **Application**: アプリケーションサービス

## 🔧 依存性注入設定

### 基本設定

```csharp
using Microsoft.Extensions.DependencyInjection;
using Legacy89DiskKit.DependencyInjection;

// サービス登録
var services = new ServiceCollection();
services.AddLegacy89DiskKit();

// サービスプロバイダー構築
var serviceProvider = services.BuildServiceProvider();
```

### 登録されるサービス

| サービス | インターフェース | 実装 | ライフタイム |
|---------|-----------------|------|-------------|
| ディスクコンテナファクトリー | `IDiskContainerFactory` | `DiskContainerFactory` | Singleton |
| ファイルシステムファクトリー | `IFileSystemFactory` | `FileSystemFactory` | Singleton |
| 文字エンコーダーファクトリー | `ICharacterEncoderFactory` | `CharacterEncoderFactory` | Singleton |
| ディスクイメージサービス | - | `DiskImageService` | Transient |
| ファイルシステムサービス | - | `FileSystemService` | Transient |
| 文字エンコーディングサービス | - | `CharacterEncodingService` | Transient |

## 📀 ディスクイメージドメイン

### IDiskContainerFactory

ディスクイメージのファクトリーインターフェース。D88形式とDSK形式をサポートします。

#### メソッド

```csharp
public interface IDiskContainerFactory
{
    // 新規ディスクイメージ作成
    IDiskContainer CreateNewDiskImage(string filePath, DiskType diskType, string diskName);
    
    // 既存ディスクイメージを開く
    IDiskContainer OpenDiskImage(string filePath, bool readOnly = false);
}
```

#### 使用例

```csharp
var factory = serviceProvider.GetRequiredService<IDiskContainerFactory>();

// 新規作成
using var newDisk = factory.CreateNewDiskImage("new.d88", DiskType.TwoD, "MY DISK");

// 既存ディスクを開く（読み取り専用）
using var existingDisk = factory.OpenDiskImage("existing.d88", readOnly: true);

// 既存ディスクを開く（読み書き）
using var writableDisk = factory.OpenDiskImage("existing.d88", readOnly: false);
```

### IDiskContainer

ディスクイメージの低レベルアクセスインターフェース。

#### プロパティ

```csharp
public interface IDiskContainer : IDisposable
{
    DiskType DiskType { get; }           // ディスクタイプ
    bool IsReadOnly { get; }             // 読み取り専用フラグ
    string FilePath { get; }             // ファイルパス
}
```

#### メソッド

```csharp
// セクタ読み取り
byte[]? ReadSector(int cylinder, int head, int sector);

// セクタ書き込み
void WriteSector(int cylinder, int head, int sector, byte[] data);

// セクタ存在チェック
bool SectorExists(int cylinder, int head, int sector);

// 変更の保存
void Flush();
```

#### 使用例

```csharp
using var container = factory.OpenDiskImage("disk.d88");

// セクタ読み取り
var bootSector = container.ReadSector(0, 0, 1);

// セクタ書き込み
var newData = new byte[256];
container.WriteSector(0, 0, 1, newData);
container.Flush();
```

### DiskType列挙体

```csharp
public enum DiskType
{
    TwoD,   // 2D: 両面倍密度 (320KB-640KB)
    TwoDD,  // 2DD: 両面倍密度 (640KB-720KB)  
    TwoHD   // 2HD: 両面高密度 (1.2MB-1.44MB)
}
```

## 🗂️ ファイルシステムドメイン

### IFileSystemFactory

ファイルシステムのファクトリーインターフェース。4つのファイルシステムをサポートします。

#### メソッド

```csharp
public interface IFileSystemFactory
{
    // 新規作成（ファイルシステム指定必須）
    IFileSystem CreateFileSystem(IDiskContainer container, FileSystemType fileSystemType);
    
    // 読み取り専用（自動検出・安全）
    IFileSystem OpenFileSystemReadOnly(IDiskContainer container);
    
    // 読み書き（ファイルシステム指定必須・安全）
    IFileSystem OpenFileSystem(IDiskContainer container, FileSystemType fileSystemType);
    
    // ファイルシステム推測（参考情報のみ）
    FileSystemType GuessFileSystemType(IDiskContainer container);
    
    // サポートするファイルシステム一覧
    IEnumerable<FileSystemType> GetSupportedFileSystemTypes();
}
```

#### 使用例

```csharp
var factory = serviceProvider.GetRequiredService<IFileSystemFactory>();

// 読み取り専用（自動検出）
var readOnlyFS = factory.OpenFileSystemReadOnly(container);

// 新規作成（ファイルシステム指定）
var newFS = factory.CreateFileSystem(container, FileSystemType.HuBasic);

// 既存ファイルシステムを開く（指定必須）
var existingFS = factory.OpenFileSystem(container, FileSystemType.MsxDos);

// ファイルシステム推測
var detectedType = factory.GuessFileSystemType(container);
Console.WriteLine($"検出されたファイルシステム: {detectedType}");
```

### FileSystemType列挙体

```csharp
public enum FileSystemType
{
    HuBasic,    // Hu-BASIC (Sharp X1)
    N88Basic,   // N88-BASIC (PC-8801)
    Fat12,      // MS-DOS FAT12 (PC汎用)
    MsxDos,     // MSX-DOS (MSX)
    Fat16,      // MS-DOS FAT16 (将来実装)
    Cpm         // CP/M (将来実装)
}
```

### IFileSystem

ファイルシステムの統一インターフェース。

#### プロパティ

```csharp
public interface IFileSystem : IDisposable
{
    IDiskContainer DiskContainer { get; }    // ディスクコンテナ
    bool IsFormatted { get; }                // フォーマット済みフラグ
}
```

#### メソッド

```csharp
// ディスク操作
void Format();                               // フォーマット
HuBasicFileSystemInfo GetFileSystemInfo();  // ファイルシステム情報
BootSector GetBootSector();                  // ブートセクタ取得
void WriteBootSector(BootSector bootSector); // ブートセクタ書き込み

// ファイル操作
IEnumerable<FileEntry> GetFiles();           // ファイル一覧取得
FileEntry? GetFile(string fileName);         // 特定ファイル取得
byte[] ReadFile(string fileName);            // ファイル読み取り
byte[] ReadFile(string fileName, bool allowPartialRead); // 部分読み取り対応
void WriteFile(string fileName, byte[] data, bool isText = false, 
               ushort loadAddress = 0, ushort execAddress = 0); // ファイル書き込み
void DeleteFile(string fileName);            // ファイル削除
```

#### 使用例

```csharp
// ファイルシステム情報取得
var info = fileSystem.GetFileSystemInfo();
Console.WriteLine($"空きクラスタ: {info.FreeClusters}/{info.TotalClusters}");

// ファイル一覧表示
var files = fileSystem.GetFiles();
foreach (var file in files)
{
    Console.WriteLine($"{file.FileName}.{file.Extension} ({file.Size} bytes)");
}

// ファイル読み取り
var data = fileSystem.ReadFile("README.TXT");
var text = System.Text.Encoding.UTF8.GetString(data);

// 破損ファイルの部分復旧
try
{
    var corruptedData = fileSystem.ReadFile("damaged.txt", allowPartialRead: true);
}
catch (FileSystemException ex)
{
    Console.WriteLine($"復旧に失敗: {ex.Message}");
}

// ファイル書き込み
var newData = System.Text.Encoding.UTF8.GetBytes("Hello World!");
fileSystem.WriteFile("hello.txt", newData, isText: true);

// バイナリファイル書き込み
var binaryData = File.ReadAllBytes("program.bin");
fileSystem.WriteFile("program.com", binaryData, isText: false, 
                    loadAddress: 0x8000, execAddress: 0x8000);

// ファイル削除
fileSystem.DeleteFile("old.txt");
```

### FileEntry

ファイル情報を表すレコード型。

```csharp
public record FileEntry(
    string FileName,                    // ファイル名
    string Extension,                   // 拡張子
    HuBasicFileMode Mode,              // ファイルモード
    HuBasicFileAttributes Attributes,  // 属性
    int Size,                          // サイズ
    ushort LoadAddress,                // ロードアドレス
    ushort ExecuteAddress,             // 実行アドレス
    DateTime ModifiedDate,             // 更新日時
    bool IsProtected                   // 保護フラグ
);
```

### HuBasicFileSystemInfo

ファイルシステム情報を表すレコード型。

```csharp
public record HuBasicFileSystemInfo(
    int TotalClusters,     // 総クラスタ数
    int FreeClusters,      // 空きクラスタ数
    int ClusterSize,       // クラスタサイズ
    int SectorSize         // セクタサイズ
);
```

### BootSector

ブートセクタ情報を表すレコード型。

```csharp
public record BootSector(
    bool IsBootable,         // ブート可能フラグ
    string Label,           // ラベル
    string Extension,       // 拡張子
    int Size,               // サイズ
    ushort LoadAddress,     // ロードアドレス
    ushort ExecuteAddress,  // 実行アドレス
    DateTime ModifiedDate,  // 更新日時
    ushort StartSector      // 開始セクタ
);
```

## 🔤 文字エンコーディングドメイン

### CharacterEncodingService

機種別文字エンコーディングサービス。

#### メソッド

```csharp
public class CharacterEncodingService
{
    // テキストエンコード
    public byte[] EncodeText(string unicodeText, MachineType machineType);
    
    // テキストデコード
    public string DecodeText(byte[] machineBytes, MachineType machineType);
}
```

#### 使用例

```csharp
var encodingService = serviceProvider.GetRequiredService<CharacterEncodingService>();

// X1文字コードでエンコード
var x1Bytes = encodingService.EncodeText("こんにちは、X1!", MachineType.X1);

// PC-8801文字コードでエンコード
var pc8801Bytes = encodingService.EncodeText("Hello PC-8801!", MachineType.Pc8801);

// MSX文字コードでエンコード
var msxBytes = encodingService.EncodeText("MSX World!", MachineType.Msx1);

// デコード
var unicodeText = encodingService.DecodeText(x1Bytes, MachineType.X1);
```

### MachineType列挙体

対応する18機種の定義。

```csharp
public enum MachineType
{
    // Sharp
    X1,           // ✅ 完全実装
    X1Turbo,      // ✅ 完全実装
    
    // NEC
    Pc8801,       // 🟡 基本ASCII
    Pc8801Mk2,    // 🟡 基本ASCII
    Pc8001,       // 🟡 基本ASCII
    Pc8001Mk2,    // 🟡 基本ASCII
    Pc6001,       // 🟡 基本ASCII
    Pc6601,       // 🟡 基本ASCII
    
    // MSX
    Msx1,         // 🟡 基本ASCII
    Msx2,         // 🟡 基本ASCII
    
    // Sharp MZ
    Mz80k,        // 🟡 基本ASCII
    Mz700,        // 🟡 基本ASCII
    Mz1500,       // 🟡 基本ASCII
    Mz2500,       // 🟡 基本ASCII
    
    // 富士通
    Fm7,          // 🟡 基本ASCII
    Fm77,         // 🟡 基本ASCII
    Fm77av,       // 🟡 基本ASCII
    
    // 任天堂
    Fc            // 🟡 基本ASCII
}
```

## 🛡️ エラーハンドリング

### 例外階層

```
System.Exception
├── DiskImageException          # ディスクイメージ関連エラー
├── FileSystemException         # ファイルシステム関連エラー
├── CharacterEncodingException  # 文字エンコーディング関連エラー
└── FileNotFoundException       # ファイル未発見エラー
```

### 使用例

```csharp
try
{
    using var container = diskFactory.OpenDiskImage("disk.d88");
    var fileSystem = fsFactory.OpenFileSystem(container, FileSystemType.HuBasic);
    var data = fileSystem.ReadFile("test.txt");
}
catch (DiskImageException ex)
{
    Console.WriteLine($"ディスクイメージエラー: {ex.Message}");
    // D88ファイル破損、読み取りエラー等
}
catch (FileSystemException ex)
{
    Console.WriteLine($"ファイルシステムエラー: {ex.Message}");
    // 未フォーマット、構造不正等
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"ファイルが見つかりません: {ex.FileName}");
    // 指定ファイルが存在しない
}
catch (CharacterEncodingException ex)
{
    Console.WriteLine($"文字エンコーディングエラー: {ex.Message}");
    // 文字変換失敗等
}
```

## 🔒 安全性機能

### ReadOnlyFileSystemWrapper

読み取り専用ラッパー。すべての書き込み操作を禁止します。

```csharp
// 自動的にReadOnlyWrapperが適用される
var readOnlyFS = factory.OpenFileSystemReadOnly(container);

// 読み取り操作は正常動作
var files = readOnlyFS.GetFiles();
var data = readOnlyFS.ReadFile("test.txt");

// 書き込み操作は例外発生
try
{
    readOnlyFS.WriteFile("new.txt", data);  // InvalidOperationException
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("読み取り専用モードでは書き込みできません");
}
```

### 構造検証

ファイルシステム指定時の構造検証。

```csharp
try
{
    // 指定されたファイルシステムで構造検証を実行
    var fileSystem = factory.OpenFileSystem(container, FileSystemType.Fat12);
}
catch (FileSystemException ex)
{
    Console.WriteLine($"構造検証エラー: {ex.Message}");
    // "Disk is not a valid Fat12 filesystem. Use 'info' command to detect actual filesystem type."
}
```

## 📊 実装状況マトリックス

### ファイルシステム対応状況

| 機能 | Hu-BASIC | N88-BASIC | FAT12 | MSX-DOS |
|------|----------|-----------|-------|---------|
| **作成・フォーマット** | ✅ | ✅ | ✅ | ✅ |
| **ファイル読み書き** | ✅ | ✅ | ✅ | ✅ |
| **自動検出** | ✅ | ✅ | ✅ | ✅ |
| **破損復旧** | ✅ | ✅ | ✅ | ✅ |
| **対応ディスクタイプ** | 2D/2DD/2HD | 2D/2DD | 2D/2DD/2HD | 2DD |

### 文字エンコーディング対応状況

| 機種 | 実装状況 | 対応範囲 |
|------|---------|---------|
| **X1/X1Turbo** | ✅ 完全実装 | ひらがな・カタカナ・漢字・グラフィック文字 |
| **PC-8801系** | 🟡 基本ASCII | ASCII文字のみ |
| **MSX系** | 🟡 基本ASCII | ASCII文字のみ |
| **MZ系** | 🟡 基本ASCII | ASCII文字のみ |
| **FM系** | 🟡 基本ASCII | ASCII文字のみ |
| **FC** | 🟡 基本ASCII | ASCII文字のみ |

## 🔍 高度な使用例

### カスタムディスク操作

```csharp
// 複数ファイルシステムでの操作
using var container = diskFactory.OpenDiskImage("multi.d88");

// ファイルシステム自動検出
var detectedType = fsFactory.GuessFileSystemType(container);
Console.WriteLine($"検出: {detectedType}");

// 読み取り専用で安全に開く
var readOnlyFS = fsFactory.OpenFileSystemReadOnly(container);
var files = readOnlyFS.GetFiles();

// 別のファイルシステムとして試す
try
{
    var alternateFS = fsFactory.OpenFileSystem(container, FileSystemType.Fat12);
    var fat12Files = alternateFS.GetFiles();
}
catch (FileSystemException)
{
    Console.WriteLine("FAT12として読み取りできませんでした");
}
```

### 破損ディスクからの復旧

```csharp
using var container = diskFactory.OpenDiskImage("damaged.d88", readOnly: true);
var fileSystem = fsFactory.OpenFileSystemReadOnly(container);

foreach (var file in fileSystem.GetFiles())
{
    try
    {
        // 通常読み取りを試行
        var data = fileSystem.ReadFile($"{file.FileName}.{file.Extension}");
        File.WriteAllBytes($"recovered_{file.FileName}.{file.Extension}", data);
        Console.WriteLine($"✅ {file.FileName} 完全復旧");
    }
    catch (FileSystemException)
    {
        try
        {
            // 部分復旧を試行
            var partialData = fileSystem.ReadFile($"{file.FileName}.{file.Extension}", 
                                                 allowPartialRead: true);
            File.WriteAllBytes($"partial_{file.FileName}.{file.Extension}", partialData);
            Console.WriteLine($"⚠️ {file.FileName} 部分復旧");
        }
        catch (FileSystemException ex)
        {
            Console.WriteLine($"❌ {file.FileName} 復旧失敗: {ex.Message}");
        }
    }
}
```

### 機種別文字エンコーディング一括変換

```csharp
var encodingService = serviceProvider.GetRequiredService<CharacterEncodingService>();

// 各機種用にテキストファイルを変換
var sourceText = "こんにちは、レトロコンピュータ！";
var machines = new[] { MachineType.X1, MachineType.Pc8801, MachineType.Msx1 };

foreach (var machine in machines)
{
    try
    {
        var encoded = encodingService.EncodeText(sourceText, machine);
        var decoded = encodingService.DecodeText(encoded, machine);
        
        Console.WriteLine($"{machine}: {decoded}");
        File.WriteAllBytes($"text_{machine}.dat", encoded);
    }
    catch (CharacterEncodingException ex)
    {
        Console.WriteLine($"{machine}: エンコードエラー - {ex.Message}");
    }
}
```

## 🚀 パフォーマンス考慮事項

### メモリ使用量制限

- **ファイル読み取り**: 最大10MB制限
- **クラスタチェーン**: 最大4000クラスタ制限
- **ディレクトリエントリ**: 最大500エントリ制限

### 推奨使用パターン

```csharp
// ✅ 推奨：usingステートメントでリソース管理
using var container = diskFactory.OpenDiskImage("large.d88");
using var fileSystem = fsFactory.OpenFileSystem(container, FileSystemType.HuBasic);

// ✅ 推奨：読み取り専用での操作
var readOnlyFS = fsFactory.OpenFileSystemReadOnly(container);

// ❌ 非推奨：手動でのリソース管理
var container = diskFactory.OpenDiskImage("disk.d88");
// ... container.Dispose()を忘れる可能性
```

## 📝 バージョン互換性

### v1.2.0での変更点

- ✅ **MSX-DOSファイルシステム追加**
- ✅ **組成パターン導入** (MsxDosFileSystem)
- ✅ **自動検出ロジック強化**

### v1.1.0での変更点

- ✅ **N88-BASICファイルシステム追加**
- ✅ **16バイトディレクトリエントリ対応**

### v1.0.0での変更点

- ✅ **安全性強化**: 書き込み時ファイルシステム指定必須
- ✅ **ReadOnlyFileSystemWrapper導入**
- ✅ **文字エンコーディングドメイン追加**

---

*このAPIリファレンスは Legacy89DiskKit v1.2.0 に基づいています。*  
*最新情報は [GitHub リポジトリ](https://github.com/yourusername/Legacy89DiskKit) をご確認ください。*