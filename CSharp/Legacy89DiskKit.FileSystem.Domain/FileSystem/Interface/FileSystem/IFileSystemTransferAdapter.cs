using Legacy89DiskKit.FileSystem.Domain.Model;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

public interface IFileSystemTransferAdapter
{
    string FileSystemId { get; }
    bool Supports(IFileSystem fs);
    TransferFileEnvelope Export(FileEntry entry);
    void Import(TransferFileEnvelope envelope, string destFileName);
}
