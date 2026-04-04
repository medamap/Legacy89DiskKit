using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;

namespace Legacy89DiskKit.FileSystem.Application;

public class FileSystemTransferOrchestrator
{
    private readonly Dictionary<IFileSystem, IFileSystemTransferAdapter> _instanceRegistry = new(ReferenceEqualityComparer.Instance);
    private readonly List<IFileSystemTransferAdapter>                    _typeAdapters     = new();

    public void Register(IFileSystem fs, IFileSystemTransferAdapter adapter)
        => _instanceRegistry[fs] = adapter;

    public void Register(IFileSystemTransferAdapter adapter)
        => _typeAdapters.Add(adapter);

    private IFileSystemTransferAdapter Resolve(IFileSystem fs)
    {
        if (_instanceRegistry.TryGetValue(fs, out var instanceAdapter)) return instanceAdapter;
        var typeAdapter = _typeAdapters.LastOrDefault(a => a.Supports(fs));
        if (typeAdapter != null) return typeAdapter;
        throw new InvalidOperationException($"No adapter registered for filesystem type '{fs.GetType().Name}'.");
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
            .FirstOrDefault(e => e.FullName == sourceFileName)
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
