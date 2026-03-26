using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Application.FileSystem;

public class FileSystemTransferOrchestrator
{
    private readonly Dictionary<IFileSystem, IFileSystemTransferAdapter> _instanceRegistry = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, IFileSystemTransferAdapter>      _typeRegistry     = new();

    public void Register(IFileSystem fs, IFileSystemTransferAdapter adapter)
        => _instanceRegistry[fs] = adapter;

    public void Register(IFileSystemTransferAdapter adapter)
        => _typeRegistry[adapter.FileSystemId] = adapter;

    private IFileSystemTransferAdapter Resolve(IFileSystem fs)
    {
        if (_instanceRegistry.TryGetValue(fs, out var instanceAdapter)) return instanceAdapter;
        var id = fs.GetFileSystemInfo().FileSystemName;
        if (_typeRegistry.TryGetValue(id, out var typeAdapter)) return typeAdapter;
        throw new InvalidOperationException($"No adapter registered for filesystem: {id}");
    }

    public void Transfer(
        IFileSystem sourceFs,
        IFileSystem destFs,
        string sourceFileName,
        string destFileName)
    {
        var srcAdapter = Resolve(sourceFs);
        var dstAdapter = Resolve(destFs);

        var entry = sourceFs.GetFiles()
            .FirstOrDefault(e => e.FileName == sourceFileName)
            ?? throw new FileNotFoundException($"File not found: {sourceFileName}");

        var envelope = srcAdapter.Export(entry);
        dstAdapter.Import(envelope, destFileName);
    }

    public void TransferAll(IFileSystem sourceFs, IFileSystem destFs)
    {
        var srcAdapter = Resolve(sourceFs);
        var dstAdapter = Resolve(destFs);

        foreach (var entry in sourceFs.GetFiles())
            dstAdapter.Import(srcAdapter.Export(entry), entry.FileName);
    }
}
