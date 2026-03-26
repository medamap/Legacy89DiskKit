using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Application.FileSystem;

public class FileSystemTransferOrchestrator
{
    public void Transfer(
        IFileSystem sourceFs,
        IFileSystemTransferAdapter sourceAdapter,
        IFileSystemTransferAdapter destAdapter,
        string sourceFileName,
        string destFileName)
    {
        var entry = sourceFs.GetFiles()
            .FirstOrDefault(e => e.FileName == sourceFileName)
            ?? throw new FileNotFoundException($"File not found: {sourceFileName}");
        var envelope = sourceAdapter.Export(entry);
        destAdapter.Import(envelope, destFileName);
    }

    public void TransferAll(
        IFileSystem sourceFs,
        IFileSystemTransferAdapter sourceAdapter,
        IFileSystemTransferAdapter destAdapter)
    {
        foreach (var entry in sourceFs.GetFiles())
            destAdapter.Import(sourceAdapter.Export(entry), entry.FileName);
    }
}
