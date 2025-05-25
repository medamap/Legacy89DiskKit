# Legacy89DiskKit

📀 **Legacy89DiskKit** は、1980〜90年代の日本のパソコン（特に X1 / PC-8801 / FM-7 など）で使用されていたディスクフォーマット（例: `.D88`, `.DDX`）を扱うための C# ライブラリです。Hu-BASIC や N88-BASIC を含む様々なファイルシステムを抽象化し、モダンなDDD（ドメイン駆動設計）で構築されています。

## 概要

- 💽 D88形式の読み書き（トラック/セクタ構造含む）
- 📂 Hu-BASICファイルシステムの読み書き
- 🧠 インターフェース駆動設計による拡張性
- 🏛️ 1クラス1ファイル設計 / DDD準拠のフォルダ構造

## 対象環境

- .NET 8.0 or later
- Windows / macOS / Linux

## 使用例（予定）

```csharp
var d88 = new D88Image("disk.d88");
var fs = new HuBasicFileSystem();
fs.Mount(d88);
foreach (var file in fs.ListFiles())
{
    Console.WriteLine($"{file.FileName}.{file.Extension}");
}
