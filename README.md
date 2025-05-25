# Legacy89DiskKit

[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Latest-239120?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat)](LICENSE)

📀 **Legacy89DiskKit** は、1980〜90年代の日本のレトロコンピュータ（Sharp X1、PC-8801、FM-7など）で使用されていたディスクフォーマットを現代的な環境で扱うためのC#ライブラリ・CLIツールセットです。

## ✨ 特徴

### 🎯 対応フォーマット
- **D88形式**: 完全対応（2D/2DD/2HD）
- **Hu-BASICファイルシステム**: Sharp X1専用ファイルシステム
- **将来拡張予定**: N88-BASIC、MSX-DOS、CP/M等

### 🏗️ モダンアーキテクチャ
- **DDD（ドメイン駆動設計）**: 拡張性・保守性を重視した設計
- **インターフェース駆動**: 異なるディスクフォーマットの統一的な操作
- **レイヤー分離**: ドメイン・インフラ・アプリケーション層の明確な分離

### 🛡️ 堅牢性
- **破損ディスク対応**: 部分的なデータ復旧機能
- **メモリ安全**: 大容量ファイルの安全な処理（10MB制限）
- **エラー検出**: 循環参照検出、詳細な診断情報
- **入力検証**: Hu-BASIC固有のファイル名規則対応

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

```bash
# 新しいディスクイメージを作成
dotnet run --project Legacy89DiskKit.CLI -- create mydisk.d88 2D "MY DISK"

# ディスクをフォーマット
dotnet run --project Legacy89DiskKit.CLI -- format mydisk.d88

# ファイル一覧表示
dotnet run --project Legacy89DiskKit.CLI -- list mydisk.d88

# テキストファイルをインポート
dotnet run --project Legacy89DiskKit.CLI -- import-text mydisk.d88 readme.txt README.TXT
```

## 📋 機能一覧

### CLIコマンド

| コマンド | 説明 | 例 |
|---------|------|-----|
| `create` | 新しいディスクイメージ作成 | `create disk.d88 2D "TITLE"` |
| `format` | ディスクフォーマット | `format disk.d88` |
| `list` | ファイル一覧表示 | `list disk.d88` |
| `import-text` | テキストファイル書き込み | `import-text disk.d88 host.txt DISK.TXT` |
| `export-text` | テキストファイル読み出し | `export-text disk.d88 DISK.TXT host.txt` |
| `import-binary` | バイナリファイル書き込み | `import-binary disk.d88 prog.bin PROG.BIN 8000 8000` |
| `export-binary` | バイナリファイル読み出し | `export-binary disk.d88 PROG.BIN prog.bin` |
| `import-boot` | ブートセクタ書き込み | `import-boot disk.d88 boot.bin "BOOTLOADER"` |
| `export-boot` | ブートセクタ情報出力 | `export-boot disk.d88 boot.txt` |
| `delete` | ファイル削除 | `delete disk.d88 OLDFILE.TXT` |
| `info` | ディスク情報表示 | `info disk.d88` |
| **`recover-text`** | **破損テキストファイル復旧** | `recover-text disk.d88 damaged.txt recovered.txt` |
| **`recover-binary`** | **破損バイナリファイル復旧** | `recover-binary disk.d88 damaged.bin recovered.bin` |

### ライブラリAPI

```csharp
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

// ディスクイメージを開く
using var container = new D88DiskContainer("disk.d88");
var fileSystem = new HuBasicFileSystem(container);

// ファイル一覧取得
var files = fileSystem.ListFiles();
foreach (var file in files)
{
    Console.WriteLine($"{file.FileName}.{file.Extension} ({file.Size} bytes)");
}

// ファイル読み込み（通常）
var data = fileSystem.ReadFile("README.TXT");

// 破損ファイルの部分復旧
var partialData = fileSystem.ReadFile("damaged.txt", allowPartialRead: true);

// 新規ディスクイメージ作成
using var newContainer = D88DiskContainer.CreateNew("new.d88", DiskType.TwoD, "NEW DISK");
var newFileSystem = new HuBasicFileSystem(newContainer);
newFileSystem.Format();
```

## 💾 対応ディスクタイプ

| タイプ | 容量 | トラック数 | 面数 | セクタ/トラック | セクタサイズ |
|-------|------|-----------|------|---------------|-------------|
| **2D** | 320KB | 40 | 2 | 16 | 256B |
| **2DD** | 640KB | 80 | 2 | 16 | 256B |
| **2HD** | 1.2MB | 77 | 2 | 26 | 256B |

## 🛠️ アーキテクチャ

### ドメイン構造

```
Legacy89DiskKit/
├── DiskImage/                     # ディスクイメージドメイン
│   ├── Domain/
│   │   ├── Interface/Container/   # IDiskContainer
│   │   └── Exception/            # DiskImageException
│   ├── Infrastructure/Container/  # D88DiskContainer
│   └── Application/              # DiskImageService
│
└── FileSystem/                   # ファイルシステムドメイン
    ├── Domain/
    │   ├── Interface/FileSystem/ # IFileSystem
    │   └── Exception/           # FileSystemException
    ├── Infrastructure/
    │   ├── FileSystem/          # HuBasicFileSystem
    │   └── Utility/             # X1文字コード変換等
    └── Application/             # FileSystemService
```

### 設計原則

- **Single Responsibility**: 各クラスは単一の責務を持つ
- **Open/Closed**: 拡張に開いて修正に閉じている
- **Dependency Inversion**: 具象ではなく抽象に依存
- **Interface Segregation**: 必要最小限のインターフェース

## 🔄 文字コード対応

Sharp X1固有の文字コード体系をサポート：

- **JIS X 0201**: 基本ASCII + 半角カタカナ
- **X1独自記号**: 罫線文字等
- **自動変換**: Unicode ↔ X1文字コード

```csharp
var converter = new X1Converter();
var x1Text = converter.ToX1("こんにちは");        // ひらがな→カタカナ変換
var unicodeText = converter.ToUnicode(x1Bytes);  // X1→Unicode変換
```

## 🚧 今後の拡張予定

### Phase 4: 追加ディスクフォーマット対応
- **PC-8801 N88-BASIC**: `.D88`形式内のN88-BASICファイルシステム
- **MSX-DOS**: `.DSK`形式とMSX-DOSファイルシステム
- **CP/M**: 8インチディスクフォーマット

### Phase 5: 高度な機能
- **ディスクイメージ変換**: D88 ↔ DSK ↔ IMD等
- **仮想ディスクマウント**: OSレベルでのマウント機能
- **バッチ処理**: 複数ディスクの一括処理

### Phase 6: GUI・Web版
- **デスクトップGUI**: WPF/Avalonia版
- **Webアプリケーション**: Blazor版ディスクブラウザ
- **REST API**: Webサービス化

## 🎯 対象ユーザー

- **レトロコンピュータ愛好家**: 古いディスクイメージの管理・変換
- **研究者・アーカイバ**: デジタル文化遺産の保存
- **エミュレータ開発者**: ディスクイメージ操作ライブラリとして
- **ゲーム保存プロジェクト**: 古いゲームの解析・アーカイブ

## 📚 ドキュメント

- [**D88フォーマット仕様**](Documents/D88_Format.md): D88形式の詳細仕様
- [**Hu-BASICファイルシステム**](Documents/Hu-BASIC_Format.md): Hu-BASICの構造解説
- [**実装履歴**](Documents/Implementation_History.md): 開発プロセスの詳細
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

# 型チェック
dotnet build --verbosity normal
```

## 🤝 コントリビューション

プロジェクトへの貢献を歓迎します！

1. **Issue報告**: バグ報告や機能要望
2. **Pull Request**: コード改善・新機能追加
3. **ドキュメント**: 説明の改善・翻訳

### 開発ガイドライン
- DDDアーキテクチャを維持
- 既存のコード規約に従う
- 適切なテストを追加
- ドキュメントを更新

## 📄 ライセンス

このプロジェクトは[MITライセンス](LICENSE)の下で公開されています。

## 🙏 謝辞

- **Sharp X1ユーザーコミュニティ**: フォーマット仕様の調査・検証
- **レトロコンピュータ保存プロジェクト**: 貴重な資料・サンプルディスクの提供
- **オープンソースコミュニティ**: ツール・ライブラリの提供

---

💖 **Legacy89DiskKit**で、貴重なレトロコンピュータの遺産を現代に蘇らせましょう！