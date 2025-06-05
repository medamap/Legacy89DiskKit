# MFM/FM記録方式の基礎

## 概要
その認識、完全に正しいです！MFM/FM記録は、**時間軸上のビットストリーム**として磁気ディスクにデータを記録する方式です。

## 基本原理

### 1. 物理的な記録の仕組み

```
ディスク回転（300rpm = 5回転/秒）
     ↓
磁気ヘッドが磁束変化を検出
     ↓
パルス列（ビットストリーム）として読み取り
     ↓
MFM/FMデコーダーがデータに変換
```

### 2. FM（Frequency Modulation）方式

**特徴**: シンプルだが記録密度が低い

```
データビット: 1  0  1  1  0  0  1  0
クロック:     C  C  C  C  C  C  C  C
記録パターン: 11 10 11 11 10 10 11 10

C = クロックビット（常に1）
データ1 = 11
データ0 = 10
```

### 3. MFM（Modified FM）方式

**特徴**: FMの2倍の記録密度

```
データビット:   1   0   1   1   0   0   1   0
記録パターン:  01  00  01  01  10  00  01  10

ルール:
- データ1 → 01
- データ0（前が1）→ 00  
- データ0（前が0）→ 10
```

## タイミングの重要性

### ビットセルとタイミング

```
300rpm、16セクタ/トラックの場合：

1回転 = 200ms
1セクタ = 12.5ms
1バイト = 48.8μs（MFM）
1ビット = 6.1μs（MFM）

わずかなズレでデータ化け！
```

### タイミングウィンドウ

```
理想的なタイミング:     |----●----|
許容範囲（±25%）:       |--[===]--|
                           ↑
                      ここでサンプリング

ズレすぎると隣のビットと誤認識
```

## FDCエミュレーションの要件

### 1. リアルタイム性

```
CPU/FDC: "Track 10, Sector 3を読みたい"
     ↓
エミュレータ: 
1. 現在のヘッド位置を計算
2. 目的セクタまでの時間を計算  
3. 適切なタイミングでデータ供給
     ↓
CPU/FDC: "データ受信OK"
```

### 2. 正確なタイミングエミュレーション

```c
// 疑似コード
void FDC_ReadSector(int track, int sector) {
    // 現在の回転位置を計算
    double rotation_time = GetCurrentRotationTime();
    double sector_angle = (sector * 360.0) / sectors_per_track;
    
    // 目的セクタまでの待機時間
    double wait_time = CalculateWaitTime(rotation_time, sector_angle);
    
    // インデックスホール信号
    if (IsIndexHole(rotation_time)) {
        SetIndexSignal(true);
    }
    
    // セクタヘッダー（ID）を送信
    if (wait_time <= 0) {
        SendSectorID(track, sector);
        // ギャップ
        WaitMicroseconds(gap2_time);
        // データ送信
        SendSectorData(sector_data);
    }
}
```

### 3. ステータス管理

```
FDCステータスレジスタ:
Bit 7: モーターON
Bit 6: Write Protect
Bit 5: Record Type (FM/MFM)
Bit 4: Seek Error
Bit 3: CRC Error
Bit 2: Track 00
Bit 1: Index
Bit 0: Busy
```

## L89形式でのMFM/FM記録

### ビットストリーム保存

```c
// L89形式でのトラックデータ
typedef struct {
    uint32_t bit_length;      // ビット数
    uint8_t* bit_stream;      // ビットデータ
    uint32_t* flux_timing;    // オプション：フラックスタイミング
    uint16_t* weak_bits;      // オプション：弱ビット位置
} L89_TrackData;
```

### セクタ解析情報

```c
// 高速アクセス用
typedef struct {
    uint32_t id_position;     // IDフィールドのビット位置
    uint32_t data_position;   // データフィールドのビット位置
    uint16_t id_crc;         // ID CRC
    uint16_t data_crc;       // データCRC
    uint8_t  status;         // エラーステータス
} L89_SectorInfo;
```

## エミュレーション実装のポイント

### 1. タイミング精度

```c
// マイクロ秒単位の精密タイマー
class PrecisionTimer {
public:
    void WaitUntil(uint64_t target_time) {
        while (GetMicroseconds() < target_time) {
            // スピンウェイトまたはスリープ
            if (target_time - GetMicroseconds() > 1000) {
                Sleep(1);
            }
        }
    }
};
```

### 2. データストリーミング

```c
class MFMStreamer {
private:
    L89_TrackData* current_track;
    uint32_t bit_position;
    
public:
    uint8_t GetNextByte() {
        uint8_t byte = 0;
        for (int i = 0; i < 8; i++) {
            byte <<= 1;
            byte |= GetNextBit();
        }
        return byte;
    }
    
    bool GetNextBit() {
        // ビットストリームから1ビット取得
        uint32_t byte_pos = bit_position / 8;
        uint32_t bit_mask = 1 << (7 - (bit_position % 8));
        
        bool bit = (current_track->bit_stream[byte_pos] & bit_mask) != 0;
        
        bit_position++;
        if (bit_position >= current_track->bit_length) {
            bit_position = 0; // トラック先頭に戻る
        }
        
        return bit;
    }
};
```

### 3. プロテクション対応

```c
// 弱ビットエミュレーション
bool GetWeakBit(uint32_t position) {
    // 弱ビット位置では乱数を返す
    for (int i = 0; i < weak_bit_count; i++) {
        if (weak_bits[i] == position) {
            return (rand() & 1);  // 50%の確率で0/1
        }
    }
    return GetNormalBit(position);
}
```

## まとめ

MFM/FM記録の本質：
1. **時間軸上のビットストリーム** - タイミングが命
2. **リアルタイム性** - FDCの要求に即座に応答
3. **正確なエミュレーション** - ステータス、タイミング、エラーも含めて

L89形式はこれらを完全にサポートし、さらに：
- 圧縮による効率化
- メタデータによる付加情報
- プロテクション情報の保存

を実現します！