# N88-BASIC vs Hu-BASIC ファイルシステム差分分析

**分析日**: 2025年5月26日  
**目的**: N88BasicFileSystem実装のための詳細仕様比較

---

## 📊 構造比較表

### ディスクレイアウト

| 項目 | N88-BASIC | Hu-BASIC | 実装影響度 |
|------|-----------|----------|-----------|
| **システムトラック(2D)** | T18/H1 | T1/H1 | 🔴 High |
| **システムトラック(2DD)** | T40/H0 | T1/H1 | 🔴 High |
| **ディレクトリ領域** | S1-12 (12セクタ) | S1-10 (10セクタ) | 🟡 Medium |
| **FAT領域** | S14-16 (3セクタ) | S11-14 (4セクタ) | 🟡 Medium |
| **IDセクタ** | S13 | なし | 🟢 Low |

### ファイルエントリ構造

| 項目 | N88-BASIC | Hu-BASIC | 実装影響度 |
|------|-----------|----------|-----------|
| **エントリサイズ** | 16バイト | 32バイト | 🔴 High |
| **ファイル名長** | 6文字 | 8文字 | 🟡 Medium |
| **拡張子長** | 3文字 | 3文字 | 同じ |
| **属性位置** | バイト9 | バイト16 | 🟡 Medium |
| **開始クラスタ位置** | バイト10 | バイト17 | 🟡 Medium |
| **サイズ情報** | なし | バイト24-27 | 🟡 Medium |

### クラスタ管理

| 項目 | N88-BASIC | Hu-BASIC | 実装影響度 |
|------|-----------|----------|-----------|
| **2D クラスタサイズ** | 8セクタ/2KB | 16セクタ/4KB | 🔴 High |
| **2DD クラスタサイズ** | 16セクタ/4KB | 16セクタ/4KB | 同じ |
| **FAT終端マーカー** | C0h+使用セクタ数 | 80h-8Fh | 🔴 High |
| **空きマーカー** | FFh | 00h | 🟡 Medium |
| **システム予約** | FEh | なし | 🟢 Low |

---

## 🏗️ 実装アーキテクチャ設計

### 基本方針
1. **継承ベース**: `HuBasicFileSystem`をベースクラスとして活用せず、独立実装
2. **共通インターフェース**: `IFileSystem`を実装し一貫性保持
3. **設定駆動**: `N88BasicConfiguration`クラスで仕様差分を吸収

### N88BasicConfiguration 設計

```csharp
public class N88BasicConfiguration
{
    // システム位置情報
    public int SystemTrack2D { get; init; } = 18;      // 2D: T18/H1
    public int SystemHead2D { get; init; } = 1;
    public int SystemTrack2DD { get; init; } = 40;     // 2DD: T40/H0  
    public int SystemHead2DD { get; init; } = 0;
    
    // ディレクトリ・FAT領域
    public int DirectoryStartSector { get; init; } = 1;    // S1-12
    public int DirectorySectorCount { get; init; } = 12;
    public int FatStartSector { get; init; } = 14;         // S14-16
    public int FatSectorCount { get; init; } = 3;
    public int IdSector { get; init; } = 13;               // S13
    
    // ファイルエントリ
    public int EntrySize { get; init; } = 16;              // 16バイト
    public int FileNameLength { get; init; } = 6;          // 6文字
    public int ExtensionLength { get; init; } = 3;         // 3文字
    
    // クラスタ定義
    public int ClusterSize2D { get; init; } = 8 * 256;     // 8セクタ/2KB
    public int ClusterSize2DD { get; init; } = 16 * 256;   // 16セクタ/4KB
    
    // FAT値
    public byte FatFreeMarker { get; init; } = 0xFF;       // 空き
    public byte FatEofBase { get; init; } = 0xC0;          // 終端ベース
    public byte FatReservedMarker { get; init; } = 0xFE;   // システム予約
}
```

### N88BasicFileEntry 設計

```csharp
public class N88BasicFileEntry
{
    public string FileName { get; set; } = string.Empty;    // 6文字
    public string Extension { get; set; } = string.Empty;   // 3文字
    public byte Attributes { get; set; }                    // 属性バイト
    public byte StartCluster { get; set; }                  // 開始クラスタ
    
    // 属性ビット操作
    public bool IsBinary => (Attributes & 0x01) != 0;              // ビット0
    public bool IsWriteProtected => (Attributes & 0x10) != 0;      // ビット4
    public bool IsEditProtected => (Attributes & 0x20) != 0;       // ビット5
    public bool IsTokenizedBasic => (Attributes & 0x80) != 0;      // ビット7
    
    public bool IsDeleted { get; set; }    // 削除済み (バイト0 = 0x00)
    public bool IsEmpty { get; set; }      // 未使用 (バイト0 = 0xFF)
    
    // サイズは実行時にFATチェーンから計算
    public int CalculatedSize { get; set; }
}
```

---

## 🎯 実装戦略

### Phase 2: ドメインモデル実装
1. **N88BasicFileEntry.cs**: ファイルエントリ構造定義
2. **N88BasicConfiguration.cs**: 設定クラス実装
3. **エントリ変換ロジック**: 16バイト ↔ N88BasicFileEntry

### Phase 3: ファイルシステム実装
1. **基本構造**: コンストラクタ、設定初期化
2. **システム位置計算**: ディスクタイプ別システムトラック解決
3. **ディレクトリ操作**: 16バイトエントリの読み書き
4. **FAT操作**: C0h+使用セクタ数終端処理
5. **クラスタマッピング**: 2D/2DD別クラスタサイズ対応
6. **ファイル操作**: 読み書き、削除、作成

### 重要な実装ポイント

#### 1. システムトラック位置解決
```csharp
private (int track, int head) GetSystemLocation(DiskType diskType)
{
    return diskType switch
    {
        DiskType.TwoD => (18, 1),    // T18/H1
        DiskType.TwoDD => (40, 0),   // T40/H0
        _ => throw new NotSupportedException($"N88-BASIC不対応: {diskType}")
    };
}
```

#### 2. FAT終端マーカー処理
```csharp
private bool IsEofCluster(byte fatValue, int usedSectors)
{
    // C0h + 使用セクタ数 = 終端マーカー
    return fatValue == (0xC0 | (usedSectors & 0x3F));
}
```

#### 3. クラスタマッピング
```csharp
private int GetClusterSize(DiskType diskType)
{
    return diskType switch
    {
        DiskType.TwoD => 8 * 256,     // 2KB
        DiskType.TwoDD => 16 * 256,   // 4KB
        _ => throw new NotSupportedException()
    };
}
```

---

## ⚠️ 技術的課題と対策

### 1. ディレクトリエントリサイズ差分
**課題**: 16バイト vs 32バイト  
**対策**: N88BasicFileEntry専用クラスで抽象化

### 2. FAT終端マーカー形式
**課題**: C0h+使用セクタ数の可変長  
**対策**: 専用解析ロジック実装

### 3. システムトラック位置
**課題**: 2D/2DDで異なる位置  
**対策**: ディスクタイプ別設定テーブル

### 4. クラスタサイズ差分  
**課題**: 2Dのみ半分サイズ  
**対策**: ディスクタイプ別計算ロジック

---

## 📋 検証項目

### 機能テスト
- [ ] ディスクタイプ判定 (2D/2DD)
- [ ] システムトラック位置解決
- [ ] ディレクトリエントリ読み書き
- [ ] FATクラスタチェーン追跡
- [ ] ファイル読み書き操作
- [ ] ファイル削除操作

### 互換性テスト
- [ ] 実PC-8801ディスクイメージ読み込み
- [ ] N88-BASICで作成したファイルアクセス
- [ ] 異なるディスクサイズでの動作確認

---

**分析完了**: Phase 1 - 技術調査完了  
**次ステップ**: Phase 2 - ドメインモデル実装開始