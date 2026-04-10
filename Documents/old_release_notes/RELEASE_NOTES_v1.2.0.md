# Legacy89DiskKit v1.2.0 リリースノート

## 🎉 新機能

### MSX-DOS ファイルシステム完全対応
Legacy89DiskKit v1.2.0では、**MSX-DOS**ファイルシステムのサポートが追加されました。これにより、MSX、Sharp X1、PC-8801の主要なレトロコンピュータのディスクフォーマットを統一的に操作できるようになりました。

## ✨ 主な変更点

### 新機能
- **MSX-DOSファイルシステム実装** - FAT12ベースのMSX専用ファイルシステム
- **MSX-DOS 1.0/2.0互換性** - BPB・FATメディア記述子自動検出
- **720KB標準フォーマット対応** - 80トラック×2面×9セクタ×512バイト
- **MSX固有設定** - 2セクタ/クラスタ、112ルートディレクトリエントリ

### CLI機能拡張
- `--filesystem msx-dos` オプション追加
- `--machine msx1` / `--machine msx2` サポート
- MSX-DOSディスクの自動検出強化

### アーキテクチャ改善
- コンポジションパターンによるFAT12の効率的な再利用
- MSX固有設定値による自動判定ロジック
- 堅牢なエラーハンドリング

## 📦 対応状況

### 完全対応ファイルシステム (v1.2.0)
| ファイルシステム | 対応機種 | 状態 |
|-----------------|---------|------|
| Hu-BASIC | Sharp X1 | ✅ 完全実装 |
| N88-BASIC | PC-8801 | ✅ 完全実装 |
| MS-DOS FAT12 | PC汎用 | ✅ 完全実装 |
| MSX-DOS | MSX | ✅ 完全実装 |

### 対応ディスクイメージ
- **D88形式** - Sharp/NECディスクイメージ標準
- **DSK形式** - PC汎用ディスクイメージ

## 🔧 使用例

```bash
# MSX-DOSディスク作成
./Legacy89DiskKit.CLI create msxdisk.d88 2DD "MSX DISK"
./Legacy89DiskKit.CLI format msxdisk.d88 --filesystem msx-dos

# ファイル操作
./Legacy89DiskKit.CLI import-text msxdisk.d88 readme.txt README.TXT --filesystem msx-dos --machine msx1
./Legacy89DiskKit.CLI list msxdisk.d88 --filesystem msx-dos
```

## 🚀 インストール

各プラットフォーム用のバイナリをダウンロードし、展開してご利用ください：

- **Windows x64**: `Legacy89DiskKit-v1.2.0-win-x64.zip`
- **Linux x64**: `Legacy89DiskKit-v1.2.0-linux-x64.tar.gz`
- **macOS x64**: `Legacy89DiskKit-v1.2.0-osx-x64.tar.gz`
- **macOS ARM64**: `Legacy89DiskKit-v1.2.0-osx-arm64.tar.gz`

## 📋 既知の問題

- MSX-DOS 2.2のサブディレクトリはまだサポートされていません
- gh CLIツールでの認証が必要な場合があります

## 🙏 謝辞

MSXコミュニティの皆様、特にMSX-DOS仕様の技術資料を提供していただいた方々に深く感謝いたします。

---

**Legacy89DiskKit** - レトロコンピュータの遺産を現代に蘇らせる