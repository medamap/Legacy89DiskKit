# Legacy89DiskKit v1.4.0 リリースノート

リリース日: 2025年6月6日

## 🎉 主要な新機能

### CDOS (Club DOS) ファイルシステム対応

Legacy89DiskKitに **6つ目のレガシーファイルシステム** として **CDOS (Club DOS)** 対応を追加しました。

#### 📋 対応ファイルシステム一覧
1. Hu-BASIC (Sharp X1) ✅
2. N88-BASIC (PC-8801) ✅
3. MS-DOS FAT12 ✅
4. MSX-DOS ✅
5. CP/M 2.2 ✅
6. **CDOS (Club DOS)** ✅ ← **NEW!**

## 🚀 新機能

### CDOS (Club DOS) ファイルシステム
- **完全なファイルシステム実装**: ファイル読み書き、削除、作成、フォーマット
- **ディスクタイプサポート**: 2D（320KB）/ 2HD（1.2MB）ディスク対応
- **混合セクターサイズ対応**: 2HDディスクのTrack 0（128バイト/セクタ）とTrack 1+（1024バイト/セクタ）
- **8.3ファイル名形式**: DOSライクなファイル名規則（8文字名 + 3文字拡張子）
- **32バイトディレクトリエントリ**: ファイル名、拡張子、サイズ、位置、属性、実行アドレス
- **ファイル属性**: ReadOnly、Archive、Hidden、System属性対応
- **ロード/実行アドレス**: 実行可能ファイルのメモリアドレス管理
- **ワイルドカード検索**: *.*, *.TXT, TEST*.* 等のパターン検索
- **自動検出機能**: ディスクイメージからCDOSフォーマットを自動識別

### テスト・品質保証
- **包括的テストスイート**: 39個のテストケース全て成功
- **プラットフォーム対応テスト**: 2D/2HDディスク両方での動作検証
- **CRUD操作テスト**: Create、Read、Update、Delete操作の完全テスト
- **エラーハンドリングテスト**: 異常系の適切な処理確認
- **ファイル名バリデーションテスト**: 8.3形式の正確な検証

## 🛠️ 技術仕様

### CDOS設定
- **2Dディスク**: 40トラック、2ヘッド、16セクタ/トラック、256バイト/セクタ
- **2HDディスク**: 77トラック、2ヘッド、8セクタ/トラック、1024バイト/セクタ
  - Track 0のみ: 26セクタ/トラック、128バイト/セクタ
- **ディレクトリ**: Track 1から開始、最大128エントリ
- **ファイルサイズ**: 最大4GB（32ビット）

### アーキテクチャ
- **ドメイン駆動設計（DDD）**: Domain/Application/Infrastructure層
- **ファクトリーパターン**: FileSystemFactory統合
- **設定管理**: CdosConfiguration クラス
- **ディレクトリキャッシュ**: 高速ディレクトリアクセス
- **セクター管理**: 動的な空き領域検索

## 📦 配布パッケージ

4つのプラットフォーム向けにバイナリを提供：

- **Windows x64**: `Legacy89DiskKit-v1.4.0-win-x64.zip` (34.1MB)
- **Linux x64**: `Legacy89DiskKit-v1.4.0-linux-x64.tar.gz` (32.8MB)
- **macOS x64**: `Legacy89DiskKit-v1.4.0-osx-x64.tar.gz` (32.3MB)
- **macOS ARM64**: `Legacy89DiskKit-v1.4.0-osx-arm64.tar.gz` (30.7MB)

## 🔧 CLI使用例

```bash
# CDOSディスクの情報表示
Legacy89DiskKit.CLI info disk.d88

# CDOSディスクのファイル一覧
Legacy89DiskKit.CLI list disk.d88

# CDOSディスクからファイルエクスポート
Legacy89DiskKit.CLI export disk.d88 GAME.BAS ./exported_game.bas

# CDOSディスクにファイルインポート
Legacy89DiskKit.CLI import disk.d88 ./program.bin PROGRAM.BIN

# CDOSディスクのフォーマット
Legacy89DiskKit.CLI format disk.d88 --filesystem Cdos --disk-type 2D
```

## 🧪 テスト結果

- **総テストケース**: 39個
- **成功率**: 100%
- **対象**: 2D/2HD両ディスクタイプ
- **カバレッジ**: ファイル操作、エラーハンドリング、設定値、バリデーション

## 🏗️ 開発者向け

### 新しいAPI

```csharp
// CDOSファイルシステムの作成
var cdosFileSystem = fileSystemFactory.OpenFileSystem(diskContainer, FileSystemType.Cdos);

// CDOS設定の取得
var config = CdosConfiguration.GetConfiguration(DiskType.TwoHD);

// CDOSファイル名検証
bool isValid = CdosFileNameValidator.IsValidFileName("TEST.TXT");
```

### 統合
- `FileSystemType.Cdos` 列挙値追加
- `FileSystemFactory` への自動検出統合
- `ComprehensiveTestSuite` への統合

## 🔄 下位互換性

- **既存API**: 完全に保持
- **設定ファイル**: 既存設定に影響なし
- **ファイル形式**: 既存のD88/DSK形式継続サポート

## 🙏 謝辞

このリリースは Claude Code による AI 支援開発で実現されました。

---

**ダウンロード**: [GitHub Releases](https://github.com/medamap/Legacy89DiskKit/releases/tag/v1.4.0)

**前バージョン**: [v1.3.0 リリースノート](RELEASE_NOTES_v1.3.0.md)