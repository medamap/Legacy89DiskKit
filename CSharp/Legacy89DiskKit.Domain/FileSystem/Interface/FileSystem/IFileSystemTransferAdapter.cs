using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

public interface IFileSystemTransferAdapter
{
    TransferFileEnvelope Export(string fileName);
    void Import(TransferFileEnvelope envelope, string destFileName);
}
