# Legacy89DiskKit リリース手順

## 概要
このドキュメントでは、Legacy89DiskKitの新バージョンをリリースする際の標準的な手順を説明します。

## リリース前の準備

### 1. バージョン番号の決定
セマンティックバージョニング（SemVer）に従います：
- **メジャーバージョン（x.0.0）**: 破壊的変更がある場合
- **マイナーバージョン（1.x.0）**: 新機能追加（後方互換性あり）
- **パッチバージョン（1.2.x）**: バグ修正

例：
- v1.2.0 → v1.3.0：新しいファイルシステム追加
- v1.3.0 → v1.3.1：バグ修正のみ

### 2. ブランチの準備
```bash
# feature/xxx-supportブランチで開発完了後
git checkout develop
git merge feature/xxx-support --no-ff

# developからmainへマージ
git checkout main
git merge develop --no-ff -m "Merge branch 'develop' for vX.Y.Z release"
```

## リリース作業

### 1. ドキュメントの更新

#### README.md の更新
```bash
# トップレベルのREADME.mdを編集
vim README.md
```

更新箇所：
- バージョンバッジ：`[![Version](https://img.shields.io/badge/Version-vX.Y.Z-blue?style=flat)]`
- 対応フォーマットセクション：新機能を追加
- 完成済み機能セクション：新しいPhaseを追加
- その他関連する箇所

### 2. リリースノートの作成

```bash
# リリースノートファイルを作成
vim RELEASE_NOTES_vX.Y.Z.md
```

テンプレート：
```markdown
# Legacy89DiskKit vX.Y.Z - [機能名]

## 🎉 新機能

### [主要機能の説明]
[詳細な説明]

#### 主な機能
- **機能1**: 説明
- **機能2**: 説明
- **機能3**: 説明

#### CLIコマンド例
```bash
# コマンド例
./CLI [コマンド]
```

## ⚠️ 既知の制限事項

### [制限事項のタイトル]
[制限事項の詳細説明]

## 📊 テスト結果
- テスト結果のサマリー
- 既存機能への影響

## 🔧 技術的な改善
- 改善点1
- 改善点2

## 📚 ドキュメント
- 追加/更新されたドキュメントへのリンク

## 🙏 謝辞
貢献者への感謝

---

**フルチェンジログ**: [vX.Y-1.Z...vX.Y.Z](https://github.com/medamap/Legacy89DiskKit/compare/vX.Y-1.Z...vX.Y.Z)
```

### 3. Gitタグの作成

```bash
# 日本語でアノテートタグを作成
git tag -a vX.Y.Z -m "リリース vX.Y.Z: [主要な変更内容]

- 主な変更点1
- 主な変更点2
- 主な変更点3"

# 例：
git tag -a v1.3.0 -m "リリース v1.3.0: CP/Mファイルシステムサポート追加

- CP/M 2.2ファイルシステムの完全実装
- 2D、2DD、2HDディスクタイプのサポート
- CP/M Generic、PC-8801、X1、MSX-DOS用文字エンコーディング対応"
```

### 4. 変更のプッシュ

```bash
# mainブランチをプッシュ
git push origin main

# タグをプッシュ
git push origin vX.Y.Z
```

## バイナリのビルド

### 1. ビルド環境の準備
```bash
# CSharpディレクトリへ移動
cd CSharp
```

### 2. 各プラットフォーム向けビルド

#### Windows x64
```bash
dotnet publish Legacy89DiskKit.CLI -c Release -r win-x64 \
  --self-contained -p:PublishSingleFile=true -o ../publish/win-x64
```

#### Linux x64
```bash
dotnet publish Legacy89DiskKit.CLI -c Release -r linux-x64 \
  --self-contained -p:PublishSingleFile=true -o ../publish/linux-x64
```

#### macOS x64
```bash
dotnet publish Legacy89DiskKit.CLI -c Release -r osx-x64 \
  --self-contained -p:PublishSingleFile=true -o ../publish/osx-x64
```

#### macOS ARM64
```bash
dotnet publish Legacy89DiskKit.CLI -c Release -r osx-arm64 \
  --self-contained -p:PublishSingleFile=true -o ../publish/osx-arm64
```

### 3. パッケージの作成

```bash
# 親ディレクトリへ移動
cd ..

# リリースディレクトリを作成
mkdir -p release/vX.Y.Z

# Windows版 (ZIP)
cd publish/win-x64
zip -r ../../release/Legacy89DiskKit-vX.Y.Z-win-x64.zip .

# Linux版 (tar.gz)
cd ../../publish/linux-x64
tar czf ../../release/Legacy89DiskKit-vX.Y.Z-linux-x64.tar.gz .

# macOS x64版 (tar.gz)
cd ../../publish/osx-x64
tar czf ../../release/Legacy89DiskKit-vX.Y.Z-osx-x64.tar.gz .

# macOS ARM64版 (tar.gz)
cd ../../publish/osx-arm64
tar czf ../../release/Legacy89DiskKit-vX.Y.Z-osx-arm64.tar.gz .
```

## GitHubリリースの作成

### 1. リリースの作成とバイナリアップロード

```bash
# releaseディレクトリへ移動
cd ../../release

# GitHubリリースを作成（リリースノートファイルを使用）
gh release create vX.Y.Z \
  --title "Legacy89DiskKit vX.Y.Z - [機能名]" \
  --notes-file ../RELEASE_NOTES_vX.Y.Z.md

# バイナリをアップロード
gh release upload vX.Y.Z \
  Legacy89DiskKit-vX.Y.Z-win-x64.zip \
  Legacy89DiskKit-vX.Y.Z-linux-x64.tar.gz \
  Legacy89DiskKit-vX.Y.Z-osx-x64.tar.gz \
  Legacy89DiskKit-vX.Y.Z-osx-arm64.tar.gz
```

### 2. リリースの確認

```bash
# リリースの内容を確認
gh release view vX.Y.Z

# ブラウザでリリースページを開く
open https://github.com/medamap/Legacy89DiskKit/releases/tag/vX.Y.Z
```

## チェックリスト

リリース前に以下を確認してください：

- [ ] すべてのテストが成功している
- [ ] README.mdのバージョン番号が更新されている
- [ ] リリースノートが作成されている
- [ ] developブランチがmainにマージされている
- [ ] Gitタグが作成されている
- [ ] 4つのプラットフォーム用バイナリがビルドされている
- [ ] GitHubリリースが公開されている
- [ ] バイナリがアップロードされている

## トラブルシューティング

### ビルドエラーが発生した場合
```bash
# パッケージの復元
dotnet restore

# クリーンビルド
dotnet clean
dotnet build -c Release
```

### gh コマンドが使えない場合
```bash
# GitHub CLIのインストール（macOS）
brew install gh

# 認証
gh auth login
```

### リリースを修正する場合
```bash
# リリースの編集
gh release edit vX.Y.Z --notes-file RELEASE_NOTES_vX.Y.Z.md

# アセットの削除と再アップロード
gh release delete-asset vX.Y.Z [ファイル名]
gh release upload vX.Y.Z [新しいファイル]
```

## 参考リンク

- [セマンティックバージョニング](https://semver.org/lang/ja/)
- [GitHub CLI ドキュメント](https://cli.github.com/manual/)
- [.NET CLI リファレンス](https://docs.microsoft.com/ja-jp/dotnet/core/tools/)

---

最終更新日: 2025年6月5日