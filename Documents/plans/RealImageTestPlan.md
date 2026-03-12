# Real Image Test Plan

## 1. 目的

Hu-BASIC (X1), N88-BASIC (PC-8801), MSX-DOS (MSX) の各ファイルシステム実装について、実機由来ディスクイメージを使って以下を体系的に検証する。

- 検出 (`CanHandle`, `DiskType`, `Container`) が実イメージで正しく働くこと
- 読み込み結果が実イメージ上の情報と一致すること
- 文字コード処理が機種差をまたいで破綻しないこと
- 書き込み・削除が安全で、既存構造を壊さないこと
- 実機らしいジオメトリ差異に耐えること
- ブート領域 (IPL / Boot Area) の読み書きが正しいこと

## 2. 対象イメージ

| Platform | File System | Image | Expected media / container | Main purpose |
| --- | --- | --- | --- | --- |
| X1 | Hu-BASIC | `CZ8FB01.d88` | 2D / D88 | Hu-BASIC 1.0 基準検証 |
| X1 | Hu-BASIC | `CZ8FB02.2d` | 2D / raw-ish 2D image | Hu-BASIC 2.0 差分確認 |
| X1 | Hu-BASIC | `X1turboIIIDemo.d88` | 2HD / D88 | 1024-byte sector, 2HD 端点 |
| PC-8801 | N88-BASIC | `PC-88SR.D88` | 2D / D88 | 2D 標準ケース |
| PC-8801 | N88-BASIC | `[OS] PC-8801MA system disk.d88` | 2DD or 2HD / D88 | 高密度系・システムディスク |
| MSX | MSX-DOS | `ldsys.dsk` | 720KB raw | FAT12 標準ケース |
| MSX | MSX-DOS | `ROM.DSK` | 720KB raw | FAT12 別実サンプル |

## 3. テスト方針

### 3.1 元本保護

- すべての書き込み系テストは元イメージに対して直接実行しない。
- 各テストケース開始前に作業コピーを作成し、コピーに対してのみ書き込み・削除・属性変更・ブート領域更新を行う。
- 元ファイルの `SHA-256` を記録し、テスト後に不変であることを確認する。
- 書き込み系は必ず「事前スナップショット」「操作」「再オープン」「差分確認」の順に行う。

### 3.2 実施レベル

- レベルA: API 単体結果確認
- レベルB: イメージ再オープン後の永続性確認
- レベルC: セクタ/FAT/ディレクトリエントリの生データ照合
- レベルD: 別機種イメージとの比較で共通基盤の健全性を確認

### 3.3 証跡

各ケースで最低限以下を残す。

- 対象イメージ名
- 期待 `DiskType` と実測 `DiskType`
- `Container` 種別
- ファイル一覧の件数
- 代表ファイル 3 件以上の `Filename / Attributes / Size / First bytes`
- Boot Area サイズと先頭 32 byte のダンプ
- 書き込み系では変更前後の FAT とディレクトリ差分

## 4. 事前準備

### 4.1 基準値の採取

各イメージについて、最初に読み取り専用モードで以下を採取し、以後の期待値表として固定する。

- ディスク全体サイズ
- コンテナ種別 (`D88` または raw)
- 実装が返す `DiskType`
- セクタサイズ
- トラック数、ヘッド数、セクタ/トラック
- ファイル一覧
- ファイル属性分布
- Boot Area のサイズ・シグネチャ・既知文字列

### 4.2 期待値表の作り方

- ファイル名、サイズ、属性は実装の一覧結果に加えて、生セクタ上のディレクトリエントリでも確認する。
- 代表ファイルの本文先頭 64 byte と末尾 64 byte を記録する。
- テキスト系は可能なら ASCII 表示と hex 表示の両方を保存する。
- 文字セット検証用に、カタカナ・記号・英数字を含む名前や本文を持つ既存ファイルを優先して選ぶ。

## 5. テストマトリクス

| Area | X1 Hu-BASIC 2D | X1 Hu-BASIC 2HD | PC-8801 2D | PC-8801 2DD/2HD | MSX-DOS 720KB |
| --- | --- | --- | --- | --- | --- |
| CanHandle / DiskType / Container | Must | Must | Must | Must | Must |
| ファイル一覧・属性・内容 | Must | Must | Must | Must | Must |
| 文字セット | Must | Should | Must | Should | Must |
| 書き込み・削除 | Must | Should | Must | Should | Must |
| ジオメトリ端点 | 256B sector | 1024B sector | 256B sector | 256/1024 系 | 512B sector |
| ブート領域 | Must | Must | Must | Must | Must |

## 6. 検出テスト

## 6.1 目的

- `CanHandle` が対象外イメージを誤認しないこと
- `DiskType` が実イメージの密度に一致すること
- `Container` が `D88` と raw を正しく区別すること

## 6.2 ケース

| ID | Image | Expected result |
| --- | --- | --- |
| DET-X1-01 | `CZ8FB01.d88` | Hu-BASIC provider が `CanHandle=true`, `DiskType=TwoD`, `Container=D88` |
| DET-X1-02 | `CZ8FB02.2d` | Hu-BASIC provider が `CanHandle=true`, raw 系 container, `DiskType=TwoD` |
| DET-X1-03 | `X1turboIIIDemo.d88` | Hu-BASIC provider が `CanHandle=true`, `DiskType=TwoHD`, `Container=D88` |
| DET-N88-01 | `PC-88SR.D88` | N88-BASIC provider が `CanHandle=true`, `DiskType=TwoD` |
| DET-N88-02 | `[OS] PC-8801MA system disk.d88` | N88-BASIC provider が `CanHandle=true`, `DiskType=TwoDD` または `TwoHD` を実イメージに一致させる |
| DET-MSX-01 | `ldsys.dsk` | MSX-DOS provider が `CanHandle=true`, raw container, FAT12 geometry が妥当 |
| DET-MSX-02 | `ROM.DSK` | MSX-DOS provider が `CanHandle=true`, raw container, FAT12 geometry が妥当 |
| DET-NEG-01 | 各イメージを他機種 provider に渡す | 他 provider は `CanHandle=false` |

## 6.3 手順

1. 各イメージを開き、container 判定結果を記録する。
2. 全 provider に対して `CanHandle` を実行し、唯一の正答 provider のみが `true` になることを確認する。
3. `DiskType` を取得し、D88 ヘッダまたは raw のサイズ・既知 geometry から導いた期待値と照合する。
4. D88 イメージでは media flag と実装の `DiskType` が一致することを確認する。
5. raw イメージでは BPB または総容量から求めた geometry と一致することを確認する。

## 6.4 観点

- D88 ヘッダの media flag 依存が強すぎて、中身と不一致なケースを誤検出しないか
- raw 判定が容量ベースに偏り、別 FS を誤認しないか
- X1 2HD と PC-88 高密度系で 1024-byte sector の扱いを混同しないか

## 7. 読み込みの完全性

## 7.1 目的

- `Filename`, `Attributes`, `Size`, `Data` が実イメージ上の実データと一致すること
- ファイルタイプごとの終端規則や FAT チェーン解釈が正しいこと

## 7.2 代表サンプル選定

各イメージから最低 3 件、可能なら以下を含むように選ぶ。

- テキスト系ファイル
- バイナリ系ファイル
- 属性付きファイル
- 0 cluster ではない通常ファイル
- サイズがクラスタ境界をまたぐファイル

## 7.3 共通チェック項目

- 一覧件数がディレクトリ生解析の件数と一致する
- ファイル名のトリム処理が正しい
- 拡張子の有無や空白パディングが崩れない
- 属性ビットが raw 値と標準属性の両方で一致する
- サイズがディレクトリエントリ値と一致する
- FAT チェーン追跡結果の総データ量がサイズまたは終端規則に一致する
- 読み出した先頭 16 byte / 末尾 16 byte が生セクタから再構成した値と一致する

## 7.4 機種別観点

### Hu-BASIC

- ASCII ファイルはサイズ値だけでなく終端シーケンスも確認する。
- BASIC / Binary / ASCII のモード解釈優先順位が正しいことを確認する。
- 2HD では FAT の分割表現と directory / FAT 位置差異を確認する。

### N88-BASIC

- 16 byte ディレクトリエントリのファイル名 6 文字 + 拡張子 3 文字を正しく復元する。
- 属性 byte の bit0, bit4, bit5, bit6, bit7 の解釈が正しいことを確認する。
- 2D と 2DD 系で cluster size の違いが内容読込に影響しないことを確認する。

### MSX-DOS

- 8.3 名称、32 byte directory entry、FAT12 チェーン解釈が正しいことを確認する。
- `BPB` の値と実際の領域計算が一致することを確認する。
- ルートディレクトリ開始位置、データ領域開始位置、クラスタ 2 の対応が正しいことを確認する。

## 7.5 推奨ケース

| ID | Check |
| --- | --- |
| READ-01 | 全ファイル一覧件数とディレクトリエントリ件数が一致する |
| READ-02 | 代表ファイルの `Filename / Attributes / Size` が生解析と一致する |
| READ-03 | 代表テキストファイル本文を読み、生データ再構成結果と一致する |
| READ-04 | クラスタ境界超えファイルで chain の各クラスタが期待順で読める |
| READ-05 | 最終クラスタ終端解釈が FS ごとの仕様どおりである |
| READ-06 | 0 byte または極小ファイルが存在する場合、空読込で破綻しない |

## 8. 異機種間文字セット検証

## 8.1 目的

- 文字コード登録の枠組みが、機種ごとのファイル名・本文デコードに適切に適用されること
- カタカナ、記号、機種依存文字の扱いで化け・欠落・誤正規化が起きないこと

## 8.2 対象文字

- 半角カタカナ
- `-`, `_`, `.`, `/`, `\`, `|` に相当する記号
- 長音、句読点、括弧、チルダ、波ダッシュ系
- 英数字混在名

## 8.3 検証方法

1. 各イメージの既存ファイル名から、カタカナまたは記号を含む候補を抽出する。
2. 候補が不足する場合は、作業コピー上に文字セット確認用ファイルを作成する。
3. 同一バイト列に対して「raw bytes」「登録 encoder での decode」「再 encode 後の bytes」を比較する。
4. 名前だけでなく本文にも同様の round-trip 検証を行う。
5. 表示文字列比較だけでなく、必ず byte 単位で再一致を確認する。

## 8.4 判定基準

- decode 後の表示が期待文字列と一致する
- encode(decode(bytes)) が元の bytes と一致する
- 異機種 encoder を意図的に適用した場合のみ差異が発生する
- ファイル名比較ロジックが文字化け文字列で別名扱いしない

## 8.5 追加ケース

| ID | Check |
| --- | --- |
| CHAR-01 | X1 ファイル名の JIS X 0201 カタカナ round-trip |
| CHAR-02 | N88 ファイル名の ASCII/Shift-JIS 系文字の round-trip |
| CHAR-03 | MSX 8.3 名称で許容文字と禁止文字の境界確認 |
| CHAR-04 | 本文中のカタカナ・記号を decode/encode して byte 一致 |
| CHAR-05 | `|` やバックスラッシュ相当文字が UI 表示と内部 bytes で混線しない |

## 9. 書き込み・削除の安全性検証

## 9.1 原則

- 必ずコピーイメージに対して実施する。
- 1 ケースごとに新しいコピーを使う。
- 操作後は再オープンし、一覧・FAT・ディレクトリ・Boot Area を再確認する。

## 9.2 書き込みテスト

### 基本ケース

- 新規小容量ファイル作成
- クラスタ境界をまたぐファイル作成
- テキスト系とバイナリ系の両方作成
- 属性付きファイル作成

### 確認項目

- 作成後に一覧へ 1 件だけ追加される
- ディレクトリエントリが 1 件だけ新規使用される
- FAT チェーンが新規割当分のみ変化する
- 未使用領域以外の既存ファイルデータが不変
- 再オープン後もサイズ、属性、内容が一致する

## 9.3 上書きテスト

- 同名ファイル上書き時の挙動を明確化し、仕様どおりか確認する
- 旧クラスタ解放と新クラスタ割当が正しく行われることを確認する
- ディレクトリエントリ重複が発生しないことを確認する

## 9.4 削除テスト

- 代表ファイル 1 件を削除する
- 一覧から消えることを確認する
- ディレクトリエントリが削除済みマーカーになることを確認する
- FAT チェーンがすべて free 状態へ戻ることを確認する
- 他ファイルの chain に影響がないことを確認する

## 9.5 安全性の不変条件

- 変更対象外ファイルの `Filename / Size / Attributes / Data hash` が不変
- FAT の free cluster 数増減が、書き込み・削除量と整合する
- ルートディレクトリの終端マーカーや未使用領域が壊れない
- ブート領域とシステム領域が意図せず変化しない
- D88 の場合、トラックオフセットテーブルやヘッダが不変

## 9.6 推奨ケース

| ID | Check |
| --- | --- |
| WRITE-01 | 小容量新規ファイル作成後の再読込一致 |
| WRITE-02 | クラスタ境界超えファイル作成後の FAT chain 一致 |
| WRITE-03 | 属性付きファイル作成と再読込一致 |
| DELETE-01 | 削除後に directory marker と FAT 解放が一致 |
| DELETE-02 | 削除対象以外のデータ hash が不変 |
| UPDATE-01 | 属性変更後に raw attribute byte が期待どおり更新される |

## 10. ジオメトリの端点ケース

## 10.1 目的

- 実機らしいセクタサイズ差、トラック数差、システム領域配置差に耐えること

## 10.2 重点観点

### セクタサイズ

- X1 / N88 の 256 byte sector
- X1 2HD または PC-88 高密度系の 1024 byte sector
- MSX raw の 512 byte sector

### トラック・密度

- 40 track 系
- 77 track 系または 80 track 系
- 2D / 2DD / 2HD 差異

### ファイルシステム依存の端点

- Hu-BASIC 2D と 2HD で FAT / directory の配置が変わる
- N88-BASIC は system track が 2D と 2DD 系で異なる
- MSX-DOS は BPB 値と実サイズの整合性が重要

## 10.3 ケース

| ID | Image | Check |
| --- | --- | --- |
| GEO-01 | `CZ8FB01.d88` | 256-byte sector 前提で FAT / directory / boot の位置が正しい |
| GEO-02 | `X1turboIIIDemo.d88` | 1024-byte sector 前提で誤って 256-byte 扱いしない |
| GEO-03 | `PC-88SR.D88` | 2D system track の位置解釈が正しい |
| GEO-04 | `[OS] PC-8801MA system disk.d88` | 高密度系で cluster size と sector size を混同しない |
| GEO-05 | `ldsys.dsk` | BPB から算出した geometry と実ファイルサイズが一致 |
| GEO-06 | `ROM.DSK` | media descriptor と BPB の矛盾がないか確認 |

## 10.4 失敗時の切り分け

- 一覧件数だけズレるなら directory 開始位置または entry size を疑う
- サイズだけズレるなら FAT 終端値または file type 別終端規則を疑う
- 内容が途中から崩れるなら cluster to sector 変換を疑う
- 高密度系だけ失敗するなら sector size と system track 算出を疑う

## 11. ブート領域 (IPL) 検証

## 11.1 目的

- `ReadBootArea` と `WriteBootArea` が各 FS の boot 領域仕様に従って動くこと
- ブート領域更新が他領域を汚染しないこと

## 11.2 読み込み検証

各イメージで以下を確認する。

- Boot Area 長が期待サイズと一致する
- 先頭バイト列が生セクタ読込結果と一致する
- 既知のシグネチャ、ラベル、ジャンプ命令、メディア情報が読める
- ブート可能イメージでは既知の boot 情報が失われていない

## 11.3 書き込み検証

作業コピー上で、最小限の非破壊パターンを使う。

- 元の boot 領域をバックアップする
- ラベルや未使用領域など、安全に変更可能なバイトのみを書き換える
- `WriteBootArea` 実行後に `ReadBootArea` で byte 一致を確認する
- イメージ再オープン後も同じ値が読めることを確認する
- 最後に元データを書き戻して、完全復元できることを確認する

## 11.4 機種別観点

### Hu-BASIC

- boot flag, label, extension, load/exec address, start sector 相当情報を確認する
- boot 領域が通常ファイル管理領域と独立していることを確認する

### N88-BASIC

- track 0 / sector 1 の IPL 相当データ取得が正しいことを確認する
- FM/MFM 由来の論理差を D88 abstraction が吸収できていることを確認する

### MSX-DOS

- 512 byte boot sector, BPB, 0x55AA 署名を確認する
- BPB 変更時に FAT/root/data 計算が崩れない範囲に限定してテストする

## 11.5 ケース

| ID | Check |
| --- | --- |
| IPL-01 | Boot Area 読込値と生セクタ値が一致 |
| IPL-02 | 既知シグネチャまたは BPB が妥当 |
| IPL-03 | コピー上で安全な 1 箇所更新後、再読込 byte 一致 |
| IPL-04 | 元データ書戻し後、boot 領域 hash が初期値へ戻る |
| IPL-05 | boot 更新後もファイル一覧と代表ファイル hash が不変 |

## 12. 実行順序

1. 元イメージのハッシュ採取
2. 検出テスト
3. 読み込み完全性テスト
4. 文字セットテスト
5. ジオメトリ端点テスト
6. Boot Area 読み込みテスト
7. 書き込み・属性変更・削除テスト
8. Boot Area 書き込みテスト
9. 差分確認と証跡整理

## 13. 完了条件

- 全対象イメージで正答 provider のみ `CanHandle=true`
- `DiskType` と `Container` が期待と一致
- 各イメージで代表ファイルの `Filename / Attributes / Size / Data` が一致
- 文字セット round-trip が byte 単位で成立
- 書き込み・削除・属性変更後に不変条件を満たす
- 端点 geometry で誤読込・誤書込が発生しない
- Boot Area 読み書きが他領域に影響しない

## 14. 既知リスクと補助確認

- 実イメージ側に元から破損や非標準レイアウトがある可能性があるため、失敗時は実装不具合と即断しない
- ASCII テキスト終端や BASIC トークン形式は、機種ごとの終端規則を必ず分離して確認する
- 文字化け判定は表示だけでなく byte round-trip を基準にする
- D88 はヘッダ上の密度と実セクタ配置の両方を見る
- 書き込み系の最終判断は「再オープン後の生データ一致」を優先する
