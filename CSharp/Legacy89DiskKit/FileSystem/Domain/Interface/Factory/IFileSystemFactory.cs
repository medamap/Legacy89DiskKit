using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.Factory;

public interface IFileSystemFactory
{
    // 新規作成：ファイルシステム指定必須
    IFileSystem CreateFileSystem(IDiskContainer container, FileSystemType fileSystemType);
    
    // 読み取り専用：自動判定OK（安全）
    IFileSystem OpenFileSystemReadOnly(IDiskContainer container);
    
    // 読み書き：ファイルシステム指定必須（安全）
    IFileSystem OpenFileSystem(IDiskContainer container, FileSystemType fileSystemType);
    
    // 推測：参考情報のみ（書き込み禁止）
    FileSystemType GuessFileSystemType(IDiskContainer container);
    
    // サポート一覧
    IEnumerable<FileSystemType> GetSupportedFileSystemTypes();
}

public enum FileSystemType
{
    HuBasic,
    Fat12,
    Fat16,
    Cpm,
    Cdos,
    N88Basic,
    MsxDos
}