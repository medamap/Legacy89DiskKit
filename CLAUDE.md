# Codex向け開発ガイド

## プロジェクト概要
Legacy89DiskKitは、1980〜90年代の日本のレトロコンピュータで使用されていたディスクフォーマットを扱うC#ライブラリです。

## 主要な情報

### リリース手順
- 新バージョンのリリース手順: [Documents/Release_Process.md](Documents/Release_Process.md)

### テスト実行
```bash
# 基本テスト
dotnet test

# 包括的テストスイート  
dotnet run --project Test
```

### ビルドコマンド
```bash
# デバッグビルド
dotnet build

# リリースビルド
dotnet build -c Release
```

## アーキテクチャ
- DDD（ドメイン駆動設計）準拠
- ファクトリーパターンを使用
- 各ドメインはApplication/Domain/Infrastructureの3層構造

## 対応ファイルシステム
1. Hu-BASIC (Sharp X1)
2. N88-BASIC (PC-8801)
3. MS-DOS FAT12
4. MSX-DOS
5. CP/M 2.2

## 重要なプロジェクトルール

### コーディング規約
- **コメント不要**: コード内にコメントは追加しない（ユーザーから明示的に要求された場合を除く）
- **例外メッセージ**: 例外は英語で記述
- **ファイル命名**: PascalCase使用（例: `CpmFileSystem.cs`）
- **名前空間**: `Legacy89DiskKit.{Domain}.{Layer}.{SubCategory}` の形式

### Git運用ルール
- **コミットメッセージ**: 日本語で記述
- **ブランチ戦略**: 
  - `main`: リリースブランチ
  - `develop`: 開発ブランチ
  - `feature/xxx-support`: 機能開発ブランチ
- **マージ方法**: `--no-ff`オプションを使用（マージコミットを作成）
- **Co-Authored-By**: `Co-Authored-By: Claude <claude-ai@anthropic.invalid>` を使用（RFC6761準拠の.invalidドメイン）

### ファイルシステム実装時の注意
- **書き込み時の安全性**: ファイルシステムタイプの明示的な指定を必須とする
- **読み取り専用ラッパー**: 自動検出時は`ReadOnlyFileSystemWrapper`でラップ
- **エラーハンドリング**: 部分的な読み取り（`allowPartialRead`）をサポート
- **文字エンコーディング**: 機種別のエンコーダーを必ず実装

### テストの実行
- **変更前後でテスト実行**: 必ずテストが通ることを確認
- **新機能追加時**: 対応するテストケースを追加
- **テストファイル命名**: `{FeatureName}Test.cs`

### ドキュメント更新
- **README.md**: 新機能追加時は必ず更新
- **APIドキュメント**: 公開APIには必ずXMLドキュメントコメントを付ける
- **実装ドキュメント**: 複雑な実装にはDocumentsフォルダに説明を追加

### リリース時の注意
- **バージョニング**: セマンティックバージョニング準拠
- **破壊的変更**: 可能な限り避ける（必要な場合はメジャーバージョンアップ）
- **リリースノート**: 日本語で詳細に記述
- **バイナリ**: 4つのプラットフォーム（Windows/Linux/macOS x64/ARM64）

### CP/M特有の制限事項
- **128バイト境界**: ファイルサイズは128バイト単位
- **EOFマーカー**: Ctrl+Z (0x1A)の適切な処理
- **大容量ファイル**: 16KB以上では正確なサイズ復元が困難

### 禁止事項
- **プロアクティブなドキュメント作成**: ユーザーから要求されない限り、README.mdや*.mdファイルを勝手に作成しない
- **不要なファイル作成**: 必要最小限のファイルのみ作成
- **既存ファイルの大幅な変更**: 必要な部分のみを変更

## デバッグ時のヒント
- **ディスクイメージの確認**: `hexdump -C disk.d88 | less`
- **セクタダンプ**: `IDiskContainer.ReadSector()`でセクタ単位の確認
- **ファイルシステム自動検出**: `FileSystemFactory.OpenFileSystemReadOnly()`の挙動確認

## 現在進行中の作業: ディスクコピー機能実装

### 現在の実装状況
詳細な進捗状況は [Documents/Disk_Copy_Implementation_Status.md](Documents/Disk_Copy_Implementation_Status.md) を参照

### 次に実装すべき項目（優先順）
1. **ファイル名自動短縮機能** - 8文字制限ファイルシステム対応
   - コピー先の制限: 8文字以下の場合、先頭5文字＋連番3桁（例: "LONGFILE.TXT" → "LONGF001.TXT"）
   - 重複時の連番自動増加
   - 実装場所: `IFileNameConverter` の拡張

2. **複数ファイル一括コピー機能**
   - 現在は1ファイルずつのみ対応
   - CLI: `copy <source> <dest> FILE1.TXT FILE2.BIN FILE3.BAS --source-fs <type> --dest-fs <type>`

3. **ディスク丸ごとコピー機能**
   - 基本機能: セクタ単位での完全コピー
   - CLI: `disk-copy <source-disk> <dest-disk>`
   - ファイルシステムに依存しない物理コピー

### 実装時の注意点
- 空き容量チェック機能は既に実装済み
- FileNameConverterクラスが既存、これを活用
- ConflictResolutionでAutoRenameオプションを改良
- 各機能実装後は必ずテストケースを追加
