using Legacy89DiskKit.Application.CharacterEncoding;
using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Domain.Timing.Interface;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;

namespace Legacy89DiskKit.Application;

/// <summary>
/// Provides the supported managed bootstrap surface for Legacy89DiskKit.
/// </summary>
public static class Legacy89DiskKitApplication
{
    /// <summary>
    /// Creates a preconfigured disk service with the supported filesystem providers.
    /// </summary>
    public static DiskImage.DiskService CreateDiskService()
    {
        return new DiskImage.DiskService(fsRegistry: CreateFileSystemRegistry());
    }

    /// <summary>
    /// Creates a preconfigured file transfer service for the specified filesystem info.
    /// </summary>
    public static FileSystem.FileTransferService CreateFileTransferService(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var encoder = ResolveEncoder(fsInfo, encodingOverride);
        return new FileSystem.FileTransferService(encoder);
    }

    /// <summary>
    /// Creates the supported directory layout service.
    /// </summary>
    public static FileSystem.DirectoryLayoutService CreateDirectoryLayoutService()
    {
        return new FileSystem.DirectoryLayoutService();
    }

    public static Drive.DriveMountService CreateDriveMountService()
    {
        return new Drive.DriveMountService();
    }

    public static Drive.MountedMediumBindingService CreateMountedMediumBindingService()
    {
        return new Drive.MountedMediumBindingService();
    }

    public static Fdc.FdcAccessService CreateFdcAccessService(IFdcController controller, IControllerClock? clock = null)
    {
        return new Fdc.FdcAccessService(controller, clock);
    }

    public static Fdc.Hosts.EventDrivenEmulatorFdcHostAdapter CreateEventDrivenEmulatorFdcHostAdapter()
    {
        return new Fdc.Hosts.EventDrivenEmulatorFdcHostAdapter(CreateDriveMountService(), CreateMountedMediumBindingService());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostProtocolEndpoint CreateEmulatorHostProtocolEndpoint()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostProtocolEndpoint(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostProtocolTextSession CreateEmulatorHostProtocolTextSession()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostProtocolTextSession(CreateEmulatorHostProtocolEndpoint());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostObservableProtocolSession CreateEmulatorHostObservableProtocolSession()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostObservableProtocolSession(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    public static Fdc.Hosts.XmilWebStyleFdcHostAdapter CreateXmilWebStyleFdcHostAdapter()
    {
        return new Fdc.Hosts.XmilWebStyleFdcHostAdapter(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    /// <summary>
    /// Creates the supported explicit filesystem resolver.
    /// </summary>
    public static FileSystem.ExplicitFileSystemResolver CreateExplicitFileSystemResolver()
    {
        return new FileSystem.ExplicitFileSystemResolver();
    }

    /// <summary>
    /// Creates the supported disk clone service.
    /// </summary>
    public static FileSystem.DiskCloneService CreateDiskCloneService(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var transferService = CreateFileTransferService(fsInfo, encodingOverride);
        return new FileSystem.DiskCloneService(transferService);
    }

    /// <summary>
    /// Creates the supported filesystem registry used by the managed public surface.
    /// </summary>
    public static IFileSystemRegistry CreateFileSystemRegistry()
    {
        var registry = new FileSystem.FileSystemRegistry();
        registry.Register(new HuBasicFileSystemProvider());
        registry.Register(new N88BasicFileSystemProvider());
        registry.Register(new MsxDosFileSystemProvider());
        return registry;
    }

    /// <summary>
    /// Creates the supported encoder registry used by the managed public surface.
    /// </summary>
    public static IEncoderRegistry CreateEncoderRegistry()
    {
        var registry = new EncoderRegistry();
        registry.Register("X1", new X1CharacterEncoder());
        registry.Register("SJIS", new ShiftJisCharacterEncoder());
        registry.Register("Shift-JIS", new ShiftJisCharacterEncoder());
        registry.Register("MSX", new ShiftJisCharacterEncoder());
        registry.Register("PC88", new ShiftJisCharacterEncoder());
        return registry;
    }

    /// <summary>
    /// Resolves the supported encoder for the specified filesystem info.
    /// </summary>
    public static ICharacterEncoder ResolveEncoder(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var registry = CreateEncoderRegistry();
        return new CharacterEncodingResolver(registry).ResolveEncoder(fsInfo, encodingOverride);
    }

    /// <summary>
    /// Resolves the supported logical character encoding profile for the specified filesystem info.
    /// </summary>
    public static CharacterEncodingProfile ResolveEncodingProfile(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var registry = CreateEncoderRegistry();
        return new CharacterEncodingResolver(registry).ResolveProfile(fsInfo, encodingOverride);
    }
}
