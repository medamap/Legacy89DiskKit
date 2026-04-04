using Legacy89DiskKit.CharacterEncoding.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Registry;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.Native.Domain;
using Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;
using Legacy89DiskKit.DiskImage.Infrastructure.Factory;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider;
using Legacy89DiskKit.FileSystem.Infrastructure.Msx.Provider;
using Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Provider;
using Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider;

namespace Legacy89DiskKit.Native.Application;

public sealed class ManagedNativeBridgeBackend : INativeBridgeBackend
{
    private readonly IFileSystemRegistry _registry;
    private IEncoderRegistry? _defaultEncoderRegistry;
    private readonly DiskContainerFactory _containerFactory = new();

    public string BackendKind => "managed-bridge";

    public string BackendImplementation => "Legacy89DiskKit.NativeInterop";

    public string BackendTarget => "Legacy89DiskKit.Application";

    public ManagedNativeBridgeBackend(IFileSystemRegistry? registry = null)
    {
        _registry = registry ?? CreateDefaultRegistry();
    }

    public INativeDiskSession OpenDisk(string path, bool readOnly)
    {
        var container = _containerFactory.Open(path, readOnly);
        var fs = _registry.DetectAndCreate(container);
        return new ManagedNativeDiskSession(container, fs);
    }

    public INativeDiskSession OpenDisk(byte[] imageData, string imageFormat, bool readOnly)
    {
        var container = _containerFactory.Open(imageData, imageFormat, readOnly);
        var fs = _registry.DetectAndCreate(container);
        return new ManagedNativeDiskSession(container, fs);
    }

    public INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName)
    {
        var container = _containerFactory.Create(path, diskType, diskName);
        var fs = _registry.DetectAndCreate(container);
        return new ManagedNativeDiskSession(container, fs);
    }

    private static IFileSystemRegistry CreateDefaultRegistry()
    {
        var registry = new FileSystemRegistry();
        registry.Register(new XDosFileSystemProvider());
        registry.Register(new HuBasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Cpm.Provider.CpmFileSystemProvider());
        registry.Register(new N88BasicFileSystemProvider());
        registry.Register(new MsxDosFileSystemProvider());
        return registry;
    }

    public IFileSystemRegistry GetRegistry() => _registry;

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
