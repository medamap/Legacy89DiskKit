# Legacy89DiskKit v1.5.0 リリースノート

## リリース日
2025年1月6日

## 概要
Legacy89DiskKit v1.5.0では、待望のインタラクティブシェル機能を追加しました。この新機能により、複数のディスクイメージを同時に扱いながら、直感的な対話型インターフェースでディスク操作が可能になります。

## 主な新機能

### 1. インタラクティブシェルモード
- **起動方法**: `Legacy89DiskKit shell` コマンドで対話型シェルを起動
- **リアルタイムフィードバック**: コマンドの実行結果が即座に表示
- **状態の保持**: シェル内で開いたディスクの状態を維持

### 2. マルチスロット管理
- **10個のディスクスロット**: 最大10個のディスクイメージを同時に開いて管理
- **スロット切り替え**: `slot` コマンドで簡単にアクティブスロットを変更
- **スロット間操作**: ディスク間でのファイルコピーや移動が可能

### 3. Tab補完機能
- **コマンド補完**: コマンド名の自動補完
- **ファイルパス補完**: ローカルファイルシステムのパスを自動補完
- **ディスクファイル補完**: 現在のディスク内のファイル名を自動補完
- **スロット指定補完**: `0:FILE.TXT` 形式でのスロット指定をサポート

### 4. 拡張コマンドセット
シェルモードで使用できる新しいコマンド：

#### スロット管理
- `slot [number]` - スロットの表示または切り替え
- `slots` - 全スロットの状態を表示
- `open <disk-file> [slot] [--readonly]` - ディスクイメージを開く
- `close [slot]` - ディスクを閉じる

#### ディスク操作
- `new <disk-file> <type> <name> [slot]` - 新しいディスクを作成
- `format <filesystem> [slot]` - ディスクをフォーマット
- `info [slot]` - ディスク情報を表示

#### ファイル操作
- `list [slot]` または `ls [slot]` - ファイル一覧表示
- `import-text <host-file> <disk-file> [slot] [--machine <type>]` - テキストファイルをインポート
- `export-text <disk-file> <host-file> [slot] [--machine <type>]` - テキストファイルをエクスポート
- `import-binary <host-file> <disk-file> [slot] [load] [exec]` - バイナリファイルをインポート
- `export-binary <disk-file> <host-file> [slot]` - バイナリファイルをエクスポート
- `delete <disk-file> [slot]` または `del` - ファイルを削除

#### クロススロット操作
- `copy <source> <destination>` または `cp` - ファイルをコピー
- `move <source> <destination>` または `mv` - ファイルを移動
  - 形式: `[slot:]filename` または `host:path`
  - 例: `copy 0:FILE.TXT 1:` （スロット0からスロット1へ）
  - 例: `copy host:/tmp/file.txt 0:FILE.TXT` （ホストからディスクへ）

#### ディレクトリ管理
- `pwd` - 現在のホストディレクトリを表示
- `cd <directory>` - ホストディレクトリを変更（`~` サポート）

#### デバッグ
- `debug <disk-file>` - ディスクヘッダー情報を詳細表示

### 5. Hu-BASICファイルシステムの改善
- **256バイトセクタ対応**: Hu-BASICディスクの検出精度を向上
- **ブートセクタ判定の最適化**: より確実なファイルシステム自動検出

## 使用例

### 基本的な使い方
```bash
# インタラクティブシェルを起動
Legacy89DiskKit shell

# ディスクを開く
Legacy89DiskKit [0:Empty]> open ~/disks/game.d88

# ファイル一覧を表示
Legacy89DiskKit [0:game.d88/HuBasic]> list

# 別のスロットにディスクを開く
Legacy89DiskKit [0:game.d88/HuBasic]> open ~/disks/data.d88 1

# スロット間でファイルをコピー
Legacy89DiskKit [0:game.d88/HuBasic]> copy GAME.BAS 1:

# Tab補完を使用
Legacy89DiskKit [0:game.d88/HuBasic]> export-text GA[TAB]
Legacy89DiskKit [0:game.d88/HuBasic]> export-text GAME.BAS ~/[TAB]
```

### 複数ディスクの同時管理
```bash
# 3つのディスクを同時に開く
Legacy89DiskKit [0:Empty]> open source.d88 0
Legacy89DiskKit [0:source.d88/HuBasic]> open work.d88 1
Legacy89DiskKit [0:source.d88/HuBasic]> open backup.d88 2

# 全スロットの状態を確認
Legacy89DiskKit [0:source.d88/HuBasic]> slots
Slot Status:
*0: source.d88/HuBasic (TwoD) - 15 files
 1: work.d88/HuBasic (TwoDD) - 3 files
 2: backup.d88/HuBasic (TwoDD) - 22 files
 3: Empty
 ...

# ファイルを複数のディスクにコピー
Legacy89DiskKit [0:source.d88/HuBasic]> copy IMPORTANT.DAT 1:
Legacy89DiskKit [0:source.d88/HuBasic]> copy IMPORTANT.DAT 2:BACKUP.DAT
```

## 技術的な改善点

### アーキテクチャ
- **モジュラー設計**: シェル機能を独立したモジュールとして実装
- **コマンドパターン**: 拡張可能なコマンドハンドラーアーキテクチャ
- **状態管理**: スロットマネージャーによる効率的なディスク状態管理

### パフォーマンス
- **遅延読み込み**: ディスクイメージは必要時にのみ読み込み
- **キャッシュ機構**: 頻繁にアクセスされるディスク情報をキャッシュ

### エラーハンドリング
- **詳細なエラーメッセージ**: 問題の原因を明確に表示
- **部分的な成功の処理**: 複数ファイル操作時の個別エラー処理

## 既知の問題
- Tab補完使用時にカーソル位置がずれることがある（macOS環境）
- 大量のファイルがあるディスクでのTab補完が遅い場合がある

## 今後の予定
- ブート情報を独立したドメインとして分離（Documents/TODO_BootInfo_Refactoring.md参照）
- ファイル検索機能の追加
- バッチ処理機能の実装
- シェルスクリプト対応

## アップグレード方法
既存のv1.4.0からのアップグレードは、単純にバイナリを置き換えるだけで完了します。既存のコマンドライン機能との後方互換性は完全に保たれています。

## 謝辞
このリリースは、コミュニティからのフィードバックと要望に基づいて開発されました。特に、マルチディスク操作のニーズを報告してくださった皆様に感謝いたします。

## ダウンロード
- [Windows x64版](https://github.com/medamap/Legacy89DiskKit/releases/download/v1.5.0/Legacy89DiskKit-win-x64-v1.5.0.zip)
- [Windows ARM64版](https://github.com/medamap/Legacy89DiskKit/releases/download/v1.5.0/Legacy89DiskKit-win-arm64-v1.5.0.zip)
- [Linux x64版](https://github.com/medamap/Legacy89DiskKit/releases/download/v1.5.0/Legacy89DiskKit-linux-x64-v1.5.0.tar.gz)
- [Linux ARM64版](https://github.com/medamap/Legacy89DiskKit/releases/download/v1.5.0/Legacy89DiskKit-linux-arm64-v1.5.0.tar.gz)
- [macOS x64版](https://github.com/medamap/Legacy89DiskKit/releases/download/v1.5.0/Legacy89DiskKit-osx-x64-v1.5.0.tar.gz)
- [macOS ARM64版](https://github.com/medamap/Legacy89DiskKit/releases/download/v1.5.0/Legacy89DiskKit-osx-arm64-v1.5.0.tar.gz)