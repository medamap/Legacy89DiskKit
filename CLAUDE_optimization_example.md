# CLAUDE.md 最適化例

## 🎯 メイン情報（CLAUDE.md）
```markdown
# Legacy89DiskKit プロジェクト

## 概要
1980-90年代の日本製レトロPCディスクイメージ操作ライブラリ

## クイックリファレンス
- ビルド: `dotnet build`
- テスト: `dotnet test`  
- CLI実行: `dotnet run --project Legacy89DiskKit.CLI`

## 詳細ドキュメント
プロジェクトの詳細は以下を参照：
- アーキテクチャ: `Documents/Architecture.md`
- API仕様: `Documents/API_Reference.md`
- 実装履歴: `Documents/Implementation_History.md`
- フォーマット仕様: `Documents/Formats/`

## 重要な注意事項
- 書き込み操作には`--filesystem`指定が必須
- エラー率1-3%未満を維持
```

## 📁 分割ファイル構成案

### Documents/Architecture.md
- DDD構造の詳細
- 各ドメインの責務
- 依存関係

### Documents/Formats/
- D88_Format.md
- FAT12_Format.md
- Hu-BASIC_Format.md
- MSX-DOS_Format.md
- N88Basic_Format.md

### Documents/Development/
- Build_Instructions.md
- Testing_Guide.md
- Contributing.md

## 💡 効果
- **CLAUDE.md**: 約1KB（必要最小限）
- **詳細情報**: 必要時にReadツールで読み込み
- **トークン節約**: 90%以上削減可能

## 🔧 実装方法
1. 現在のCLAUDE.mdを分割
2. 各トピックを専用ファイルに
3. CLAUDE.mdにはパスのみ記載
4. 私が必要に応じて読み込み