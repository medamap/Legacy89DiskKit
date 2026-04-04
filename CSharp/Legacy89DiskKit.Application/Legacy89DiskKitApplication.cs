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
using Legacy89DiskKit.Infrastructure.DiskImage.Factory;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;

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

    /// <summary>
    /// Creates the supported boot profile service.
    /// </summary>
    public static Legacy89DiskKit.FileSystem.Application.IBootProfileService CreateBootProfileService()
    {
        return new Legacy89DiskKit.FileSystem.Application.CompositeBootProfileService();
    }

    public static Legacy89DiskKit.FileSystem.Application.IBootEntryExportService CreateBootEntryExportService()
    {
        return new FileSystem.BootEntryExportService();
    }

    public static Legacy89DiskKit.FileSystem.Application.IBootEntryImportService CreateBootEntryImportService()
    {
        return new FileSystem.BootEntryImportService();
    }

    public static FileSystem.DiskInspectionService CreateDiskInspectionService()
    {
        return new FileSystem.DiskInspectionService();
    }

    public static FileSystem.FileInspectionService CreateFileInspectionService()
    {
        return new FileSystem.FileInspectionService();
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
        return new Fdc.Hosts.EventDrivenEmulatorFdcHostAdapter(
            CreateDriveMountService(),
            CreateMountedMediumBindingService(),
            new DiskContainerFactory());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostProtocolEndpoint CreateEmulatorHostProtocolEndpoint()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostProtocolEndpoint(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostProtocolTextSession CreateEmulatorHostProtocolTextSession()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostProtocolTextSession(CreateEmulatorHostProtocolEndpoint());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostProtocolStdioRunner CreateEmulatorHostProtocolStdioRunner()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostProtocolStdioRunner(CreateEmulatorHostProtocolTextSession());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostObservableProtocolSession CreateEmulatorHostObservableProtocolSession()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostObservableProtocolSession(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    public static Fdc.Hosts.Protocol.EmulatorHostObservableProtocolStdioRunner CreateEmulatorHostObservableProtocolStdioRunner()
    {
        return new Fdc.Hosts.Protocol.EmulatorHostObservableProtocolStdioRunner(CreateEmulatorHostObservableProtocolSession());
    }

    public static IReadOnlyList<Fdc.Hosts.Protocol.EmulatorHostRequest> CreateReadOnlyD88PathScript(string imagePath, int driveNumber = 0)
    {
        return Fdc.Hosts.Scripting.EmulatorHostRequestScriptFactory.CreateReadOnlyD88ByPathSequence(imagePath, driveNumber);
    }

    public static IReadOnlyList<Fdc.Hosts.Protocol.EmulatorHostRequest> CreateReadOnlyD88BufferScript(byte[] imageData, string imageFormat = "d88", int driveNumber = 0)
    {
        return Fdc.Hosts.Scripting.EmulatorHostRequestScriptFactory.CreateReadOnlyD88ByBufferSequence(imageData, imageFormat, driveNumber);
    }

    public static IReadOnlyList<Fdc.Hosts.Protocol.EmulatorHostRequest> CreateReadOnlyRawBufferScript(byte[] imageData, string imageFormat = "2d", int driveNumber = 0)
    {
        return Fdc.Hosts.Scripting.EmulatorHostRequestScriptFactory.CreateReadOnlyRawByBufferSequence(imageData, imageFormat, driveNumber);
    }

    public static async Task<Fdc.Hosts.Scripting.EmulatorHostBundle> ReadEmulatorHostBundleAsync(
        string outputDirectory,
        string baseName,
        CancellationToken cancellationToken = default)
    {
        return await Fdc.Hosts.Scripting.EmulatorHostBundleReader.ReadAsync(outputDirectory, baseName, cancellationToken);
    }

    public static Fdc.Hosts.Scripting.EmulatorHostProofReport BuildEmulatorHostProofReport(
        IReadOnlyList<Fdc.Hosts.Scripting.EmulatorHostTranscriptEntry> transcript,
        string openMode,
        string exchangeMode)
    {
        return Fdc.Hosts.Scripting.EmulatorHostProofReportBuilder.Build(transcript, openMode, exchangeMode);
    }

    public static IReadOnlyList<string> CompareEmulatorHostBundle(
        Fdc.Hosts.Scripting.EmulatorHostBundle bundle,
        Fdc.Hosts.Scripting.EmulatorHostProofExpectation expectation)
    {
        return Fdc.Hosts.Scripting.EmulatorHostBundleComparer.Compare(bundle, expectation);
    }

    public static IReadOnlyList<string> CompareEmulatorHostProofReport(
        Fdc.Hosts.Scripting.EmulatorHostProofReport report,
        Fdc.Hosts.Scripting.EmulatorHostProofExpectation expectation)
    {
        return Fdc.Hosts.Scripting.EmulatorHostProofReportComparer.Compare(report, expectation);
    }

    public static async Task<IReadOnlyList<Fdc.Hosts.Scripting.EmulatorHostTranscriptEntry>> ReadEmulatorHostTranscriptAsync(
        string transcriptPath,
        CancellationToken cancellationToken = default)
    {
        return await Fdc.Hosts.Scripting.EmulatorHostTranscriptFileStore.LoadAsync(transcriptPath, cancellationToken);
    }

    public static async Task<IReadOnlyList<Fdc.Hosts.Protocol.EmulatorHostRequest>> ReadEmulatorHostRequestScriptAsync(
        string requestScriptPath,
        CancellationToken cancellationToken = default)
    {
        return await Fdc.Hosts.Scripting.EmulatorHostRequestScriptFileStore.LoadAsync(requestScriptPath, cancellationToken);
    }

    public static async Task WriteEmulatorHostBundleAsync(
        string outputDirectory,
        string baseName,
        Fdc.Hosts.Scripting.EmulatorHostProofReport report,
        IReadOnlyList<Fdc.Hosts.Scripting.EmulatorHostTranscriptEntry> transcript,
        IReadOnlyList<Fdc.Hosts.Protocol.EmulatorHostRequest>? requestScript = null,
        CancellationToken cancellationToken = default)
    {
        await Fdc.Hosts.Scripting.EmulatorHostBundleWriter.WriteAsync(
            outputDirectory,
            baseName,
            report,
            transcript,
            requestScript,
            cancellationToken);
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
        var encoderRegistry = CreateEncoderRegistry();
        var normalizationService = new Services.FileNameNormalizationService(encoderRegistry);
        return new FileSystem.DiskCloneService(transferService, normalizationService);
    }

    /// <summary>
    /// Creates the supported filesystem registry used by the managed public surface.
    /// </summary>
    public static IFileSystemRegistry CreateFileSystemRegistry()
    {
        var registry = new FileSystem.FileSystemRegistry();
        registry.Register(new XDosFileSystemProvider());
        registry.Register(new HuBasicFileSystemProvider());
        registry.Register(new Infrastructure.FileSystem.Cpm.Provider.CpmFileSystemProvider());
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
        registry.Register("ShiftJIS", new ShiftJisCharacterEncoder());
        registry.Register("Shift-JIS", new ShiftJisCharacterEncoder());
        registry.Register("Shift_JIS", new ShiftJisCharacterEncoder());
        registry.Register("sjis", new ShiftJisCharacterEncoder());
        registry.Register("shiftjis", new ShiftJisCharacterEncoder());
        registry.Register("shift-jis", new ShiftJisCharacterEncoder());
        registry.Register("shift_jis", new ShiftJisCharacterEncoder());
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
