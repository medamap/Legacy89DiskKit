# Legacy89DiskKit - C# Implementation

Sharp X1 Hu-BASIC ディスクイメージ（D88形式）を操作するためのC#ライブラリとCLIツールです。

## 機能

- D88形式ディスクイメージの作成・読み込み（2D/2DD/2HD対応）
- Hu-BASICファイルシステムのフォーマット
- ファイルの入出力（テキスト・バイナリ）
- ブートセクタの操作
- ファイル一覧表示・削除

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
    │   ├── FileSystem/          # HuBasicFileSystem 実装
    │   └── Utility/             # X1文字コード変換
    └── Application/             # FileSystemService
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
Legacy89DiskKit.CLI format disk.d88
```

#### ファイル一覧表示
```bash
Legacy89DiskKit.CLI list disk.d88
```

#### テキストファイルの入出力
```bash
# インポート
Legacy89DiskKit.CLI import-text disk.d88 readme.txt README.TXT

# エクスポート
Legacy89DiskKit.CLI export-text disk.d88 README.TXT readme_export.txt
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

## 文字コード

X1固有の文字コード（JIS X 0201 + カタカナ・記号）とUnicodeの相互変換をサポートしています。

## 制限事項

- 現在の実装では、ファイルの書き込み・削除機能は部分的な実装です
- ディレクトリはサポートされていません（ルートディレクトリのみ）
- マルチバイト文字（漢字等）はサポートされていません

## ビルド

.NET 8.0 SDK が必要です：

```bash
cd CSharp
dotnet build
dotnet run --project Legacy89DiskKit.CLI
```

## ライセンス

このプロジェクトのライセンスについては、プロジェクトルートのLICENSEファイルを参照してください。