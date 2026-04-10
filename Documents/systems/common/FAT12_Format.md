# MS-DOS FAT12 ファイルシステム仕様

**対象**: MS-DOS 1.0-6.22, PC-DOS, および互換システム  
**ディスクタイプ**: フロッピーディスク（360KB, 720KB, 1.2MB, 1.44MB等）  
**実装状況**: Legacy89DiskKit Phase 5で読み取り専用対応完了

---

## 📋 概要

FAT12 (File Allocation Table 12-bit) は、Microsoft DOS で最初に使用されたファイルシステムです。12ビットのFATエントリを使用し、最大4085個のクラスタを管理できます。主にフロッピーディスクで使用されていました。

## 🗂️ ディスク構造

### 全体レイアウト
```
+------------------+
| ブートセクタ     | セクタ 0
+------------------+
| FAT #1          | セクタ 1-9 (典型例)
+------------------+
| FAT #2 (コピー) | セクタ 10-18
+------------------+
| ルートディレクトリ| セクタ 19-32
+------------------+
| データ領域       | セクタ 33-
+------------------+
```

## 🥾 ブートセクタ (セクタ 0)

### 構造 (512バイト)

| オフセット | サイズ | フィールド名 | 説明 |
|-----------|--------|-------------|------|
| 0x00 | 3 | JumpInstruction | ジャンプ命令 (0xEB, 0x??, 0x90) |
| 0x03 | 8 | OemName | OEM名 (例: "MSDOS5.0") |
| 0x0B | 2 | BytesPerSector | セクタあたりバイト数 (通常512) |
| 0x0D | 1 | SectorsPerCluster | クラスタあたりセクタ数 |
| 0x0E | 2 | ReservedSectors | 予約セクタ数 (通常1) |
| 0x10 | 1 | NumberOfFats | FATの数 (通常2) |
| 0x11 | 2 | RootEntries | ルートディレクトリエントリ数 |
| 0x13 | 2 | TotalSectors16 | 総セクタ数 (16ビット) |
| 0x15 | 1 | MediaType | メディアタイプ (0xF0=3.5"、0xF9=5.25") |
| 0x16 | 2 | SectorsPerFat | FATあたりセクタ数 |
| 0x18 | 2 | SectorsPerTrack | トラックあたりセクタ数 |
| 0x1A | 2 | NumberOfHeads | ヘッド数 |
| 0x1C | 4 | HiddenSectors | 隠しセクタ数 |
| 0x20 | 4 | TotalSectors32 | 総セクタ数 (32ビット) |
| 0x24 | 1 | DriveNumber | ドライブ番号 |
| 0x25 | 1 | Reserved1 | 予約 |
| 0x26 | 1 | BootSignature | ブート署名 (0x29) |
| 0x27 | 4 | VolumeId | ボリュームID |
| 0x2B | 11 | VolumeLabel | ボリュームラベル |
| 0x36 | 8 | FileSystemType | ファイルシステム名 ("FAT12   ") |
| 0x3E | 448 | BootCode | ブートコード |
| 0x1FE | 2 | BootSignature | ブート署名 (0x55, 0xAA) |

## 🗃️ FAT (File Allocation Table)

### FAT12の特徴

- **12ビットエントリ**: 各クラスタ番号を12ビットで表現
- **パック形式**: 3バイトで2つのエントリを格納
- **範囲**: 0x002-0xFEF (有効クラスタ番号)

### エントリ値の意味

| 値 | 意味 |
|----|------|
| 0x000 | 未使用クラスタ |
| 0x001 | 予約 |
| 0x002-0xFEF | 次のクラスタ番号 |
| 0xFF0-0xFF6 | 予約 |
| 0xFF7 | 不良クラスタ |
| 0xFF8-0xFFF | ファイル終端 |

### FAT12エントリ読み取り例

```csharp
int GetNextCluster(int currentCluster)
{
    // クラスタ番号からFATオフセット計算
    var fatOffset = currentCluster + (currentCluster / 2);
    
    int nextCluster;
    if (currentCluster % 2 == 0)
    {
        // 偶数クラスタ: 下位12ビット使用
        nextCluster = _fatTable[fatOffset] | ((_fatTable[fatOffset + 1] & 0x0F) << 8);
    }
    else
    {
        // 奇数クラスタ: 上位12ビット使用
        nextCluster = (_fatTable[fatOffset] >> 4) | (_fatTable[fatOffset + 1] << 4);
    }
    
    return nextCluster & 0xFFF;
}
```

## 📁 ディレクトリエントリ

### 構造 (32バイト)

| オフセット | サイズ | フィールド名 | 説明 |
|-----------|--------|-------------|------|
| 0x00 | 8 | FileName | ファイル名 (8.3形式の名前部分) |
| 0x08 | 3 | Extension | 拡張子 |
| 0x0B | 1 | Attributes | ファイル属性 |
| 0x0C | 1 | Reserved | 予約 |
| 0x0D | 1 | CreationTimeTenths | 作成時刻（10分の1秒） |
| 0x0E | 2 | CreationTime | 作成時刻 |
| 0x10 | 2 | CreationDate | 作成日付 |
| 0x12 | 2 | LastAccessDate | 最終アクセス日付 |
| 0x14 | 2 | FirstClusterHigh | 開始クラスタ上位16ビット (FAT32用) |
| 0x16 | 2 | WriteTime | 更新時刻 |
| 0x18 | 2 | WriteDate | 更新日付 |
| 0x1A | 2 | FirstCluster | 開始クラスタ下位16ビット |
| 0x1C | 4 | FileSize | ファイルサイズ |

### ファイル属性ビット

| ビット | 値 | 意味 |
|-------|----|----- |
| 0 | 0x01 | 読み取り専用 |
| 1 | 0x02 | 隠しファイル |
| 2 | 0x04 | システムファイル |
| 3 | 0x08 | ボリュームラベル |
| 4 | 0x10 | サブディレクトリ |
| 5 | 0x20 | アーカイブ |
| 6-7 | - | 予約 |

### 特殊エントリ

- **0xE5**: 削除されたファイル
- **0x00**: ディレクトリ終端
- **0x05**: 実際のファイル名が0xE5で始まる場合

## 📅 日時フォーマット

### DOS日付形式 (16ビット)

| ビット | 範囲 | 意味 |
|-------|------|------|
| 15-9 | 0-119 | 年 (1980年からの経過年数) |
| 8-5 | 1-12 | 月 |
| 4-0 | 1-31 | 日 |

### DOS時刻形式 (16ビット)

| ビット | 範囲 | 意味 |
|-------|------|------|
| 15-11 | 0-23 | 時 |
| 10-5 | 0-59 | 分 |
| 4-0 | 0-29 | 秒 ÷ 2 |

### 日時変換例

```csharp
DateTime GetModifiedDate(ushort dosDate, ushort dosTime)
{
    if (dosDate == 0) return DateTime.MinValue;
    
    var year = 1980 + ((dosDate >> 9) & 0x7F);
    var month = (dosDate >> 5) & 0x0F;
    var day = dosDate & 0x1F;
    
    var hour = (dosTime >> 11) & 0x1F;
    var minute = (dosTime >> 5) & 0x3F;
    var second = (dosTime & 0x1F) * 2;
    
    return new DateTime(year, month, day, hour, minute, second);
}
```

## 💾 代表的なディスク仕様

### 5.25インチ 360KB

- **セクタサイズ**: 512バイト
- **セクタ/トラック**: 9
- **トラック数**: 40
- **ヘッド数**: 2
- **クラスタサイズ**: 2セクタ (1024バイト)
- **FATセクタ数**: 2
- **ルートエントリ数**: 112

### 3.5インチ 720KB

- **セクタサイズ**: 512バイト
- **セクタ/トラック**: 9
- **トラック数**: 80
- **ヘッド数**: 2
- **クラスタサイズ**: 2セクタ (1024バイト)
- **FATセクタ数**: 3
- **ルートエントリ数**: 112

### 3.5インチ 1.44MB

- **セクタサイズ**: 512バイト
- **セクタ/トラック**: 18
- **トラック数**: 80
- **ヘッド数**: 2
- **クラスタサイズ**: 1セクタ (512バイト)
- **FATセクタ数**: 9
- **ルートエントリ数**: 224

## 🔧 Legacy89DiskKit実装詳細

### 対応機能

✅ **読み取り機能**
- ブートセクタ解析
- FAT12テーブル読み込み
- ディレクトリ一覧表示
- ファイル読み込み
- 破損データ部分復旧

🚧 **未実装機能**
- ファイル書き込み
- ディレクトリ作成/削除
- ファイル削除
- フォーマット

### 使用例

```bash
# FAT12ディスクの自動検出
./Legacy89DiskKit.CLI list disk.dsk

# 明示的なファイルシステム指定
./Legacy89DiskKit.CLI list disk.dsk fat12

# ファイル読み出し
./Legacy89DiskKit.CLI export-text disk.dsk README.TXT readme.txt
./Legacy89DiskKit.CLI export-binary disk.dsk PROGRAM.EXE program.exe
```

### 自動検出ロジック

1. **ブートセクタ署名チェック**: 0x55AA at offset 0x1FE
2. **ファイルシステム名確認**: "FAT12" または "FAT" at offset 0x36
3. **構造的妥当性検証**: BytesPerSector, SectorsPerCluster, NumberOfFats

## 🌍 互換性

### 対応OS/システム

- MS-DOS 1.0 - 6.22
- PC-DOS 1.0 - 7.0
- Windows 9x/ME
- OS/2 1.x
- FreeDOS
- 各種DOSクローン

### ディスクイメージ形式

- **DSK**: 生セクタイメージ（MSX, CPC等）
- **IMG**: PC標準イメージ
- **IMA**: WinImage形式
- **VFD**: Virtual Floppy Disk

---

**参考資料**:
- Microsoft FAT32 File System Specification
- "MS-DOS Encyclopedia" Microsoft Press
- "Undocumented DOS" Andrew Schulman