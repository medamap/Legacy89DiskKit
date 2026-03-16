using Legacy89DiskKit.Application.CharacterEncoding;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class ManagedNativeBridgeBackend : INativeBridgeBackend
{
    private IFileSystemRegistry? _defaultRegistry;
    private IEncoderRegistry? _defaultEncoderRegistry;

    public string BackendKind => "managed-bridge";

    public string BackendImplementation => "Legacy89DiskKit.NativeInterop";

    public string BackendTarget => "Legacy89DiskKit.Application";

    public INativeDiskSession OpenDisk(string path, bool readOnly)
    {
        return NativeSessionFactory.OpenDisk(path, readOnly, GetDefaultRegistry());
    }

    public INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName)
    {
        return NativeSessionFactory.CreateDisk(path, diskType, diskName, GetDefaultRegistry());
    }

    public IFileSystemRegistry GetDefaultRegistry()
    {
        if (_defaultRegistry == null)
        {
            var registry = new FileSystemRegistry();
            registry.Register(new HuBasicFileSystemProvider());
            registry.Register(new N88BasicFileSystemProvider());
            registry.Register(new MsxDosFileSystemProvider());
            _defaultRegistry = registry;
        }
        return _defaultRegistry;
    }

    public IEncoderRegistry GetDefaultEncoderRegistry()
    {
        if (_defaultEncoderRegistry == null)
        {
            var registry = new EncoderRegistry();
            registry.Register("X1", new X1CharacterEncoder());
            registry.Register("PC88", new Pc8801CharacterEncoder());
            registry.Register("MSX", new Msx1CharacterEncoder());
            _defaultEncoderRegistry = registry;
        }
        return _defaultEncoderRegistry;
    }
}
