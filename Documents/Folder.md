# フォルダ構造

## 概要

- フォルダ構造は基本的に DDD (ドメイン駆動設計) 準拠の設計を採用しています.
- トップフォルダにはそれぞれのドメイン責任において決定されたフォルダが配置されます
  - Application: ドメイン責任の Domain Model または Infrastructure にアクセスするための API やサービスが提供されます、Domain や Infrastructure へのアクセスは基本的には Application からアクセスされます
  - Domain: ドメイン責任のビジネスロジックが配置されます(大抵はモデル定義またはインターフェイス定義がほとんどです)
    - Model: ドメインモデルが配置されます
    - Interface: ドメインインターフェイスが配置されます
      - Repository: ドメインのリポジトリインターフェイスが配置されます
      - Service: ドメインのサービスインターフェイスが配置されます
      - State: ステートパターンの状態インターフェイス及び、ステートを表す列挙型が配置されます
    - ValueObject: ドメインの値オブジェクトが配置されます
    - Exception: ドメインの例外定義が配置されます
    - Entity: ドメインのエンティティが配置されます
    - Enum: ドメインの列挙型が配置されます
    - その他必要に応じてカテゴリ分けした方が良いと判断された場合には、それに応じたフォルダを配置します
  - Infrastructure: ドメイン責任のインフラストラクチャ層が配置されます(大抵はインターフェイスに対する実装がほとんどです)
    - ほとんどが Domain Interface の実装となり、Domain と同様のフォルダ構造を採用します

## フォルダ分けの例

```
+ Legacy89DiskKit
  + Documents
    + D88_Format.md
    + Hu-BASIC_Format.md
    + Folder.md
    + README.md
  + CSharp (or Cpp)
    + DiskImage
      + Application
        + DiskImageService.cs
      + Domain
        + Interface
          + Container
            + IDiskContainer.cs
      + Infrastructure
        + Container
          + D88DiskContainer.cs (D88形式のディスクイメージを表すコンテナで、Domain Interface の IDiskContainer.cs を実装します)
    + FileSystem
      + Application
        + FileSystemService.cs
      + Domain
        + Interface
          + FileSystem
            + IFileSystem.cs
      + Infrastructure
        + FileSystem
          + HuBasicFileSystem.cs (Hu-BASICを表すファイルシステムで、Domain Interface の IFileSystem.cs を実装します)
          + MsdosFileSystem.cs (MS-DOSを表すファイルシステムで、Domain Interface の IFileSystem.cs を実装します)
```

- CSharp (または Cpp) 以下のフォルダ構造は、それぞれのドメイン責任においてこのようにDDD準拠の設計を採用している例を表しており、必ずしもこの通りに実装する必要はありません
- ドメインを跨ぐアクセスは基本的には Application 層を介して行う必要があります

