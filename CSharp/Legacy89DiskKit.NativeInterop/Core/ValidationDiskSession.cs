using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class ValidationDiskSession : INativeDiskSession
{
    private readonly INativeDiskSession _reference;
    private readonly INativeDiskSession _target;
    private readonly string? _targetPath;
    private ValidationFileSystem? _fileSystem;

    public ValidationDiskSession(INativeDiskSession reference, INativeDiskSession target, string? targetPath = null)
    {
        _reference = reference;
        _target = target;
        _targetPath = targetPath;
    }

    public IFileSystem? FileSystem
    {
        get
        {
            if (_fileSystem == null && _reference.FileSystem != null && _target.FileSystem != null)
            {
                _fileSystem = new ValidationFileSystem(_reference.FileSystem, _target.FileSystem);
            }
            return _fileSystem;
        }
    }

    public DiskContainerMetadata? GetContainerMetadata()
    {
        var refMeta = _reference.GetContainerMetadata();
        var targetMeta = _target.GetContainerMetadata();

        if (refMeta != null && targetMeta != null)
        {
            Validate(refMeta, targetMeta);
        }

        return refMeta;
    }

    private void Validate(DiskContainerMetadata refMeta, DiskContainerMetadata targetMeta)
    {
        if (refMeta.ImageFormat != targetMeta.ImageFormat)
            throw new Exception($"Validation Error: ImageFormat mismatch. Ref: {refMeta.ImageFormat}, Target: {targetMeta.ImageFormat}");
        
        if (refMeta.DiskType != targetMeta.DiskType)
            throw new Exception($"Validation Error: DiskType mismatch. Ref: {refMeta.DiskType}, Target: {targetMeta.DiskType}");

        if (refMeta.Geometry != targetMeta.Geometry)
            throw new Exception($"Validation Error: Geometry mismatch. Ref: {refMeta.Geometry}, Target: {targetMeta.Geometry}");
    }

    public void CloseDisk()
    {
        _reference.CloseDisk();
        _target.CloseDisk();
    }

    public void Dispose()
    {
        _reference.Dispose();
        _target.Dispose();
        
        // Clean up target temporary disk image
        if (_targetPath != null && File.Exists(_targetPath))
        {
            try { File.Delete(_targetPath); } catch { /* ignore */ }
        }
    }
}
