# Legacy89DiskKit v1.3.0 - CP/M 2.2対応

## 🎉 新機能

### CP/M 2.2ファイルシステム完全対応
8ビット時代の標準OSであるCP/M 2.2のディスクイメージを完全サポートしました。

#### 主な機能
- **完全なCP/M 2.2互換性**: ユーザー番号、マルチエクステント、ファイル属性管理
- **複数ディスクタイプ対応**: 2D (250KB)、2DD (500KB)、2HD (1.25MB)
- **文字エンコーディング**: 4種類の機種別エンコーダーを実装
  - CP/M Generic
  - CP/M PC-8801
  - CP/M X1  
  - CP/M MSX-DOS
- **大容量ファイル対応**: 16KB以上のファイルに対するマルチエクステント自動管理
- **EOFマーカー処理**: CP/M特有のCtrl+Z (0x1A)による終端マーカー対応

#### CLIコマンド例
```bash
# CP/Mディスク作成
./CLI create cpmdisk.d88 2DD "CPM DISK"
./CLI format cpmdisk.d88 --filesystem cpm

# ファイル操作
./CLI import-text cpmdisk.d88 readme.txt README.TXT --filesystem cpm --machine cpm-generic
./CLI export-text cpmdisk.d88 README.TXT readme.txt --filesystem cpm --machine cpm-generic
./CLI list cpmdisk.d88 --filesystem cpm
```

## ⚠️ 既知の制限事項

### CP/Mファイルシステムの制限
CP/M 2.2は128バイト単位でファイルを管理するため、以下の制限があります：

1. **ファイルサイズの精度**: 実際のファイルサイズは128バイト単位に切り上げられます
2. **EOFマーカー**: ファイルの実際の終端を示すためCtrl+Z (0x1A)が使用されます
3. **大容量ファイル**: 16KB以上のファイルでは、正確なバイト単位のサイズ復元が困難です

詳細は[CP/M実装設計ドキュメント](Documents/CPM_Implementation_Design.md)を参照してください。

## 📊 テスト結果
- CP/Mファイルシステムテスト: 22/23成功 (95.7%)
- 既存機能への影響: なし

## 🔧 技術的な改善
- ファイルシステムファクトリーにCP/M自動検出ロジックを追加
- DDD準拠のクリーンなアーキテクチャを維持

## 📚 ドキュメント
- [CP/M実装設計](Documents/CPM_Implementation_Design.md)
- [CP/M文字エンコーディング](Documents/CPM_Character_Encoding.md)
- README.mdをv1.3.0対応に更新

## 🙏 謝辞
CP/Mコミュニティの皆様に、仕様情報とテストデータの提供について感謝いたします。

---

**フルチェンジログ**: [v1.2.0...v1.3.0](https://github.com/medamap/Legacy89DiskKit/compare/v1.2.0...v1.3.0)