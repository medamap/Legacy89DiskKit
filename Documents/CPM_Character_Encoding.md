# CP/M 文字エンコーディング仕様書

## 概要
CP/Mシステムにおける文字エンコーディングの仕様と、日本の8ビット機でのCP/M実装における文字コード体系について記載します。

## 1. 標準CP/Mの文字エンコーディング

### 基本仕様
- **文字セット**: 7ビットASCII (0x00-0x7F)
- **印字可能文字**: 0x20-0x7E
- **制御文字**: 0x00-0x1F
- **削除マーク**: 0x7F (DEL)

### ファイル名での使用
- **使用可能文字**: A-Z, 0-9, 一部の特殊文字
- **大文字変換**: 小文字は自動的に大文字に変換
- **禁止文字**: < > . , ; : = ? * [ ] % | ( ) / \

## 2. 日本の8ビット機でのCP/M文字コード

### 時代背景
- **CP/M全盛期**: 1970年代後半〜1980年代前半
- **Shift-JIS制定**: 1982年（MS-DOS 2.0と同時期）
- **結論**: 初期CP/MではShift-JISは使用されず、各機種独自の文字コードを採用

### 機種別文字コード体系

#### PC-8801シリーズ
```
文字コード範囲:
0x00-0x1F: 制御文字
0x20-0x7E: ASCII互換
0x80-0x9F: （未使用または制御）
0xA0-0xDF: 半角カタカナ
0xE0-0xFF: （機種により異なる）

特徴:
- 独自の文字コード体系
- JIS第1水準漢字対応（後期モデル）
- グラフィック文字セット
```

#### Sharp X1シリーズ
```
文字コード範囲:
0x00-0x1F: 制御文字
0x20-0x7E: ASCII互換
0x80-0xA0: （未使用）
0xA1-0xDF: 半角カタカナ（JIS X 0201準拠）
0xE0-0xEF: 罫線・ブロック文字
0xF0-0xFF: 特殊文字（数学記号、漢字「年月日時分」等）

特徴:
- X1Converterクラスでの実装例あり
- グラフィック文字の豊富なサポート
```

#### MSXシリーズ / MSX-DOS
```
初期（MSX1）:
0x00-0x1F: 制御文字
0x20-0x7E: ASCII互換
0x80-0xFF: グラフィック文字

MSX2以降 / MSX-DOS:
- Shift-JIS対応
- MS-DOS互換性重視
- 漢字ROM搭載
```

## 3. Legacy89DiskKitでの実装方針

### エンコーダークラス設計

#### 1. CpmCharacterEncoder（汎用）
```csharp
public class CpmCharacterEncoder : ICharacterEncoder
{
    public MachineType SupportedMachine => MachineType.CpmGeneric;
    
    public byte[] EncodeText(string text)
    {
        // 大文字変換（CP/Mファイル名規則）
        var upperText = text.ToUpper();
        
        // ASCII範囲外は空白に置換
        var result = new byte[upperText.Length];
        for (int i = 0; i < upperText.Length; i++)
        {
            var ch = upperText[i];
            result[i] = (ch >= 0x20 && ch <= 0x7E) ? (byte)ch : (byte)0x20;
        }
        return result;
    }
    
    public string DecodeText(byte[] data)
    {
        // 単純なASCIIデコード
        var chars = new char[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var b = data[i];
            chars[i] = (b >= 0x20 && b <= 0x7E) ? (char)b : ' ';
        }
        return new string(chars);
    }
}
```

#### 2. CpmPc8801CharacterEncoder
```csharp
public class CpmPc8801CharacterEncoder : Pc8801CharacterEncoder
{
    public override MachineType SupportedMachine => MachineType.CpmPc8801;
    
    // 既存のPc8801CharacterEncoderを継承して使用
    // 必要に応じてCP/M固有の調整を追加
}
```

#### 3. CpmX1CharacterEncoder
```csharp
public class CpmX1CharacterEncoder : X1CharacterEncoder
{
    public override MachineType SupportedMachine => MachineType.CpmX1;
    
    // 既存のX1CharacterEncoderを継承して使用
    // X1Converterの文字変換ロジックを活用
}
```

#### 4. CpmMsxDosCharacterEncoder
```csharp
public class CpmMsxDosCharacterEncoder : ICharacterEncoder
{
    public MachineType SupportedMachine => MachineType.CpmMsxDos;
    
    public byte[] EncodeText(string text)
    {
        // Shift-JISエンコーディング
        return Encoding.GetEncoding("shift_jis").GetBytes(text.ToUpper());
    }
    
    public string DecodeText(byte[] data)
    {
        // Shift-JISデコーディング
        try
        {
            return Encoding.GetEncoding("shift_jis").GetString(data);
        }
        catch
        {
            // フォールバック: ASCII
            return new CpmCharacterEncoder().DecodeText(data);
        }
    }
}
```

### CharacterEncoderFactoryの更新

```csharp
public class CharacterEncoderFactory : ICharacterEncoderFactory
{
    public ICharacterEncoder GetEncoder(MachineType machineType)
    {
        return machineType switch
        {
            // 既存のエンコーダー
            MachineType.X1 => new X1CharacterEncoder(),
            MachineType.Pc8801 => new Pc8801CharacterEncoder(),
            MachineType.Msx1 => new Msx1CharacterEncoder(),
            
            // CP/M用エンコーダー
            MachineType.CpmGeneric => new CpmCharacterEncoder(),
            MachineType.CpmPc8801 => new CpmPc8801CharacterEncoder(),
            MachineType.CpmX1 => new CpmX1CharacterEncoder(),
            MachineType.CpmMsxDos => new CpmMsxDosCharacterEncoder(),
            
            _ => throw new CharacterEncodingException($"Unsupported machine type: {machineType}")
        };
    }
}
```

## 4. 文字エンコーディング選択指針

### ファイル形式による自動判定
1. **CP/Mディスク判定時**
   - デフォルト: `CpmGeneric`（ASCII）
   - ユーザー指定可能

2. **機種ヒントによる判定**
   - ディスクラベルやファイル名パターンから推測
   - 例: "PC8801"を含む → `CpmPc8801`

3. **ユーザー明示指定**
   - CLIオプションで指定: `--encoding cpm-x1`

### エンコーディング優先順位
```
1. ユーザー明示指定
2. ディスク内容からの自動判定
3. デフォルト（CpmGeneric/ASCII）
```

## 5. 注意事項

### ファイル名エンコーディング
- CP/Mのファイル名は常にASCII（大文字）
- ファイル内容のみ機種別エンコーディングを適用

### 互換性の考慮
- 異なる機種間でのファイル交換時の文字化け
- エンコーディング情報の保存方法（メタデータ）

### エラーハンドリング
- 不正な文字コードの扱い
- フォールバック戦略（ASCII変換）

## 6. テストケース

### 基本テスト
1. ASCII範囲の文字列エンコード/デコード
2. 各機種固有文字のエンコード/デコード
3. 混在文字列の処理

### 境界値テスト
1. 制御文字の扱い
2. 0xFF付近の文字
3. 空文字列・null

### 互換性テスト
1. 実機で作成したファイルの読み取り
2. 各エンコーダー間の相互変換
3. ファイル名と内容の異なるエンコーディング