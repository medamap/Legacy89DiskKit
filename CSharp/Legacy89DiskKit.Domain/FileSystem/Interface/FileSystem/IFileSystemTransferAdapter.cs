using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

public interface IFileSystemTransferAdapter
{
    string FileSystemId { get; }
    bool Supports(IFileSystem fs);
    TransferFileEnvelope Export(FileEntry entry);
    void Import(TransferFileEnvelope envelope, string destFileName);
}
