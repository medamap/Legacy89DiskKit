# 2D形式（Plain Disk Image）仕様書

## 概要
2D形式は、ヘッダーを持たない**生のセクタデータ**を連続して配置したシンプルなディスクイメージ形式です。D88形式からヘッダーとトラック情報を除いた純粋なデータ部分のみで構成されます。

## フォーマット構造

### 基本仕様
- **ファイル拡張子**: `.2D`
- **ヘッダー**: なし
- **データ配置**: セクタ順に連続配置

### データ配列順序
```
[Track 0, Side 0, Sector 1]
[Track 0, Side 0, Sector 2]
...
[Track 0, Side 0, Sector 16]
[Track 0, Side 1, Sector 1]
[Track 0, Side 1, Sector 2]
...
[Track 0, Side 1, Sector 16]
[Track 1, Side 0, Sector 1]
...
[Track 39, Side 1, Sector 16]
```

### ディスクパラメータ（2D標準）
| パラメータ | 値 |
|-----------|-----|
| トラック数 | 40 |
| 面数 | 2 |
| セクタ/トラック | 16 |
| バイト/セクタ | 256 |
| 総容量 | 327,680バイト (320KB) |

## セクタアドレス計算

```c
// 物理アドレスからオフセットを計算
uint32_t Calculate2DOffset(int track, int side, int sector)
{
    const int SECTORS_PER_TRACK = 16;
    const int BYTES_PER_SECTOR = 256;
    const int SIDES = 2;
    
    // セクタ番号は1から始まる
    int linearSector = (track * SIDES * SECTORS_PER_TRACK) +
                       (side * SECTORS_PER_TRACK) +
                       (sector - 1);
    
    return linearSector * BYTES_PER_SECTOR;
}
```

## Legacy89DiskKit実装設計

### 1. 新しいコンテナクラス

```csharp
namespace Legacy89DiskKit.DiskImage.Infrastructure.Container
{
    public class TwoDDiskContainer : IDiskContainer
    {
        private readonly byte[] _diskData;
        private readonly bool _readOnly;
        
        // 2D固定パラメータ
        private const int TRACKS = 40;
        private const int SIDES = 2;
        private const int SECTORS_PER_TRACK = 16;
        private const int BYTES_PER_SECTOR = 256;
        private const int TOTAL_SIZE = 327680; // 320KB
        
        public TwoDDiskContainer(string filePath, bool readOnly = true)
        {
            _readOnly = readOnly;
            _diskData = File.ReadAllBytes(filePath);
            
            if (_diskData.Length != TOTAL_SIZE)
            {
                throw new DiskImageException(
                    $"Invalid 2D file size. Expected {TOTAL_SIZE} bytes, " +
                    $"but got {_diskData.Length} bytes.");
            }
        }
        
        public byte[] ReadSector(int track, int sector, int side)
        {
            ValidateAddress(track, sector, side);
            
            int offset = CalculateOffset(track, side, sector);
            byte[] sectorData = new byte[BYTES_PER_SECTOR];
            Array.Copy(_diskData, offset, sectorData, 0, BYTES_PER_SECTOR);
            
            return sectorData;
        }
        
        public void WriteSector(int track, int sector, int side, byte[] data)
        {
            if (_readOnly)
                throw new DiskImageException("Disk image is read-only");
                
            ValidateAddress(track, sector, side);
            
            if (data.Length != BYTES_PER_SECTOR)
                throw new ArgumentException($"Sector size must be {BYTES_PER_SECTOR} bytes");
            
            int offset = CalculateOffset(track, side, sector);
            Array.Copy(data, 0, _diskData, offset, BYTES_PER_SECTOR);
        }
        
        private int CalculateOffset(int track, int side, int sector)
        {
            // セクタ番号は1から始まる
            int linearSector = (track * SIDES * SECTORS_PER_TRACK) +
                               (side * SECTORS_PER_TRACK) +
                               (sector - 1);
            
            return linearSector * BYTES_PER_SECTOR;
        }
        
        private void ValidateAddress(int track, int sector, int side)
        {
            if (track < 0 || track >= TRACKS)
                throw new ArgumentOutOfRangeException(nameof(track), 
                    $"Track must be between 0 and {TRACKS - 1}");
                    
            if (side < 0 || side >= SIDES)
                throw new ArgumentOutOfRangeException(nameof(side),
                    $"Side must be 0 or 1");
                    
            if (sector < 1 || sector > SECTORS_PER_TRACK)
                throw new ArgumentOutOfRangeException(nameof(sector),
                    $"Sector must be between 1 and {SECTORS_PER_TRACK}");
        }
        
        // IDiskContainer実装
        public DiskType DiskType => DiskType.TwoD;
        public int NumberOfTracks => TRACKS;
        public int SectorsPerTrack => SECTORS_PER_TRACK;
        public int NumberOfSides => SIDES;
        public int BytesPerSector => BYTES_PER_SECTOR;
        public string MediaName => "2D Plain Image";
    }
}
```

### 2. ファクトリー拡張

```csharp
public class DiskContainerFactory : IDiskContainerFactory
{
    public IDiskContainer OpenDiskImage(string filePath, bool readOnly = true)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fileInfo = new FileInfo(filePath);
        
        switch (extension)
        {
            case ".d88":
                return new D88DiskContainer(filePath, readOnly);
                
            case ".dsk":
                return new DskDiskContainer(filePath, readOnly);
                
            case ".2d":
                // ファイルサイズで2D形式を確認
                if (fileInfo.Length == 327680) // 320KB
                {
                    return new TwoDDiskContainer(filePath, readOnly);
                }
                else
                {
                    throw new DiskImageException(
                        $"Invalid 2D file size: {fileInfo.Length} bytes. " +
                        "2D format must be exactly 327,680 bytes (320KB).");
                }
                
            default:
                throw new NotSupportedException($"Unsupported disk format: {extension}");
        }
    }
    
    public IDiskContainer CreateNewDiskImage(string filePath, DiskType diskType, 
                                             string volumeName)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        switch (extension)
        {
            case ".2d":
                if (diskType != DiskType.TwoD)
                    throw new ArgumentException(
                        "2D format only supports 2D disk type (320KB)");
                        
                // 空の2Dディスクを作成
                return CreateNew2DDisk(filePath, volumeName);
                
            // 既存の処理...
        }
    }
    
    private IDiskContainer CreateNew2DDisk(string filePath, string volumeName)
    {
        // 327,680バイトの空ディスクを作成
        byte[] emptyDisk = new byte[327680];
        
        // 全セクタを0xE5で初期化（フォーマット済み状態）
        for (int i = 0; i < emptyDisk.Length; i++)
        {
            emptyDisk[i] = 0xE5;
        }
        
        File.WriteAllBytes(filePath, emptyDisk);
        
        return new TwoDDiskContainer(filePath, false);
    }
}
```

### 3. CLI対応

```bash
# 2Dファイルのリスト表示
./CLI list ~/Documents/emulator/x1turboROM/DISK/FT1_265.2D

# 2Dファイルの情報表示
./CLI info ~/Documents/emulator/x1turboROM/DISK/FT1_265.2D

# 2DからD88への変換
./CLI convert FT1_265.2D FT1_265.d88 --format d88

# D88から2Dへの変換（2Dトラックのみ）
./CLI convert disk.d88 disk.2d --format 2d

# 新規2Dディスク作成
./CLI create new.2d 2D "MY DISK" --format 2d
```

## 実装上の注意点

### 1. ファイルサイズ検証
2D形式は**必ず327,680バイト**でなければなりません。これ以外のサイズは無効な2Dファイルとして扱います。

### 2. セクタ番号
- 物理セクタ番号は**1から始まる**
- 配列インデックスは0から始まるため、変換時は-1する

### 3. トラック/面の順序
- 必ず**トラック0の面0→面1、トラック1の面0→面1**の順
- インターリーブなし

### 4. エラー情報なし
D88と異なり、セクタごとのステータス情報は保持できません。すべてのセクタは正常として扱われます。

## テスト項目

1. **読み込みテスト**
   - 327,680バイトの2Dファイルを正常に読み込めること
   - 各セクタのデータが正しい位置から読み込まれること
   - Hu-BASICファイルシステムとして認識されること

2. **変換テスト**
   - 2D → D88変換が正常に動作すること
   - D88 → 2D変換が正常に動作すること（2Dトラックのみ）
   - 変換後のデータが一致すること

3. **エラーテスト**
   - 327,680バイト以外のファイルを拒否すること
   - 無効なトラック/セクタ/面へのアクセスを拒否すること

## まとめ

2D形式は、最もシンプルなディスクイメージ形式です。ヘッダーがないため、ディスクの物理パラメータは固定（40トラック、2面、16セクタ、256バイト/セクタ）となります。

この実装により、Legacy89DiskKitは以下のディスクイメージ形式に対応します：
- D88形式（可変パラメータ、エラー情報付き）
- DSK形式（PC標準）
- **2D形式（固定パラメータ、生データ）** ← NEW!