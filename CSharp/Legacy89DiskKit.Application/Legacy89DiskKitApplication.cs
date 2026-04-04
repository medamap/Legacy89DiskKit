using Legacy89DiskKit.CharacterEncoding.Application;
using Legacy89DiskKit.Drive.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Registry;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.Fdc.Domain.Interface;
using Legacy89DiskKit.Timing.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;
using Legacy89DiskKit.DiskImage.Infrastructure.Factory;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider;
using Legacy89DiskKit.FileSystem.Infrastructure.Msx.Provider;
using Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Provider;
using Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider;

// Compatibility-only bootstrap surface.
// This namespace is intentionally preserved as the public facade for the responsibility-first core.
// It is not part of the responsibility-first namespace migration and exists solely for backward compatibility.
// Compatibility-only bootstrap surface.
// This namespace is intentionally preserved as the public facade for the responsibility-first core.
// It is not part of the responsibility-first namespace migration and exists solely for backward compatibility.
namespace Legacy89DiskKit.Application;

/// <summary>
/// Provides the supported managed bootstrap surface for Legacy89DiskKit.
/// </summary>
public static class Legacy89DiskKitApplication
{
    /// <summary>
    /// Creates a preconfigured disk service with the supported filesystem providers.
    /// </summary>
    public static Legacy89DiskKit.DiskImage.Application.DiskService CreateDiskService()
    {
        return new Legacy89DiskKit.DiskImage.Application.DiskService(fsRegistry: CreateFileSystemRegistry());
    }

    /// <summary>
    /// Creates a preconfigured file transfer service for the specified filesystem info.
    /// </summary>
    public static Legacy89DiskKit.FileSystem.Application.FileTransferService CreateFileTransferService(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var encoder = ResolveEncoder(fsInfo, encodingOverride);
        return new Legacy89DiskKit.FileSystem.Application.FileTransferService(encoder);
    }

    /// <summary>
    /// Creates the supported directory layout service.
    /// </summary>
    public static Legacy89DiskKit.FileSystem.Application.DirectoryLayoutService CreateDirectoryLayoutService()
    {
        return new Legacy89DiskKit.FileSystem.Application.DirectoryLayoutService();
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
        return new Legacy89DiskKit.FileSystem.Application.BootEntryExportService();
    }

    public static Legacy89DiskKit.FileSystem.Application.IBootEntryImportService CreateBootEntryImportService()
    {
        return new Legacy89DiskKit.FileSystem.Application.BootEntryImportService();
    }

    public static Legacy89DiskKit.FileSystem.Application.DiskInspectionService CreateDiskInspectionService()
    {
        return new Legacy89DiskKit.FileSystem.Application.DiskInspectionService();
    }

    public static Legacy89DiskKit.FileSystem.Application.FileInspectionService CreateFileInspectionService()
    {
        return new Legacy89DiskKit.FileSystem.Application.FileInspectionService();
    }

    public static Legacy89DiskKit.Drive.Application.DriveMountService CreateDriveMountService()
    {
        return new Legacy89DiskKit.Drive.Application.DriveMountService();
    }

    public static Legacy89DiskKit.Drive.Application.MountedMediumBindingService CreateMountedMediumBindingService()
    {
        return new Legacy89DiskKit.Drive.Application.MountedMediumBindingService();
    }

    public static Legacy89DiskKit.Fdc.Application.FdcAccessService CreateFdcAccessService(IFdcController controller, IControllerClock? clock = null)
    {
        return new Legacy89DiskKit.Fdc.Application.FdcAccessService(controller, clock);
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter CreateEventDrivenEmulatorFdcHostAdapter()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter(
            CreateDriveMountService(),
            CreateMountedMediumBindingService(),
            new DiskContainerFactory());
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostProtocolEndpoint CreateEmulatorHostProtocolEndpoint()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostProtocolEndpoint(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostProtocolTextSession CreateEmulatorHostProtocolTextSession()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostProtocolTextSession(CreateEmulatorHostProtocolEndpoint());
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostProtocolStdioRunner CreateEmulatorHostProtocolStdioRunner()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostProtocolStdioRunner(CreateEmulatorHostProtocolTextSession());
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostObservableProtocolSession CreateEmulatorHostObservableProtocolSession()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostObservableProtocolSession(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostObservableProtocolStdioRunner CreateEmulatorHostObservableProtocolStdioRunner()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostObservableProtocolStdioRunner(CreateEmulatorHostObservableProtocolSession());
    }

    public static IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostRequest> CreateReadOnlyD88PathScript(string imagePath, int driveNumber = 0)
    {
        return Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostRequestScriptFactory.CreateReadOnlyD88ByPathSequence(imagePath, driveNumber);
    }

    public static IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostRequest> CreateReadOnlyD88BufferScript(byte[] imageData, string imageFormat = "d88", int driveNumber = 0)
    {
        return Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostRequestScriptFactory.CreateReadOnlyD88ByBufferSequence(imageData, imageFormat, driveNumber);
    }

    public static IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostRequest> CreateReadOnlyRawBufferScript(byte[] imageData, string imageFormat = "2d", int driveNumber = 0)
    {
        return Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostRequestScriptFactory.CreateReadOnlyRawByBufferSequence(imageData, imageFormat, driveNumber);
    }

    public static async Task<Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostBundle> ReadEmulatorHostBundleAsync(
        string outputDirectory,
        string baseName,
        CancellationToken cancellationToken = default)
    {
        return await Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostBundleReader.ReadAsync(outputDirectory, baseName, cancellationToken);
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostProofReport BuildEmulatorHostProofReport(
        IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostTranscriptEntry> transcript,
        string openMode,
        string exchangeMode)
    {
        return Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostProofReportBuilder.Build(transcript, openMode, exchangeMode);
    }

    public static IReadOnlyList<string> CompareEmulatorHostBundle(
        Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostBundle bundle,
        Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostProofExpectation expectation)
    {
        return Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostBundleComparer.Compare(bundle, expectation);
    }

    public static IReadOnlyList<string> CompareEmulatorHostProofReport(
        Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostProofReport report,
        Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostProofExpectation expectation)
    {
        return Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostProofReportComparer.Compare(report, expectation);
    }

    public static async Task<IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostTranscriptEntry>> ReadEmulatorHostTranscriptAsync(
        string transcriptPath,
        CancellationToken cancellationToken = default)
    {
        return await Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostTranscriptFileStore.LoadAsync(transcriptPath, cancellationToken);
    }

    public static async Task<IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostRequest>> ReadEmulatorHostRequestScriptAsync(
        string requestScriptPath,
        CancellationToken cancellationToken = default)
    {
        return await Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostRequestScriptFileStore.LoadAsync(requestScriptPath, cancellationToken);
    }

    public static async Task WriteEmulatorHostBundleAsync(
        string outputDirectory,
        string baseName,
        Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostProofReport report,
        IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostTranscriptEntry> transcript,
        IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostRequest>? requestScript = null,
        CancellationToken cancellationToken = default)
    {
        await Legacy89DiskKit.Fdc.Application.Hosts.Scripting.EmulatorHostBundleWriter.WriteAsync(
            outputDirectory,
            baseName,
            report,
            transcript,
            requestScript,
            cancellationToken);
    }

    public static Legacy89DiskKit.Fdc.Application.Hosts.XmilWebStyleFdcHostAdapter CreateXmilWebStyleFdcHostAdapter()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.XmilWebStyleFdcHostAdapter(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    /// <summary>
    /// Creates the supported explicit filesystem resolver.
    /// </summary>
    public static Legacy89DiskKit.FileSystem.Application.ExplicitFileSystemResolver CreateExplicitFileSystemResolver()
    {
        return new Legacy89DiskKit.FileSystem.Application.ExplicitFileSystemResolver();
    }

    /// <summary>
    /// Creates the supported disk clone service.
    /// </summary>
    public static Legacy89DiskKit.FileSystem.Application.DiskCloneService CreateDiskCloneService(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var transferService = CreateFileTransferService(fsInfo, encodingOverride);
        var encoderRegistry = CreateEncoderRegistry();
        var normalizationService = new Legacy89DiskKit.FileSystem.Application.FileNameNormalizationService(encoderRegistry);
        return new Legacy89DiskKit.FileSystem.Application.DiskCloneService(transferService, normalizationService);
    }

    /// <summary>
    /// Creates the supported filesystem registry used by the managed public surface.
    /// </summary>
    public static IFileSystemRegistry CreateFileSystemRegistry()
    {
        var registry = new Legacy89DiskKit.FileSystem.Application.FileSystemRegistry();
        registry.Register(new XDosFileSystemProvider());
        registry.Register(new HuBasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Cpm.Provider.CpmFileSystemProvider());
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
