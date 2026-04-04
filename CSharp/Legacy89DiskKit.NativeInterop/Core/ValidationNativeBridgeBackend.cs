using Legacy89DiskKit.Native.Application;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.Native.Domain;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class ValidationNativeBridgeBackend : INativeBridgeBackend
{
    private readonly INativeBridgeBackend _reference;
    private readonly INativeBridgeBackend _target;

    public ValidationNativeBridgeBackend(INativeBridgeBackend reference, INativeBridgeBackend target)
    {
        _reference = reference;
        _target = target;
    }

    public string BackendKind => $"validation({_reference.BackendKind} vs {_target.BackendKind})";

    public string BackendImplementation => "ValidationNativeBridgeBackend";

    public string BackendTarget => _target.BackendTarget;

    public INativeDiskSession OpenDisk(string path, bool readOnly)
    {
        // For OpenDisk, we need two separate image files to avoid locks and state pollution
        string targetPath = GetTargetPath(path);
        if (File.Exists(path) && !File.Exists(targetPath))
        {
            File.Copy(path, targetPath, overwrite: true);
        }

        var refSession = _reference.OpenDisk(path, readOnly);
        var targetSession = _target.OpenDisk(targetPath, readOnly);
        return new ValidationDiskSession(refSession, targetSession, targetPath);
    }

    public INativeDiskSession OpenDisk(byte[] imageData, string imageFormat, bool readOnly)
    {
        var refSession = _reference.OpenDisk(imageData, imageFormat, readOnly);
        var targetSession = _target.OpenDisk(imageData, imageFormat, readOnly);
        return new ValidationDiskSession(refSession, targetSession);
    }

    public INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName)
    {
        // For CreateDisk, we create two separate files
        string targetPath = GetTargetPath(path);
        
        var refSession = _reference.CreateDisk(path, diskType, diskName);
        var targetSession = _target.CreateDisk(targetPath, diskType, diskName);
        return new ValidationDiskSession(refSession, targetSession, targetPath);
    }

    private static string GetTargetPath(string path)
    {
        string dir = Path.GetDirectoryName(path) ?? "";
        string name = Path.GetFileNameWithoutExtension(path);
        string ext = Path.GetExtension(path);
        return Path.Combine(dir, $"{name}.target{ext}");
    }
}
