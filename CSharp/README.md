# Legacy89DiskKit - C# Implementation

レトロコンピュータのディスクイメージ（D88/DSK形式）を操作するためのC#ライブラリとCLIツールです。

## 対応システム

- **Sharp X1** - Hu-BASIC ファイルシステム
- **PC-8801** - N88-BASIC ファイルシステム
- **FAT12** - MS-DOS互換ファイルシステム

## 機能

- D88/DSK形式ディスクイメージの作成・読み込み（2D/2DD/2HD対応）
- 複数ファイルシステムのフォーマット・操作
- ファイルの入出力（テキスト・バイナリ）
- ブートセクタの操作
- ファイル一覧表示・削除
- 文字エンコーディング変換（X1/PC-8801/MSX1対応）

## プロジェクト構成

### ライブラリ (Legacy89DiskKit)

DDDアーキテクチャに基づいて設計されています：

```
Legacy89DiskKit/
├── DiskImage/                    # ディスクイメージドメイン
│   ├── Domain/
│   │   ├── Interface/Container/  # IDiskContainer インターフェイス
│   │   └── Exception/           # ドメイン例外
│   ├── Infrastructure/Container/ # D88DiskContainer 実装
│   └── Application/             # DiskImageService
└── FileSystem/                  # ファイルシステムドメイン
    ├── Domain/
    │   ├── Interface/FileSystem/ # IFileSystem インターフェイス
    │   └── Exception/           # ドメイン例外
    ├── Infrastructure/
    │   ├── FileSystem/          # HuBasic, N88Basic, Fat12 実装
    │   ├── Factory/             # FileSystemFactory
    │   └── Utility/             # 文字コード変換・ファイル名検証
    └── Application/             # FileSystemService
├── CharacterEncoding/           # 文字エンコーディングドメイン
│   ├── Domain/Interface/        # ICharacterEncoder インターフェイス
│   ├── Infrastructure/Encoder/  # X1, PC-8801, MSX1 エンコーダ
│   └── Application/             # CharacterEncodingService
```

### CLIツール (Legacy89DiskKit.CLI)

コマンドラインからディスクイメージを操作できるツールです。

## 使用方法

### CLIツール

#### ディスクイメージの作成
```bash
Legacy89DiskKit.CLI create disk.d88 2D "My Disk"
```

#### フォーマット
```bash
# ファイルシステム自動判定
Legacy89DiskKit.CLI format disk.d88

# ファイルシステム明示指定
Legacy89DiskKit.CLI format disk.d88 --filesystem HuBasic
Legacy89DiskKit.CLI format disk.d88 --filesystem N88Basic  
Legacy89DiskKit.CLI format disk.d88 --filesystem Fat12
```

#### ファイル一覧表示
```bash
Legacy89DiskKit.CLI list disk.d88
```

#### テキストファイルの入出力
```bash
# インポート（文字エンコーディング指定可能）
Legacy89DiskKit.CLI import-text disk.d88 readme.txt README.TXT --encoding X1
Legacy89DiskKit.CLI import-text disk.d88 readme.txt README.TXT --encoding Pc8801
Legacy89DiskKit.CLI import-text disk.d88 readme.txt README.TXT --encoding Msx1

# エクスポート
Legacy89DiskKit.CLI export-text disk.d88 README.TXT readme_export.txt --encoding X1
```

#### バイナリファイルの入出力
```bash
# インポート (ロード・実行アドレス指定可能)
Legacy89DiskKit.CLI import-binary disk.d88 program.bin PROGRAM.BIN 8000 8000

# エクスポート
Legacy89DiskKit.CLI export-binary disk.d88 PROGRAM.BIN program_export.bin
```

#### ブートセクタの操作
```bash
# インポート
Legacy89DiskKit.CLI import-boot disk.d88 boot.bin "BOOT"

# エクスポート（情報をテキストで出力）
Legacy89DiskKit.CLI export-boot disk.d88 bootinfo.txt
```

#### その他
```bash
# ファイル削除
Legacy89DiskKit.CLI delete disk.d88 OLDFILE.TXT

# ディスク情報表示
Legacy89DiskKit.CLI info disk.d88

# ヘルプ表示
Legacy89DiskKit.CLI help
```

### ライブラリ使用例

```csharp
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Application;

// ディスクイメージサービスの初期化
var diskService = new DiskImageService();
var fileService = new FileSystemService();

// 新しいディスクイメージを作成
using var container = diskService.CreateNewDiskImage("new.d88", DiskType.TwoD, "NEW DISK");

// ファイルシステムをフォーマット
fileService.FormatDisk(container);

// ファイルシステムを開く
var fileSystem = fileService.OpenFileSystem(container);

// ファイル一覧を取得
var files = fileService.ListFiles(fileSystem);
foreach (var file in files)
{
    Console.WriteLine($"{file.FileName}.{file.Extension} - {file.Size} bytes");
}

// テキストファイルをインポート
fileService.ImportTextFile(fileSystem, "host.txt", "IMPORT.TXT");

// 既存のディスクイメージを開く
using var existingContainer = diskService.OpenDiskImage("existing.d88", readOnly: true);
var existingFileSystem = fileService.OpenFileSystem(existingContainer);

// ファイルを読み込み
var fileData = existingFileSystem.ReadFile("README.TXT");
```

## ディスクタイプ

- **2D**: 320KB (40トラック × 2面 × 16セクタ × 256バイト)
- **2DD**: 640KB (80トラック × 2面 × 16セクタ × 256バイト) 
- **2HD**: 1.2MB (77トラック × 2面 × 26セクタ × 256バイト)

## ファイルシステム対応状況

| 機能 | Hu-BASIC | N88-BASIC | FAT12 |
|------|----------|-----------|-------|
| ディスク作成・フォーマット | ✅ | ✅ | ✅ |
| ファイル読み取り | ✅ | ✅ | ✅ |
| ファイル書き込み | ✅ | ✅ | ✅ |
| ファイル削除 | ✅ | ✅ | ✅ |
| ディスク情報取得 | ✅ | ✅ | ✅ |
| ブートセクタ操作 | ✅ | ✅ | ✅ |

## 文字エンコーディング

各システム固有の文字コードとUnicodeの相互変換をサポートしています：

- **X1**: JIS X 0201 + Sharp X1固有文字
- **PC-8801**: JIS X 0201 + PC-8801固有文字  
- **MSX1**: JIS X 0201 + MSX固有文字

## 制限事項

- ディレクトリはサポートされていません（ルートディレクトリのみ）
- マルチバイト文字（漢字等）はサポートされていません
- DSK形式での2HD以外のサポートは制限があります

## ビルド

.NET 8.0 SDK が必要です：

```bash
cd CSharp
dotnet build
dotnet run --project Legacy89DiskKit.CLI
```

## ライセンス

このプロジェクトのライセンスについては、プロジェクトルートのLICENSEファイルを参照してください。