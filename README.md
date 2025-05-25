# Legacy89DiskKit

📀 **Legacy89DiskKit** は、1980〜90年代の日本のパソコン（特に X1 / PC-8801 / FM-7 など）で使用されていたディスクフォーマット（例: `.D88`, `.DDX`）を扱うための C# ライブラリです。Hu-BASIC や N88-BASIC を含む様々なファイルシステムを抽象化し、モダンなDDD（ドメイン駆動設計）で構築されています。

## 概要

- 💽 D88形式の読み書き（トラック/セクタ構造含む）
- 📂 Hu-BASICファイルシステムの読み書き
- 🧠 インターフェース駆動設計による拡張性
- 🏛️ 1クラス1ファイル設計 / DDD準拠のフォルダ構造
- 🛡️ 破損ディスクからの部分復旧機能
- 🔍 詳細なエラー検出と報告
- 💾 メモリ安全な大容量ファイル処理

## 対象環境

- .NET 8.0 or later
- Windows / macOS / Linux

## 使用例

```bash
# ディスクファイル一覧表示
./Legacy89DiskKit.CLI list disk.d88

# ファイル読み出し
./Legacy89DiskKit.CLI read disk.d88 filename.txt output.txt

# 破損したテキストファイルの部分復旧
./Legacy89DiskKit.CLI recover-text disk.d88 file.txt recovered.txt

# 破損したバイナリファイルの部分復旧  
./Legacy89DiskKit.CLI recover-binary disk.d88 program.bin recovered.bin
```

### C# ライブラリとしての使用

```csharp
using var container = new D88DiskContainer("disk.d88");
var fileSystem = new HuBasicFileSystem(container);

// ファイル一覧取得
var files = fileSystem.ListFiles();
foreach (var file in files)
{
    Console.WriteLine($"{file.FileName}.{file.Extension}");
}

// ファイル読み込み（通常）
var data = fileSystem.ReadFile("filename.txt");

// 破損ファイルの部分復旧
var partialData = fileSystem.ReadFile("damaged.txt", allowPartialRead: true);
