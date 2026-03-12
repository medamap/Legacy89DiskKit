# ディスクコピー機能実装計画

## 概要
Legacy89DiskKitにおけるディスクイメージ間のデータコピー・移動機能の実装計画書です。

## 機能分類

### 1. ファイルシステムレベルのファイル操作
ファイルシステムの構造を理解した上でのファイル単位の操作。

#### 1.1 基本操作
- **コピー（COPY）**: ソースからデスティネーションへファイルを複製
- **移動（MOVE）**: ソースからデスティネーションへファイルを移動（コピー後削除）
- **削除（DELETE）**: ファイルの削除

#### 1.2 拡張機能
- **ワイルドカード対応**: `*.TXT`, `GAME?.BAS` などのパターンマッチング
- **バッチ処理**: 複数ファイルの一括操作
- **再帰処理**: サブディレクトリ対応（将来的な拡張）

### 2. ディスクイメージレベルの丸ごとコピー
ディスクイメージ形式を変換しながら、全セクタをコピー。

#### 2.1 基本仕様
- **セクタ単位コピー**: ファイルシステムを意識せずに全セクタをコピー
- **形式変換**: 2D → D88、D88 → DSK など
- **メタデータ保持**: ボリュームラベル、ブート情報などを可能な限り保持

#### 2.2 制約事項
- **同一容量制限**: 基本的に同じディスクタイプ（2D→2D、2DD→2DD）
- **下位互換**: 大容量→小容量は原則不可（データロスの可能性）

## 実装上の課題と解決策

### 課題1: 異なるファイルシステム間のコピー

#### ファイル名規則の違い
| ファイルシステム | ファイル名規則 | 拡張子 |
|-----------------|---------------|--------|
| Hu-BASIC | 8+3文字 | 3文字 |
| N88-BASIC | 6+3文字 | なし |
| FAT12/MSX-DOS | 8+3文字 | 3文字 |
| CP/M | 8+3文字 | 3文字 |

#### 解決策：ファイル名変換ルール
```
例：LONGFILENAME.TXT (13文字) → LONGF001.TXT (8+3文字)

アルゴリズム：
1. ベース名が制限を超える場合、先頭N文字を取得
2. 末尾に3桁の連番を付与（001〜999）
3. 既存ファイルと重複した場合、連番をインクリメント
4. 拡張子は可能な限り保持（3文字に切り詰め）
```

### 課題2: 容量制限

#### ディスクタイプ別容量
| タイプ | 容量 | セクタ構成 |
|--------|------|------------|
| 2D | 320KB | 40×2×16×256B |
| 2DD | 640-720KB | 80×2×9×512B |
| 2HD | 1.2-1.44MB | 80×2×15-18×512B |

#### 解決策：動的容量チェック
```csharp
public class DiskSpaceManager
{
    public bool CanCopyFile(IFileSystem source, IFileSystem dest, string fileName)
    {
        var fileSize = source.GetFileSize(fileName);
        var freeSpace = dest.GetFreeSpace();
        
        // ディレクトリエントリ分も考慮
        var requiredSpace = fileSize + GetDirectoryEntrySize(dest.FileSystemType);
        
        return freeSpace >= requiredSpace;
    }
}
```

### 課題3: 特殊属性の扱い

#### ファイルシステム別属性
- **Hu-BASIC**: BIN/BAS/ASC、Read-Only、Hidden
- **N88-BASIC**: Machine Language/BASIC/ASCII、Write Protect
- **FAT12**: Archive、Hidden、System、Read-Only
- **CP/M**: Read-Only、System

#### 解決策：属性マッピングテーブル
```csharp
public class AttributeMapper
{
    private Dictionary<(FileSystemType, FileSystemType), Func<FileAttributes, FileAttributes>> _mappings;
    
    public FileAttributes MapAttributes(FileSystemType source, FileSystemType dest, FileAttributes attr)
    {
        // 共通属性（Read-Only）は保持
        // 特殊属性は可能な限りマッピング
        // マッピング不可能な属性は破棄
    }
}
```

## 実装優先順位

### Phase 1: 基本的なファイルコピー機能（優先度：高）
1. **単一ファイルコピー**
   - 同一ファイルシステム間
   - CLIコマンド実装
   - エラーハンドリング

2. **ファイル名変換ロジック**
   - 8+3制限への対応
   - 重複回避アルゴリズム

3. **容量チェック**
   - コピー前の空き容量確認
   - 適切なエラーメッセージ

### Phase 2: ワイルドカード対応（優先度：中）
1. **パターンマッチング実装**
   - `*` と `?` のサポート
   - 複数ファイル選択

2. **バッチ処理**
   - 進捗表示
   - エラー時の継続/中断オプション

### Phase 3: ディスク丸ごとコピー（優先度：中）
1. **同一容量間のコピー**
   - 2D → 2D (in D88)
   - セクタ単位の高速コピー

2. **異なる形式への変換**
   - 2D → D88
   - D88 → DSK
   - メタデータ変換

### Phase 4: インタラクティブシェル統合（優先度：高）
1. **シェルコマンド拡張**
   - `copy` コマンドの改良
   - `move` コマンドの実装
   - `delete` コマンドのワイルドカード対応

2. **ユーザビリティ向上**
   - Tab補完の拡張
   - 確認プロンプト
   - 操作のUndo（将来的）

### Phase 5: 高度な機能（優先度：低）
1. **ディレクトリ対応**
   - サブディレクトリのコピー
   - 再帰的操作

2. **フィルタリング**
   - 日付による選択
   - 属性による選択

## CLIコマンド設計

### ファイル操作コマンド
```bash
# 単一ファイルコピー
./CLI copy source.d88 dest.2d GAME.BAS --source-fs hu-basic --dest-fs hu-basic

# ワイルドカードコピー
./CLI copy source.d88 dest.d88 "*.TXT" --source-fs fat12 --dest-fs msx-dos

# ファイル移動
./CLI move source.d88 dest.d88 README.TXT --source-fs hu-basic --dest-fs hu-basic

# ファイル削除（ワイルドカード対応）
./CLI delete disk.d88 "*.BAK" --filesystem hu-basic
```

### ディスクコンバートコマンド
```bash
# ディスク丸ごとコピー（形式変換）
./CLI convert source.2d dest.d88 --type 2D

# セクタレベルコピー（同一形式）
./CLI clone source.d88 dest.d88 --type 2DD
```

### インタラクティブシェル拡張
```bash
# 現在のシェル
Legacy89DiskKit [0:source.2d]> copy GAME.BAS 1:

# 拡張後のシェル
Legacy89DiskKit [0:source.2d]> copy *.TXT 1:
Copying 5 files...
  README.TXT -> 1:README.TXT [OK]
  MANUAL.TXT -> 1:MANUAL.TXT [OK]
  VERYLONGFILENAME.TXT -> 1:VERYLO01.TXT [Renamed]
  ...
```

## エラーハンドリング

### エラーケース
1. **容量不足**
   - 明確なエラーメッセージ
   - 必要容量と空き容量の表示

2. **ファイル名重複**
   - 自動リネーム
   - 上書き確認オプション

3. **読み取り/書き込みエラー**
   - セクタレベルエラーの詳細表示
   - 部分的な成功の報告

### エラーメッセージ例
```
Error: Insufficient disk space
  Required: 15,360 bytes
  Available: 10,240 bytes
  
Error: File name too long for destination filesystem
  Source: VERYLONGFILENAME.TXT (16 characters)
  Destination limit: 8 characters
  Renamed to: VERYLO01.TXT
  
Warning: 3 files could not be copied due to space constraints
  Use --verbose for detailed information
```

## テスト計画

### 単体テスト
1. ファイル名変換ロジック
2. 容量計算
3. ワイルドカードマッチング

### 統合テスト
1. 異なるファイルシステム間のコピー
2. 大量ファイルのバッチ処理
3. エラー状況のシミュレーション

### 実機相当テスト
1. 実際のディスクイメージでの動作確認
2. パフォーマンス測定
3. メモリ使用量の監視

## まとめ

この実装計画に基づいて、以下の順序で開発を進めることを推奨します：

1. **Phase 1**: 基本的なファイルコピー機能（2週間）
2. **Phase 4**: インタラクティブシェル統合（1週間）
3. **Phase 2**: ワイルドカード対応（1週間）
4. **Phase 3**: ディスク丸ごとコピー（1週間）

これにより、約1ヶ月で実用的なディスクコピー機能が完成します。