# N88Basic ファイルシステム実装 - 引き継ぎドキュメント

**実装開始**: 2025年5月26日  
**対象**: PC-8801 N88-BASICファイルシステム実装  
**ブランチ**: feature/n88basic-filesystem  
**工数見積もり**: 20-26時間 / 22,000-28,000トークン

---

## 📋 実装進捗状況

### ✅ 完了済み
- [x] developブランチ作成
- [x] feature/n88basic-filesystemブランチ作成
- [x] 引き継ぎドキュメント作成
- [x] **Phase 1**: 技術調査・仕様策定 (2時間)
  - [x] N88Basic仕様資料読解
  - [x] Hu-BASICとの差分分析完了 (`N88Basic_vs_HuBasic_Analysis.md`)
  - [x] 実装方針決定 (独立実装、設定駆動アーキテクチャ)

### ✅ 完了済み (続き)
- [x] **Phase 2**: ドメイン実装 (2.5-3時間)
  - [x] N88BasicFileEntry.cs作成 (16バイト構造、属性ビット操作)
  - [x] N88BasicConfiguration.cs作成 (ディスクタイプ別設定、物理アドレス計算)
  - [x] エントリ変換ロジック実装 (FromBytes/ToBytes)

### 🔄 現在作業中
- [ ] **Phase 3**: インフラストラクチャ実装 (8-10.5時間)
  - [ ] N88BasicFileSystem.cs核心実装
  - [ ] N88BasicFileNameValidator.cs実装
  - [ ] FileSystemFactory拡張

### ⏳ 予定
- [ ] **Phase 3**: インフラストラクチャ実装 (8-10.5時間) 
- [ ] **Phase 4**: アプリケーション層統合 (1.5-2時間)
- [ ] **Phase 5**: CLI統合 (1時間)
- [ ] **Phase 6**: テスト・検証 (5-7時間)
- [ ] **Phase 7**: ドキュメント更新 (2-3時間)

---

## 🔍 技術仕様まとめ

### N88-BASIC vs Hu-BASIC 主要差分

| 項目 | N88-BASIC | Hu-BASIC | 差分 |
|------|-----------|----------|------|
| **ディレクトリエントリサイズ** | 16バイト | 32バイト | ▼半分 |
| **ファイル名構造** | 6文字+3文字拡張子 | 8文字+3文字拡張子 | ▼2文字短い |
| **クラスタサイズ(2D)** | 8セクタ/2KB | 16セクタ/4KB | ▼半分 |
| **クラスタサイズ(2DD)** | 16セクタ/4KB | 16セクタ/4KB | 同じ |
| **システムトラック(2D)** | T18/H1 | T1/H1 | ▲異なる位置 |
| **システムトラック(2DD)** | T40/H0 | T1/H1 | ▲異なる位置 |
| **FAT終端マーカー** | C0h+使用セクタ数 | 80h-8Fh | ▲異なる形式 |
| **ディレクトリ領域** | S1-12 | S1-10 | ▲セクタ範囲異なる |
| **FAT領域** | S14-16 | S11-14 | ▲セクタ範囲異なる |

### N88-BASIC固有仕様

**ディレクトリエントリ構造 (16バイト)**:
```
バイト 0-5:   ファイル名 (6文字)
バイト 6-8:   拡張子 (3文字)
バイト 9:     ファイル属性
バイト 10:    開始クラスタ番号
バイト 11-15: システム予約
```

**ファイル属性ビット**:
- ビット0: バイナリフォーマット
- ビット4: 書き込み禁止
- ビット5: 編集禁止
- ビット7: ASCII(0) / トークン化BASIC(1)

---

## 🎯 実装戦略

### Phase別アプローチ
1. **Phase 1-2**: 基本構造実装（Hu-BASICテンプレート活用）
2. **Phase 3**: N88Basic固有ロジック実装
3. **Phase 4-5**: 既存システム統合
4. **Phase 6-7**: 検証・文書化

### 実装優先度
- **High**: Phase 1-3 (コア機能)
- **Medium**: Phase 4-5 (統合)
- **Low**: Phase 6-7 (検証・文書)

---

## 📁 実装予定ファイル

### 新規作成
- `FileSystem/Domain/Model/N88BasicFileEntry.cs`
- `FileSystem/Infrastructure/FileSystem/N88BasicFileSystem.cs`
- `FileSystem/Infrastructure/Utility/N88BasicFileNameValidator.cs`
- `Test/N88BasicFileSystemTest.cs`

### 修正予定
- `FileSystem/Domain/Interface/FileSystem/IFileSystem.cs`
- `FileSystem/Infrastructure/Factory/FileSystemFactory.cs`
- `FileSystem/Application/FileSystemService.cs`
- `DependencyInjection/ServiceCollectionExtensions.cs`
- `Legacy89DiskKit.CLI/Program.cs`
- `Test/ComprehensiveTestSuite.cs`

---

## ⚠️ 留意事項

### 技術的課題
1. **クラスタマッピング**: システムトラックスキップロジック
2. **FAT終端判定**: C0h+使用セクタ数の正確な処理
3. **ディスクタイプ判定**: 2D/2DDでの異なるレイアウト

### テスト戦略
1. **段階的テスト**: Phase完了ごとの動作確認
2. **既存テスト活用**: ComprehensiveTestSuite拡張
3. **手動検証**: 実PC-8801ディスクイメージでの確認

---

## 🔄 フリーズ時の復旧手順

1. 現在のブランチ確認: `git branch`
2. 作業状況確認: このドキュメントのチェックリスト
3. 最新コミット確認: `git log --oneline -5`
4. Todo状況確認: TodoReadツール実行
5. 該当Phaseから再開

---

**最終更新**: 2025年5月26日 Phase 1開始時