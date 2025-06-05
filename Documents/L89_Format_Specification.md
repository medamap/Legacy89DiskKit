# L89 - Legacy89 MFM/FM Recording Format

## 概要
L89（Legacy89 Recording Format）は、Legacy89DiskKitのMFM/FM記録に対応した**オリジナル**フォーマットです。技術的な最適解を追求し、最新の技術を活用した設計となっています。

## 設計理念

### なぜ新フォーマットか？
- **既存フォーマット**: それぞれに制約や依存関係がある
- **技術的制限**: 現代の技術を活かしきれていない
- **拡張性**: 将来の機能追加が困難

→ **自分たちで最適なフォーマットを作る！**

## L89 フォーマット仕様

### ファイル構造
```
L89 File Format v1.0
=======================

[Magic Header] - 16 bytes
  "L89\x00\x1A\x00"  (8 bytes) - 識別子
  Version           (2 bytes) - フォーマットバージョン
  Flags             (2 bytes) - グローバルフラグ
  Reserved          (4 bytes) - 将来の拡張用

[Metadata Section] - 可変長
  DiskGeometry      - ディスク物理仕様
  RecordingInfo     - 記録パラメータ
  ProtectionHints   - プロテクション情報
  ExtendedMetadata  - 拡張メタデータ（JSON形式）

[Track Index] - 可変長
  TrackCount        (2 bytes)
  TrackEntries[]    - 各トラックのオフセットと情報

[Track Data] - 可変長
  各トラックのビットストリームデータ
  圧縮オプション対応
```

### 特徴

#### 1. モダンな設計
```c
// ヘッダー構造体
typedef struct {
    char     magic[8];      // "L89\x00\x1A\x00"
    uint16_t version;       // 0x0100 = v1.0
    uint16_t flags;         // ビットフラグ
    uint32_t reserved;      // 予約領域
} L89_Header;

// フラグ定義
#define L89_FLAG_COMPRESSED    0x0001  // zstd圧縮
#define L89_FLAG_ENCRYPTED     0x0002  // 暗号化
#define L89_FLAG_VERIFIED      0x0004  // CRC検証済み
#define L89_FLAG_PROTECTION    0x0008  // プロテクション含む
```

#### 2. 柔軟なメタデータ
```json
{
  "disk_info": {
    "title": "Game Disk #1",
    "publisher": "Retro Games Inc.",
    "date": "1989-12-25",
    "system": "Sharp X1 Turbo Z",
    "notes": "Original disk with copy protection"
  },
  "recording_info": {
    "device": "KryoFlux",
    "date": "2025-01-06T15:30:00Z",
    "settings": {
      "sampling_rate": 24027428,
      "calibration": "auto"
    }
  },
  "protection_analysis": {
    "type": "weak_bits",
    "tracks": [15, 16, 17],
    "confidence": 0.95
  }
}
```

#### 3. トラックデータ構造
```c
typedef struct {
    uint32_t offset;        // ファイル内オフセット
    uint32_t length;        // データ長（圧縮後）
    uint32_t raw_length;    // 非圧縮時の長さ
    uint16_t encoding;      // MFM/FM/GCR等
    uint16_t flags;         // トラック固有フラグ
    uint32_t bitrate;       // ビットレート
    uint32_t checksum;      // CRC32
} L89_TrackEntry;

// エンコーディング種別
#define L89_ENC_MFM     0x0001
#define L89_ENC_FM      0x0002
#define L89_ENC_GCR     0x0003
#define L89_ENC_MIXED   0x00FF  // 混在
```

#### 4. 高度な機能

**ビットストリーム拡張**
```c
// 特殊マーカー
typedef struct {
    uint32_t position;      // ビット位置
    uint8_t  type;         // マーカー種別
    uint8_t  data[3];      // マーカーデータ
} L89_Marker;

// マーカー種別
#define MARKER_WEAK_BIT     0x01  // 弱ビット
#define MARKER_NO_FLUX      0x02  // フラックスなし
#define MARKER_SYNC_LOST    0x03  // 同期喪失
#define MARKER_INDEX_HOLE   0x04  // インデックスホール
```

**セクタマップ（オプション）**
```c
// 高速アクセス用セクタマップ
typedef struct {
    uint16_t track;
    uint16_t side;
    uint16_t sector;
    uint16_t size;
    uint32_t bit_offset;    // トラック内ビットオフセット
    uint16_t crc_status;    // CRC状態
    uint16_t flags;         // セクタフラグ
} L89_SectorMap;
```

### 実装メリット

#### 1. 技術的優位性
- **圧縮対応**: Zstandard採用で50-70%のサイズ削減
- **ストリーミング**: 巨大ファイルでも効率的処理
- **並列処理**: トラック単位での並列読み書き
- **エラー訂正**: Reed-Solomon符号（オプション）

#### 2. 実用的機能
- **メタデータ**: JSON形式で自由に拡張可能
- **検証機能**: チェックサム・署名対応
- **バージョニング**: 後方互換性を考慮した設計
- **ツール連携**: 標準的なフォーマットで解析容易

#### 3. コミュニティフレンドリー
- **オープン仕様**: 完全公開・自由利用
- **参照実装**: C/C++/C#/Rustで提供
- **変換ツール**: 他形式との相互変換
- **ドキュメント**: 詳細な仕様書・サンプル

## CLI統合

```bash
# L89形式で記録
./CLI record /dev/disk2 output.l89 --format l89

# 情報表示
./CLI info disk.l89 --detailed

# プロテクション解析
./CLI analyze disk.l89 --protection

# 他形式への変換
./CLI convert disk.l89 disk.d88 --mode compatible
./CLI convert disk.hfe disk.l89 --format l89
./CLI convert disk.fdx disk.l89 --format l89

# ストリーミング読み込み
./CLI stream disk.l89 --track 15 --side 0
```

## C# 実装例

```csharp
namespace Legacy89DiskKit.MfmRecording.Domain.Model
{
    public class L89Header
    {
        public string Magic { get; set; } = "L89\x00\x1A\x00";
        public ushort Version { get; set; } = 0x0100;
        public L89MfmFlags Flags { get; set; }
        
        public bool IsValid => Magic.StartsWith("L89");
    }
    
    [Flags]
    public enum L89Flags : ushort
    {
        None = 0,
        Compressed = 0x0001,
        Encrypted = 0x0002,
        Verified = 0x0004,
        HasProtection = 0x0008,
        HasSectorMap = 0x0010,
        HasMetadata = 0x0020
    }
    
    public class L89Container : IMfmContainer
    {
        private readonly L89Header _header;
        private readonly Dictionary<int, TrackData> _tracks;
        private readonly JObject _metadata;
        
        public void SaveTrack(int trackNum, BitStream data, TrackOptions options)
        {
            // Zstandard圧縮
            if (_header.Flags.HasFlag(L89Flags.Compressed))
            {
                data = CompressTrack(data);
            }
            
            // プロテクション情報を埋め込み
            if (options.HasProtection)
            {
                EmbedProtectionMarkers(data, options.ProtectionInfo);
            }
            
            _tracks[trackNum] = new TrackData
            {
                BitStream = data,
                Encoding = options.Encoding,
                Bitrate = options.Bitrate,
                Checksum = CalculateCRC32(data)
            };
        }
        
        public ProtectionAnalysis AnalyzeProtection()
        {
            var analyzer = new L89ProtectionAnalyzer();
            return analyzer.Analyze(_tracks.Values);
        }
    }
}
```

## 優位性まとめ

### 技術的優位性

| 特徴 | L89 | 他形式 |
|------|--------|-------|
| ライセンス | MIT | 様々 |
| 圧縮対応 | ✅ Zstd | 一般的に非対応 |
| メタデータ | ✅ JSON | 固定形式が多い |
| 拡張性 | ✅ 高 | 形式による |
| ストリーミング | ✅ | 一般的に非対応 |
| プロテクション | ✅ 完全 | 形式による |

### なぜL89か？

1. **技術的自由度**: 最適な実装を追求できる
2. **モダン設計**: 2025年の技術で最適化
3. **実用的**: 圧縮・メタデータ・ストリーミング対応
4. **将来性**: 拡張可能な設計で10年先も使える

## まとめ

技術的に最適なフォーマットを目指して、**自分たちで新しいフォーマットを作る**という選択をしました！

L89は：
- 最新技術を活用した設計
- オープンで拡張可能
- コミュニティのためのフォーマット

**Legacy89DiskKitが定義する新標準**として、レトロコンピューティングコミュニティに貢献します！