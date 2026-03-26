using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Application.FileSystem;

public class FileSystemTransferOrchestrator
{
    public void Transfer(
        IFileSystemTransferAdapter source,
        IFileSystemTransferAdapter dest,
        string sourceFileName,
        string destFileName)
    {
        var envelope = source.Export(sourceFileName);
        dest.Import(envelope, destFileName);
    }

    public void TransferAll(
        IFileSystemTransferAdapter source,
        IFileSystemTransferAdapter dest,
        IEnumerable<string> fileNames)
    {
        foreach (var name in fileNames)
            Transfer(source, dest, name, name);
    }
}
