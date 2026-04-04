using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.Native.Domain;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class ValidationDiskSession : INativeDiskSession, IDiskContainer
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

    // INativeDiskSession
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

    // IDiskContainer
    public string FilePath => (_reference as IDiskContainer)?.FilePath ?? "";
    public bool IsReadOnly => (_reference as IDiskContainer)?.IsReadOnly ?? true;
    public DiskType DiskType => (_reference as IDiskContainer)?.DiskType ?? DiskType.TwoD;
    public DiskContainerMetadata GetMetadata() => GetContainerMetadata() ?? throw new InvalidOperationException();

    public byte[] ReadSector(int cylinder, int head, int sector)
    {
        var refData = (_reference as IDiskContainer)!.ReadSector(cylinder, head, sector);
        var targetData = (_target as IDiskContainer)!.ReadSector(cylinder, head, sector);
        if (!refData.SequenceEqual(targetData))
            throw new Exception($"Validation Error: ReadSector mismatch at C{cylinder}H{head}S{sector}");
        return refData;
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted)
    {
        var refData = (_reference as IDiskContainer)!.ReadSector(cylinder, head, sector, allowCorrupted);
        var targetData = (_target as IDiskContainer)!.ReadSector(cylinder, head, sector, allowCorrupted);
        if (!refData.SequenceEqual(targetData))
            throw new Exception($"Validation Error: ReadSector mismatch at C{cylinder}H{head}S{sector}");
        return refData;
    }

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        (_reference as IDiskContainer)!.WriteSector(cylinder, head, sector, data);
        (_target as IDiskContainer)!.WriteSector(cylinder, head, sector, data);
    }

    public bool SectorExists(int cylinder, int head, int sector) => (_reference as IDiskContainer)!.SectorExists(cylinder, head, sector);
    public IEnumerable<SectorInfo> GetAllSectors() => (_reference as IDiskContainer)!.GetAllSectors();
    public void Save()
    {
        (_reference as IDiskContainer)!.Save();
        (_target as IDiskContainer)!.Save();
    }
    public void SaveAs(string filePath) => throw new NotSupportedException();

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
