# TODO: ブート情報ドメインの独立化

## 背景
現在、ブート情報（ブートセクタ）はファイルシステムに依存した実装になっているが、本来ブート情報はファイルシステムとは独立した概念である。機種判別やブート可能性の判定をより適切に行うため、ブート情報を独立したドメインとして分離する必要がある。

## 現状の問題点
1. `IFileSystem`インターフェースが`HuBasic.BootSector`型を返すため、他のファイルシステムでの実装が不自然
2. ブート情報の扱いが統一されていない（MSX-DOSは独自の`MsxDosBootSector`クラスを持つ）
3. 機種情報がファイルシステム判定ロジックに埋もれている
4. 同じブート形式を複数のファイルシステムで再利用できない

## 提案する解決策

### 新しいドメイン構造
```
Legacy89DiskKit/
├── BootInfo/                    # 新しいドメイン
│   ├── Domain/
│   │   ├── Interface/
│   │   │   ├── IBootInfoService.cs
│   │   │   ├── IBootSector.cs
│   │   │   └── Factory/
│   │   │       └── IBootSectorFactory.cs
│   │   ├── Model/
│   │   │   ├── MachineBootType.cs  # X1, PC88, MSX等
│   │   │   ├── BootCapability.cs   # 起動可能性の情報
│   │   │   └── BootSectorInfo.cs   # 共通ブート情報
│   │   └── Exception/
│   │       └── BootInfoException.cs
│   ├── Application/
│   │   └── BootInfoService.cs
│   └── Infrastructure/
│       ├── BootSector/
│       │   ├── X1BootSector.cs
│       │   ├── Pc88BootSector.cs
│       │   ├── MsxBootSector.cs
│       │   └── GenericBootSector.cs
│       └── Factory/
│           └── BootSectorFactory.cs
```

### 主要インターフェース
```csharp
public interface IBootInfoService
{
    MachineBootType DetectMachineType(IDiskContainer container);
    IBootSector GetBootSector(IDiskContainer container);
    void WriteBootSector(IDiskContainer container, IBootSector bootSector);
    bool IsBootable(IDiskContainer container);
}

public interface IBootSector
{
    MachineBootType MachineType { get; }
    bool IsBootable { get; }
    string Label { get; }
    byte[] GetRawData();
}
```

## 実装手順
1. 新しいBootInfoドメインを作成
2. 既存のブート情報関連コードを調査・整理
3. 段階的に既存コードを新ドメインに移行
4. `IFileSystem`からブート関連メソッドを削除
5. `FileSystemFactory`の機種判別ロジックを`BootInfoService`に移管
6. テストケースの追加

## 影響範囲
- `IFileSystem`インターフェース
- `HuBasicFileSystem`のブート処理
- `MsxDosFileSystem`のブートセクタ処理
- `FileSystemFactory`の判定ロジック
- CLI Shell（infoコマンドなど）

## 期待される効果
1. **保守性向上**: ブート情報とファイルシステムの責務が明確に分離
2. **拡張性向上**: 新しい機種のブート形式を追加しやすくなる
3. **再利用性向上**: 同一のブート形式を複数のファイルシステムで共有可能
4. **テスタビリティ向上**: ブート情報の単体テストが書きやすくなる

## 注意事項
- 後方互換性を保つため、段階的な移行を行う
- 既存のAPIは非推奨（Obsolete）として残し、次のメジャーバージョンで削除