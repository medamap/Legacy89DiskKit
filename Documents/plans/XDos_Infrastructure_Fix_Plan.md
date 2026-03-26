# X-DOS インフラ修正計画

## 背景

X-DOS の 2DD クロスコピーディスクが「System not found!」でブートしない問題を調査した。
Codex カーネル解析と既存コードの差分分析の結果、2つの根本バグとメディアタイプ対応の欠如が判明した。

---

## 現状分析：API1〜9 の状態

| API | メソッド | 場所 | 状態 |
|-----|---------|------|------|
| API1 | `GetFiles()` | IFileSystem / XDosFileSystem | ✓ 動作（ジオメトリ依存なし） |
| API2 | `ReadFile()` / `ReadFileRaw()` | XDosFileSystem | ✓ 動作（2D限定だが実害少） |
| API3 | `WriteFile()` / `WriteFileInternal()` | XDosFileSystem | ❌ バグあり（Bug 1） |
| API4 | `Format()` | XDosFileSystem | ❌ FAT[0] の初期化に関連 |
| API5 | `WriteBootArea()` | XDosFileSystem | ✓ 動作 |
| API6 | `WriteFileInternal()` (拡張版) | XDosFileSystem | ❌ クラスタ0割当バグ |
| API7 | `GetFilesWithMetadata()` | XDosFileSystem | ✓ 動作 |
| API8 | `ReadFileRaw()` | XDosFileSystem | ✓ 動作 |
| API9 | `GetFileSystemInfo()` | XDosFileSystem | △ 容量計算がジオメトリ非依存 |

---

## Bug 1（最重要）：クラスタ0が割り当てられてしまう

### 現象

`XDosFatWriter.ClearAll()` 後の FAT 状態：
- FAT[0] = 0x00（空き → **誤り**）
- FAT[1] = 0x01（予約済み）
- FAT[2] = 0x4A（使用中 ← FAM+bdir トラック）

`AllocateClusters()` が FAT[0]=0x00 を発見してクラスタ0を割り当てる。
クラスタ0 = C=0,H=0 = **ブートトラック！**

### 連鎖障害

1. `WriteFileInternal` がブートトラック（C=0,H=0,R=1..16）にファイルデータを書き込む
2. `WriteBootArea` がその後でブートコードで上書き
3. FAM チェーンはクラスタ0から始まるファイルエントリを指す
4. IPL がカーネルファイルを読もうとするとブートコードを読んでしまう
→ **「System not found!」**

### 補足

実際の X-DOS ディスク（Codex 解析済み）では FAT[0]=0x00 のままだが、
実 IPL はクラスタ0を使わない実装になっている。
我々の AllocateClusters にはそのロジックがない。

### 修正方針

`AllocateClusters()` の走査開始インデックスを 0 → 2 に変更する。

```csharp
// 変更前
for (int i = 0; i < Fat.Length; i++)

// 変更後
for (int i = 2; i < Fat.Length; i++)  // 0=ブート, 1=予約 をスキップ
```

- クラスタ0（ブートトラック）→ 絶対に割り当てない
- クラスタ1（FAT[1]=0x01）→ 既に予約フラグでスキップ済み
- クラスタ2（FAT[2]=0x4A）→ 既にスキップ済み
- クラスタ3以降 → 通常割り当て

**ファイル:** `csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFatWriter.cs`

---

## Bug 2：2HD ジオメトリがハードコード

### 現象

全コンポーネントがデータトラック = 10セクタ×512B で固定。
Codex 解析で確認済み：2HD は 16セクタ×512B。

| コンポーネント | 問題箇所 | ハードコード値 |
|-------------|---------|-------------|
| `XDosFileSystem.XDosTrackGeometry()` | static メソッド | 常に10 |
| `XDosFileSystem.WriteFileInternal()` | `maxR = ... : 10` | 常に10 |
| `XDosClusterReader` | `SectorsPerTrack = 10` | 常に10 |
| `XDosDirParser` | `for r = 2..10` | 常に10（変更不要） |
| `XDosDirWriter` | `LastDirR = 10` | 常に10（変更不要） |

ディレクトリ（R=2..10）は全メディアで共通。変更不要。

---

## Codex 解析から判明したメディア別構造

| 項目 | 2D | 2DD | 2HD |
|-----|----|----|-----|
| ブートトラック | C=0,H=0,R=1..16, 256B | 同じ | 同じ |
| FAT | C=0,H=1,R=1, 512B | 同じ | 同じ |
| FAM | C=1,H=0,R=1, 512B | 同じ | 同じ |
| ディレクトリ | C=0,H=1,R=2..10 | 同じ | 同じ |
| データトラック | 10セクタ×512B | 10セクタ×512B | **16セクタ×512B** |
| クラスタ容量 | 5120B | 5120B | **8192B** |
| ヒドゥン常駐エリア | C=1,H=0,R=2..10 | 同じ | 同じ |
| クラスタ→物理 | C=N/2, H=N%2 | 同じ | 同じ |

---

## アーキテクチャ設計：XDosMediaGeometry

### 新規ファイル

`csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosMediaGeometry.cs`

```csharp
public record XDosMediaGeometry(
    int DataSectorsPerTrack,
    int DataSectorSize,
    int BootSectorsPerTrack,
    int BootSectorSize
)
{
    public static XDosMediaGeometry FromDiskType(DiskType diskType) =>
        diskType == DiskType.TwoHD
            ? new(16, 512, 16, 256)
            : new(10, 512, 16, 256);

    public (int sectors, ushort size, byte density) GetTrackGeometry(int c, int h) =>
        (c == 0 && h == 0)
            ? (BootSectorsPerTrack, (ushort)BootSectorSize, (byte)0x00)
            : (DataSectorsPerTrack, (ushort)DataSectorSize, (byte)0x00);
}
```

### XDosFileSystem の変更

コンストラクタで DiskType を検出して geometry を初期化：

```csharp
public XDosFileSystem(IDiskContainer container)
{
    _geometry = XDosMediaGeometry.FromDiskType(container.DiskType);
    // ... 各 Writer/Reader に geometry を渡す
}
```

後方互換性のため static メソッドは残す：

```csharp
public static (int sectors, ushort size, byte density)? XDosTrackGeometry(int c, int h) =>
    XDosMediaGeometry.FromDiskType(DiskType.TwoD).GetTrackGeometry(c, h);
```

---

## ヒドゥン常駐エリア（bdir）について

「3コピーエリア」として別途 API が必要か検討したが、**不要**と結論。

- C=1,H=0,R=2..10（bdir_hidden）はカーネルファイルのクラスタチェーン（クラスタ2）の一部として `ReadFileRaw` で読み込まれる
- `WriteFileInternal` でオートアロケーション時、bdir の内容は別クラスタに書かれるが**バイナリ内容は同じ**
- IPL はクラスタチェーンを辿ってカーネルをRAMに読み込むため、物理アドレスは関係ない
- ボトルネックは bdir ではなく Bug 1（クラスタ0割当）

`WriteHiddenArea` / `ReadHiddenArea` は現時点で不要。物理セクタコピーモードが必要になった場合に検討。

---

## 修正対象ファイル一覧

| ファイル | 変更種別 | 優先度 |
|---------|---------|-------|
| `Infrastructure/FileSystem/XDos/XDosMediaGeometry.cs` | **新規** | 高 |
| `Infrastructure/FileSystem/XDos/Reader/XDosFatWriter.cs` | 修正（Bug 1: i=2 から走査） | **最高** |
| `Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs` | 修正（Bug 2: geometry 注入） | 高 |
| `Infrastructure/FileSystem/XDos/XDosFileSystem.cs` | 修正（geometry フィールド・WriteFileInternal） | 高 |
| `Infrastructure/FileSystem/XDos/Reader/XDosDirWriter.cs` | **変更なし** | - |
| `Infrastructure/FileSystem/XDos/Reader/XDosDirParser.cs` | **変更なし** | - |
| `Infrastructure/FileSystem/XDos/Reader/XDosFatReader.cs` | **変更なし** | - |
| `Infrastructure/FileSystem/XDos/Reader/XDosFamWriter.cs` | **変更なし** | - |
| `Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs` | **変更なし** | - |

---

## 将来拡張性（HDD / RAM ディスク）

`XDosMediaGeometry.FromDiskType()` に追加するだけで対応可能：

```csharp
DiskType.HardDisk => new(????, 512, 16, 256),
DiskType.RamDisk  => new(????, 512, 16, 256),
```

---

## 検証方法

1. **Bug 1 修正確認：**
   ```bash
   dotnet test --filter "WriteFile_NewDisk2DD_CrossCopy"
   ```
   生成された D88 を実機エミュレータ（X1 turbo 設定）でブートし「System not found!」が消えることを確認。

2. **FAT 状態の Assert（Diagnostic テストに追加）：**
   - クロスコピー後、FAM チェーンがクラスタ 0 を含まないこと
   - 最初のカーネルファイルの FirstCluster ≥ 3 であること

3. **2HD ジオメトリ確認：**
   - 2HD ディスクの ReadFile / WriteFile が 16セクタを使うことを単体テストで確認

4. **回帰テスト：**
   ```bash
   dotnet test
   dotnet run --project Test
   ```
