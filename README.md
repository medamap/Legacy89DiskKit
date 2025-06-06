# Legacy89DiskKit

[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Latest-239120?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat)](LICENSE)
[![Version](https://img.shields.io/badge/Version-v1.6.0-blue?style=flat)](https://github.com/yourusername/Legacy89DiskKit/releases)

📀 **Legacy89DiskKit** は、1980〜90年代の日本のレトロコンピュータ（Sharp X1、PC-8801、MSX等）で使用されていたディスクフォーマットを現代的な環境で扱うためのC#ライブラリ・CLIツールセットです。

## ✨ 特徴

### 🎯 対応フォーマット (v1.6.0)

**完全対応ファイルシステム**:
- ✅ **Hu-BASIC** (Sharp X1) - 完全実装
- ✅ **N88-BASIC** (PC-8801) - 完全実装  
- ✅ **MS-DOS FAT12** (汎用PC) - 完全実装
- ✅ **MSX-DOS** (MSX) - 完全実装
- ✅ **CP/M 2.2** (汎用) - 完全実装
- ✅ **CDOS (Club DOS)** - 完全実装

**対応ディスクイメージ**:
- ✅ **D88形式** - Sharp/NECディスクイメージ標準
- ✅ **DSK形式** - PC汎用ディスクイメージ
- ✅ **2D形式** - Plain Disk Image (320KB固定) ← **NEW in v1.6.0!**

**文字エンコーディング**: **18機種対応** (X1完全実装、他機種基本ASCII)

**対応ディスクタイプ**: 2D (320KB) / 2DD (720KB) / 2HD (1.2MB)

### 🎮 インタラクティブシェル
- **対話型操作**: リアルタイムフィードバックで直感的な操作
- **マルチスロット**: 最大10個のディスクを同時管理
- **Tab補完**: コマンド・ファイルパスの自動補完
- **クロスディスク操作**: ディスク間でのファイルコピー・移動

### 🏗️ モダンアーキテクチャ
- **DDD（ドメイン駆動設計）**: 拡張性・保守性を重視した設計
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection採用
- **Factory Pattern**: ディスクコンテナ・ファイルシステムの抽象化
- **組成パターン**: コード再利用による効率的な実装
- **レイヤー分離**: ドメイン・インフラ・アプリケーション層の明確な分離

### 🛡️ 堅牢性
- **破損ディスク対応**: 部分的なデータ復旧機能
- **メモリ安全**: 大容量ファイルの安全な処理（10MB制限）
- **エラー検出**: 循環参照検出、詳細な診断情報
- **入力検証**: 各ファイルシステム固有の名前規則対応
- **データ安全**: 書き込み時のファイルシステム指定必須化
- **誤操作防止**: 構造検証による不正アクセス阻止

## ⚠️ 重要な安全性について

**🔒 データ保護強化**: ファイルシステムの誤判定による**データ破損を防止**するため、**すべての書き込み操作**で `--filesystem` パラメータの指定が**必須**になりました。

### 安全な操作パターン

```bash
# ✅ 安全：読み取り専用操作（自動検出OK）
./CLI list disk.d88                    # ファイル一覧表示
./CLI info disk.d88                    # ディスク情報表示
./CLI recover-text disk.d88 src dst    # 破損ファイル復旧

# ⚠️ 必須：書き込み操作（ファイルシステム指定必須）
./CLI export-text disk.d88 src dst --filesystem hu-basic    # ✅ 正しい
./CLI import-text disk.d88 src dst --filesystem msx-dos     # ✅ 正しい
./CLI export-text disk.d88 src dst                          # ❌ エラー
```

## 🚀 クイックスタート

### インストール

```bash
# リポジトリのクローン
git clone https://github.com/yourusername/Legacy89DiskKit.git
cd Legacy89DiskKit/CSharp

# ビルド
dotnet build

# CLIツールの実行
dotnet run --project Legacy89DiskKit.CLI -- help
```

### 基本的な使用例

#### 🎮 インタラクティブシェルモード（NEW!）

```bash
# インタラクティブシェルを起動
dotnet run --project Legacy89DiskKit.CLI -- shell

# シェル内での操作例
Legacy89DiskKit [0:Empty]> open ~/disks/game.d88
Legacy89DiskKit [0:game.d88/HuBasic]> list
Legacy89DiskKit [0:game.d88/HuBasic]> open ~/disks/work.d88 1
Legacy89DiskKit [0:game.d88/HuBasic]> copy GAME.BAS 1:
Legacy89DiskKit [0:game.d88/HuBasic]> export-text README.TXT ~/desktop/readme.txt
```

#### 📝 従来のコマンドライン操作

```bash
# 新しいディスクイメージを作成
dotnet run --project Legacy89DiskKit.CLI -- create mydisk.d88 2D "MY DISK"

# ディスクをフォーマット（ファイルシステム指定）
dotnet run --project Legacy89DiskKit.CLI -- format mydisk.d88 --filesystem hu-basic

# ファイル一覧表示（自動検出・読み取り専用）
dotnet run --project Legacy89DiskKit.CLI -- list mydisk.d88

# ディスク情報表示（ファイルシステム推測）
dotnet run --project Legacy89DiskKit.CLI -- info disk.d88

# 安全な書き込み操作（ファイルシステム指定必須）
dotnet run --project Legacy89DiskKit.CLI -- import-text mydisk.d88 readme.txt README.TXT --filesystem hu-basic --machine x1
dotnet run --project Legacy89DiskKit.CLI -- export-text mydisk.d88 README.TXT readme.txt --filesystem hu-basic --machine x1
```

## 📋 CLIコマンド詳細

### 基本コマンド

| コマンド | 説明 | 必須パラメータ | オプション |
|---------|------|---------------|-----------|
| `create` | 新しいディスクイメージ作成 | `<disk-file> <type> <name>` | - |
| `format` | ディスクフォーマット | `<disk-file> --filesystem <type>` | - |
| `list` | ファイル一覧表示 | `<disk-file>` | `--filesystem <type>` |
| `info` | ディスク情報表示 | `<disk-file>` | - |

### ファイル操作コマンド

| コマンド | 説明 | 必須パラメータ | オプション |
|---------|------|---------------|-----------|
| `import-text` | テキストファイル書き込み | `<disk-file> <host-file> <disk-name> --filesystem <type>` | `--machine <machine>` |
| `export-text` | テキストファイル読み出し | `<disk-file> <disk-name> <host-file> --filesystem <type>` | `--machine <machine>` |
| `import-binary` | バイナリファイル書き込み | `<disk-file> <host-file> <disk-name>` | `[load-addr] [exec-addr]` |
| `export-binary` | バイナリファイル読み出し | `<disk-file> <disk-name> <host-file>` | - |
| `delete` | ファイル削除 | `<disk-file> <disk-name> --filesystem <type>` | - |

### システム操作コマンド

| コマンド | 説明 | 必須パラメータ | オプション |
|---------|------|---------------|-----------|
| `import-boot` | ブートセクタ書き込み | `<disk-file> <host-file> <label>` | - |
| `export-boot` | ブートセクタ情報出力 | `<disk-file> <output-file>` | - |

### 復旧コマンド

| コマンド | 説明 | 必須パラメータ | 注意 |
|---------|------|---------------|------|
| `recover-text` | 破損テキストファイル復旧 | `<disk-file> <disk-name> <host-file>` | 読み取り専用・自動検出 |
| `recover-binary` | 破損バイナリファイル復旧 | `<disk-file> <disk-name> <host-file>` | 読み取り専用・自動検出 |

### パラメータ詳細

#### ディスクタイプ (`<type>`)
| 値 | 説明 | 容量 | 用途 |
|----|------|------|------|
| `2D` | 両面倍密度 | 320KB-640KB | Sharp X1, PC-8801 |
| `2DD` | 両面倍密度 | 640KB-720KB | 汎用, MSX |
| `2HD` | 両面高密度 | 1.2MB-1.44MB | PC汎用 |

#### ファイルシステム (`--filesystem <type>`)
| 値 | 説明 | 対応機種 | 対応ディスクタイプ |
|----|------|----------|------------------|
| `hu-basic` | Hu-BASICファイルシステム | Sharp X1 | 2D, 2DD, 2HD |
| `n88-basic` | N88-BASICファイルシステム | PC-8801 | 2D, 2DD |
| `fat12` | MS-DOS FAT12ファイルシステム | PC汎用 | 2D, 2DD, 2HD |
| `msx-dos` | MSX-DOSファイルシステム | MSX | 2DD |

#### 機種名 (`--machine <machine>`)
| カテゴリ | 対応値 | 実装状況 |
|---------|-------|---------|
| **Sharp** | `x1`, `x1turbo` | ✅ 完全実装 |
| **NEC** | `pc8801`, `pc8801mk2`, `pc8001`, `pc8001mk2`, `pc6001`, `pc6601` | 🟡 基本ASCII |
| **MSX** | `msx1`, `msx2` | 🟡 基本ASCII |
| **Sharp MZ** | `mz80k`, `mz700`, `mz1500`, `mz2500` | 🟡 基本ASCII |
| **富士通** | `fm7`, `fm77`, `fm77av` | 🟡 基本ASCII |
| **任天堂** | `fc` | 🟡 基本ASCII |

#### 使用例

```bash
# Hu-BASICディスク作成・操作
./CLI create x1disk.d88 2D "X1 DISK"
./CLI format x1disk.d88 --filesystem hu-basic
./CLI import-text x1disk.d88 hello.txt HELLO.TXT --filesystem hu-basic --machine x1

# MSX-DOSディスク作成・操作  
./CLI create msxdisk.d88 2DD "MSX DISK"
./CLI format msxdisk.d88 --filesystem msx-dos
./CLI import-text msxdisk.d88 readme.txt README.TXT --filesystem msx-dos --machine msx1

# N88-BASICディスク操作
./CLI list pc88disk.d88 --filesystem n88-basic
./CLI export-text pc88disk.d88 PROGRAM.BAS program.bas --filesystem n88-basic --machine pc8801

# MS-DOS FAT12ディスク操作
./CLI create pcdisk.dsk 2HD "PC DISK"  
./CLI format pcdisk.dsk --filesystem fat12
./CLI import-binary pcdisk.dsk program.exe PROGRAM.EXE 
```

## 🔧 ライブラリAPI詳細

### 基本的な使用方法

```csharp
using Microsoft.Extensions.DependencyInjection;
using Legacy89DiskKit.DependencyInjection;

// DI設定
var services = new ServiceCollection();
services.AddLegacy89DiskKit();
var serviceProvider = services.BuildServiceProvider();
```

### ファクトリーサービス

```csharp
// ファクトリー取得
var diskFactory = serviceProvider.GetRequiredService<IDiskContainerFactory>();
var fsFactory = serviceProvider.GetRequiredService<IFileSystemFactory>();
var encodingService = serviceProvider.GetRequiredService<CharacterEncodingService>();
```

### ディスクイメージ操作

```csharp
// 既存ディスクを開く（読み取り専用・自動検出）
using var container = diskFactory.OpenDiskImage("disk.d88", readOnly: true);
var fileSystem = fsFactory.OpenFileSystemReadOnly(container);

// 新規ディスクイメージ作成
using var newContainer = diskFactory.CreateNewDiskImage("new.d88", DiskType.TwoD, "NEW DISK");
var newFileSystem = fsFactory.CreateFileSystem(newContainer, FileSystemType.HuBasic);
newFileSystem.Format();

// 既存ディスクへの書き込み（ファイルシステム指定必須）
using var writeContainer = diskFactory.OpenDiskImage("disk.d88", readOnly: false);
var writeFileSystem = fsFactory.OpenFileSystem(writeContainer, FileSystemType.HuBasic);
```

### ファイル操作

```csharp
// ファイル一覧取得
var files = fileSystem.GetFiles();
foreach (var file in files)
{
    Console.WriteLine($"{file.FileName}.{file.Extension} ({file.Size} bytes, {file.Mode})");
}

// ファイル読み込み
var data = fileSystem.ReadFile("README.TXT");

// 破損ファイルの部分復旧
var partialData = fileSystem.ReadFile("damaged.txt", allowPartialRead: true);

// ファイル書き込み
var textData = System.Text.Encoding.UTF8.GetBytes("Hello World!");
writeFileSystem.WriteFile("hello.txt", textData, isText: true);

// バイナリファイル書き込み
var binaryData = File.ReadAllBytes("program.bin");
writeFileSystem.WriteFile("program.bin", binaryData, isText: false, loadAddress: 0x8000, execAddress: 0x8000);

// ファイル削除
writeFileSystem.DeleteFile("oldfile.txt");
```

### 文字エンコーディング

```csharp
// 機種別文字エンコーディング
var x1Bytes = encodingService.EncodeText("こんにちは、X1!", MachineType.X1);
var pc8801Bytes = encodingService.EncodeText("Hello PC-8801!", MachineType.Pc8801);
var msxBytes = encodingService.EncodeText("MSX World!", MachineType.Msx1);

// デコード
var unicodeText = encodingService.DecodeText(x1Bytes, MachineType.X1);
```

### エラーハンドリング

```csharp
try
{
    var fileSystem = fsFactory.OpenFileSystem(container, FileSystemType.HuBasic);
    var data = fileSystem.ReadFile("test.txt");
}
catch (FileSystemException ex)
{
    Console.WriteLine($"ファイルシステムエラー: {ex.Message}");
}
catch (DiskImageException ex)
{
    Console.WriteLine($"ディスクイメージエラー: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"ファイルが見つかりません: {ex.FileName}");
}
```

> **📚 詳細なAPIリファレンス**: [Documents/API_Reference.md](Documents/API_Reference.md)

## 💾 対応ディスクタイプ詳細

### Sharp X1 (D88 + Hu-BASIC)
| タイプ | 容量 | ジオメトリ | 用途 |
|-------|------|-----------|------|
| **2D** | 320KB | 40×2×16×256B | X1標準 |
| **2DD** | 640KB | 80×2×16×256B | X1拡張 |
| **2HD** | 1.2MB | 77×2×26×256B | X1 Turbo |

### PC-8801 (D88 + N88-BASIC)
| タイプ | 容量 | ジオメトリ | 用途 |
|-------|------|-----------|------|
| **2D** | 320KB | 40×2×16×256B | PC-8801標準 |
| **2DD** | 640KB | 80×2×16×256B | PC-8801拡張 |

### MSX (D88/DSK + MSX-DOS)
| タイプ | 容量 | ジオメトリ | 用途 |
|-------|------|-----------|------|
| **2DD** | 720KB | 80×2×9×512B | MSX-DOS標準 |

### PC汎用 (DSK + MS-DOS FAT12)
| タイプ | 容量 | ジオメトリ | 説明 |
|-------|------|-----------|------|
| **5.25" DD** | 360KB | 40×2×9×512B | PC初期フロッピー |
| **3.5" DD** | 720KB | 80×2×9×512B | PC標準フロッピー |
| **5.25" HD** | 1.2MB | 80×2×15×512B | PC高密度フロッピー |
| **3.5" HD** | 1.44MB | 80×2×18×512B | PC標準HD |

### 🔄 クロス対応マトリックス

| ディスクイメージ＼ファイルシステム | Hu-BASIC | N88-BASIC | FAT12 | MSX-DOS | CP/M |
|----------------------------------|----------|-----------|-------|---------|------|
| **D88** | ✅ ネイティブ | ✅ ネイティブ | ✅ 対応 | ✅ 対応 | ✅ 対応 |
| **DSK** | ✅ 対応 | ❌ 非対応 | ✅ ネイティブ | ✅ 対応 | ✅ 対応 |

## 🛠️ アーキテクチャ詳細

### ドメイン構造

```
Legacy89DiskKit/
├── DependencyInjection/           # DI設定
│   └── ServiceCollectionExtensions.cs
│
├── DiskImage/                     # ディスクイメージドメイン
│   ├── Domain/
│   │   ├── Interface/
│   │   │   ├── Container/         # IDiskContainer
│   │   │   └── Factory/           # IDiskContainerFactory
│   │   └── Exception/             # DiskImageException
│   ├── Infrastructure/
│   │   ├── Container/
│   │   │   ├── D88DiskContainer.cs    # D88形式実装
│   │   │   └── DskDiskContainer.cs    # DSK形式実装
│   │   └── Factory/               # DiskContainerFactory
│   └── Application/               # DiskImageService
│
├── FileSystem/                    # ファイルシステムドメイン  
│   ├── Domain/
│   │   ├── Interface/
│   │   │   ├── FileSystem/        # IFileSystem
│   │   │   └── Factory/           # IFileSystemFactory
│   │   ├── Model/                 # ドメインモデル
│   │   │   ├── MsxDosConfiguration.cs
│   │   │   ├── MsxDosBootSector.cs
│   │   │   ├── N88BasicFileEntry.cs
│   │   │   └── N88BasicConfiguration.cs
│   │   └── Exception/             # FileSystemException
│   ├── Infrastructure/
│   │   ├── FileSystem/
│   │   │   ├── HuBasicFileSystem.cs    # Hu-BASIC実装
│   │   │   ├── N88BasicFileSystem.cs   # N88-BASIC実装
│   │   │   ├── Fat12FileSystem.cs      # MS-DOS FAT12実装  
│   │   │   ├── MsxDosFileSystem.cs     # MSX-DOS実装
│   │   │   └── ReadOnlyFileSystemWrapper.cs
│   │   ├── Factory/               # FileSystemFactory
│   │   └── Utility/               # ユーティリティ
│   │       ├── X1Converter.cs         # X1文字コード変換
│   │       ├── HuBasicFileNameValidator.cs
│   │       ├── N88BasicFileNameValidator.cs
│   │       └── MsxDosFileNameValidator.cs
│   └── Application/               # FileSystemService
│
└── CharacterEncoding/             # 文字エンコーディングドメイン
    ├── Domain/
    │   ├── Interface/
    │   │   ├── ICharacterEncoder.cs   # エンコーダーIF
    │   │   └── Factory/           # ICharacterEncoderFactory
    │   ├── Model/
    │   │   └── MachineType.cs     # 18機種定義
    │   └── Exception/             # CharacterEncodingException
    ├── Infrastructure/
    │   ├── Encoder/               # 機種別エンコーダー
    │   │   ├── X1CharacterEncoder.cs      # X1完全実装
    │   │   ├── Pc8801CharacterEncoder.cs  # PC-8801基本ASCII
    │   │   └── Msx1CharacterEncoder.cs    # MSX基本ASCII
    │   └── Factory/               # CharacterEncoderFactory
    └── Application/               # CharacterEncodingService
```

### 設計原則

- **Single Responsibility**: 各クラスは単一の責務を持つ
- **Open/Closed**: 拡張に開いて修正に閉じている
- **Dependency Inversion**: 具象ではなく抽象に依存
- **Interface Segregation**: 必要最小限のインターフェース
- **Factory Method**: オブジェクト生成の抽象化
- **Composition over Inheritance**: 組成パターンによるコード再利用

## 🚧 実装状況とロードマップ

### ✅ 完成済み機能 (v1.2.0)

#### Phase 8完了: MSX-DOSファイルシステム (2025年5月)
- ✅ **MSX-DOS完全実装**: FAT12ベース、720KB標準フォーマット
- ✅ **MSX-DOS 1.0/2.0互換性**: BPB・FATメディア記述子自動検出
- ✅ **CLI統合**: `--filesystem msx-dos`、`--machine msx1`対応
- ✅ **自動検出強化**: MSX固有設定値による判定ロジック

#### Phase 7完了: N88-BASICファイルシステム (2025年5月)
- ✅ **N88-BASIC完全実装**: PC-8801専用、16バイトディレクトリエントリ
- ✅ **ディスクタイプ対応**: 2D/2DD（2HD非対応）
- ✅ **属性処理**: バイナリ・BASIC・ASCII・書き込み保護
- ✅ **CLI統合**: `--filesystem n88-basic`、`--machine pc8801`対応

#### Phase 6完了: 基本機能完成 (2025年1月-5月)
- ✅ **Hu-BASIC完全実装**: Sharp X1、全ディスクタイプ対応
- ✅ **MS-DOS FAT12基本実装**: PC汎用、DSK形式ネイティブサポート
- ✅ **DSK形式完全対応**: 作成・読み書き・フォーマット
- ✅ **文字エンコーディング**: 18機種アーキテクチャ、X1完全実装
- ✅ **安全性強化**: 書き込み時ファイルシステム指定必須
- ✅ **エラーハンドリング**: 破損ディスク復旧、プロフェッショナルレベル

### 🚧 今後の拡張予定

#### Phase 9: 文字エンコーダー拡張
- **PC-8801エンコーダー**: グラフィック文字・罫線文字対応
- **MSX1/MSX2エンコーダー**: MSX固有文字セット対応  
- **MZ-700/MZ-1500エンコーダー**: Sharp MZ系統文字コード
- **FM-7/FM-77エンコーダー**: 富士通系統文字コード

#### Phase 10: 追加ファイルシステム対応
- **MS-DOS FAT16**: FAT12の上位互換、大容量対応
- **CP/M**: 8ビット時代の標準、資料豊富
- **MSX-DOS 2.2**: サブディレクトリ対応

#### Phase 11: ディスクフォーマット拡張
- **IMD形式**: ImageDisk形式対応
- **IMG形式**: PC標準イメージ対応
- **ディスクイメージ変換**: D88 ↔ DSK ↔ IMD ↔ IMG

#### Phase 12: 高度な機能
- **仮想ディスクマウント**: OSレベルでのマウント機能
- **バッチ処理**: 複数ディスクの一括処理  
- **REST API**: Webサービス化

#### Phase 13: GUI・Web版
- **デスクトップGUI**: WPF/Avalonia版
- **Webアプリケーション**: Blazor版ディスクブラウザ
- **クロスプラットフォーム**: MAUI対応

## 🎯 対象ユーザー

- **レトロコンピュータ愛好家**: 古いディスクイメージの管理・変換
- **研究者・アーカイバ**: デジタル文化遺産の保存
- **エミュレータ開発者**: ディスクイメージ操作ライブラリとして
- **ゲーム保存プロジェクト**: 古いゲームの解析・アーカイブ

## 📚 ドキュメント

### 🗺️ [開発ロードマップ](Documents/ROADMAP.md) ← **必見！**
将来の展望、C++/WASM/組み込み対応計画など

### フォーマット仕様
- [**D88フォーマット仕様**](Documents/D88_Format.md): D88形式の詳細仕様
- [**Hu-BASICファイルシステム**](Documents/Hu-BASIC_Format.md): Hu-BASICの構造解説
- [**FAT12ファイルシステム**](Documents/FAT12_Format.md): MS-DOS FAT12の詳細仕様
- [**MSX-DOSフォーマット仕様**](Documents/MSX_DOS_Format.md): MSX-DOS FAT12の詳細仕様
- [**2D形式仕様**](Documents/2D_Format_Specification.md): Plain Disk Image仕様 ← **NEW!**

### 開発ドキュメント
- [**APIリファレンス**](Documents/API_Reference.md): 詳細なライブラリAPI仕様
- [**実装履歴**](Documents/Implementation_History.md): 開発プロセスの詳細記録
- [**実装要件**](Documents/Implement.md): プロジェクトの要件定義

## 🔧 開発環境

### 要件
- **.NET 8.0 SDK** 以上
- **Windows / macOS / Linux** 対応
- **Git** (ソースコード管理用)

### 開発・ビルド

```bash
# 開発用ビルド
dotnet build

# リリースビルド  
dotnet build -c Release

# テスト実行
dotnet test

# 包括的テストスイート実行
dotnet run --project Test

# 型チェック
dotnet build --verbosity normal
```

### プロジェクト構成

```
Legacy89DiskKit/
├── CSharp/                        # C#実装
│   ├── Legacy89DiskKit/           # メインライブラリ
│   ├── Legacy89DiskKit.CLI/       # CLIツール
│   └── Test/                      # テストプロジェクト
├── Documents/                     # ドキュメント
├── SampleCode/                    # サンプルコード
└── README.md                      # このファイル
```

## 🤝 コントリビューション

プロジェクトへの貢献を歓迎します！

### 貢献方法
1. **Issue報告**: バグ報告や機能要望
2. **Pull Request**: コード改善・新機能追加
3. **ドキュメント**: 説明の改善・翻訳

### 開発ガイドライン
- DDDアーキテクチャを維持
- 既存のコード規約に従う
- 適切なテストを追加
- ドキュメントを更新

### 主要な貢献ポイント
- **新ファイルシステム実装**: CP/M、FAT16等
- **文字エンコーダー拡張**: PC-8801、MSX等のグラフィック文字対応
- **新ディスクフォーマット**: IMD、IMG等
- **GUI版実装**: WPF、Avalonia、Blazor等

## 📄 ライセンス

このプロジェクトは[MITライセンス](LICENSE)の下で公開されています。

## 🙏 謝辞

- **Sharp X1ユーザーコミュニティ**: フォーマット仕様の調査・検証
- **PC-8801保存プロジェクト**: N88-BASIC仕様の詳細情報提供
- **MSXコミュニティ**: MSX-DOS仕様の技術資料提供
- **レトロコンピュータ保存プロジェクト**: 貴重な資料・サンプルディスクの提供
- **オープンソースコミュニティ**: ツール・ライブラリの提供

---

💖 **Legacy89DiskKit v1.2.0**で、貴重なレトロコンピュータの遺産を現代に蘇らせましょう！

**🎉 最新情報**: MSX-DOSファイルシステム完全対応！MSX、Sharp X1、PC-8801の主要ディスクフォーマットを統一的に操作可能になりました。