# X-DOS ライセンスおよび入手先情報

調査日: 2026-03-19
調査方法: Wayback Machine アーカイブの直接解析、Availability API による確認

> **免責**: このドキュメントに記載されている情報は、現時点で Wayback Machine から
> 取得できるアーカイブ記録のみに基づく推測・確認事項です。
> 一次情報源へのアクセスが失われているため、不完全な情報が含まれる可能性があります。

---

## 開発元・著作権者

| 項目 | 内容 | 根拠 |
|---|---|---|
| OS 名称 | X-DOS（Sharp X1/turbo シリーズ専用 DOS） | サイトタイトル |
| 開発者 | **Regulus**（ハンドル名） | マニュアル冒頭「System created by Regulus」 |
| 連絡先（当時） | regulus@pc.highway.ne.jp | サイト掲載メールアドレス |
| サイト URL（当時） | http://home4.highway.ne.jp/regulus/xdos/ | Wayback Machine アーカイブより |
| 初版リリース推定 | 1984年4月17日 | ディスクイメージ内 Volume Record の BCD 日付より |
| マニュアル日付 | 1990年3月11日（V2.4c 以降版） | ユーザーズマニュアル冒頭より |
| 動作対象 | X1 シリーズ、X1 turbo シリーズ（Z 含む）、MZ-2500（暫定版） | サイト仕様ページより |

> **注**: 一部資料に「C&S Soft」という名称が記載されている場合があるが、
> 今回調査した Wayback Machine アーカイブからはこの名称を直接確認できなかった。
> 出典不明のため、本ドキュメントでは開発者名として「Regulus」のみを記載する。

---

## ライセンス状況

### 公式な宣言

- **フリーウェア宣言: なし**
- **パブリックドメイン宣言: なし**
- **再配布許可・禁止の明示: なし**

作者 Regulus は自身の公式 Web サイト（`http://home4.highway.ne.jp/regulus/xdos/`）から
D88 形式のディスクイメージを LZH 圧縮で無償ダウンロード提供していた（2001年時点確認）。
ただし、ライセンス条件・著作権表示・利用規約の記載は一切存在しなかった。

### 日本著作権法上の扱い

法人または個人が公表した著作物は公表後 70 年間保護される（著作権法第 51〜53 条）。
X-DOS が 1984 年頃に公表されたとすれば、**著作権保護期間は 2054 年頃まで継続**する。
「アバンドンウェア」という概念は日本の著作権法に存在せず、
著作権者が連絡不能であっても著作権は消滅しない。

### Legacy89DiskKit としての立場

**ファイルシステム仕様の実装（本ライブラリのコード）は著作権の保護対象外。**
ファイルシステムのフォーマット構造・アルゴリズムに著作権は生じない（MAME/DOSBox が
ROM/OS 本体を同梱しないのと同じ構造）。

X-DOS のディスクイメージ本体（バイナリ）の配布は行わない。
本ライブラリの X-DOS 対応機能は、**X-DOS を合法的に所有するユーザーが
自身のディスクイメージを操作するために使用するもの**と位置づける。

---

## 公式サイト アーカイブ情報

元サイトは現在消滅しているが、Wayback Machine に一部スナップショットが残っている。

| URL | 内容 | アーカイブ日時 |
|---|---|---|
| `https://web.archive.org/web/20010708163009/http://home4.highway.ne.jp/regulus/xdos/index.html` | トップページ（仕様説明、ダウンロードリンク） | 2001年7月8日 |
| `http://web.archive.org/web/20030416004159/http://home4.highway.ne.jp:80/regulus/xdos/` | トップページ（別スナップショット） | 2003年4月16日 |

### ダウンロードファイルのアーカイブ状況

元サイトで配布されていたファイルは、`/regulus/file/` パス下に保存されており、
**いずれも Wayback Machine からダウンロード可能**（2026年3月時点確認済み）。

| ファイル名 | 内容 | Wayback Machine URL |
|---|---|---|
| `XDS25a10.LZH` | X-DOS V2.5a Rel.1.0 システムディスク（D88 イメージ） | https://web.archive.org/web/20021022075959/http://home4.highway.ne.jp/regulus/file/XDS25a10.LZH |
| `XDSUTL10.LZH` | ユーティリティディスク Rel.1.0（D88 イメージ） | https://web.archive.org/web/20010821180208/http://home4.highway.ne.jp/regulus/file/XDSUTL10.LZH |

> **注意**: 調査時に `/regulus/xdos/file/` パスを誤って検索したため「アーカイブなし」と
> 一時記録したが、正しいパスは `/regulus/file/` であり、アーカイブは存在する。

### その他のコレクション

| コレクション | URL | X-DOS 収録状況 |
|---|---|---|
| Neo Kobe Sharp X1 (2016) | `https://archive.org/details/Neo_Kobe_Sharp_X1_2016-02-25` | **未収録**（確認済み） |
| TOSEC Sharp X1 (2012) | `https://archive.org/details/Sharp_X1_TOSEC_2012_04_23` | 未確認 |

---

## ユーザー向け案内文（参考）

README 等に記載する場合の文例:

> X-DOS 対応機能を使用するには、X-DOS のディスクイメージ（D88 形式）を別途用意してください。
> X-DOS は作者 Regulus が公式サイトで無償配布していた OS ですが、現在サイトは消滅しています。
> ディスクイメージは Wayback Machine に以下の URL でアーカイブされています：
>
> - システムディスク: https://web.archive.org/web/20021022075959/http://home4.highway.ne.jp/regulus/file/XDS25a10.LZH
> - ユーティリティ: https://web.archive.org/web/20010821180208/http://home4.highway.ne.jp/regulus/file/XDSUTL10.LZH
>
> ダウンロード後、LZH を展開すると D88 形式のディスクイメージが得られます。
> 実機やエミュレータ用に X-DOS のディスクイメージを既に所有している場合は、そのまま使用できます。

---

## 参考: 公式サイトに記載されていたファイルシステム仕様

```
ファイルシステム仕様（公式サイト原文）:
  オリジナル特殊フォーマット採用（拡張 FAM 方式）
    2D   L=2*10Sector  400KB
    2DD  L=2*10Sector  800KB
    2HD  L=2*16Sector  1.2MB
  ファイル名: 16 文字 + ファイルタイプ 3 文字（拡張子）
  階層ディレクトリ対応
  システムコマンドディレクトリ（PATH）対応
  ファイルタイプ連動コマンド起動
  S-OS "SWORD" 互換システムコール API（エントリアドレスは異なる）
```

この仕様の特筆点として、**2HD のみトラックあたり 16 セクタ**（2D/2DD は 10 セクタ）。
現在の実装は 2D/2DD のみ対応（`SectorsPerTrack = 10` 固定）。
2HD 対応は将来課題（Roadmap_V2.md 参照）。
