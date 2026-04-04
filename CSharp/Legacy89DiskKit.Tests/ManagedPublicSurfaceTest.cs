using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.Fdc.Application.Hosts.Scripting;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.Fdc.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Application;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Registry;
using Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class ManagedPublicSurfaceTest
{
    [Fact]
    public void CreateDiskService_ReturnsPreconfiguredService()
    {
        using var service = CreateDiskService();
        Assert.NotNull(service);
    }

    [Fact]
    public void CreateFileTransferService_UsesSupportedBootstrap()
    {
        var fsInfo = new DiskFileSystemInfo("Hu-BASIC", 1024000, 0, 256, 16, "X1");
        var service = CreateFileTransferService(fsInfo, "sjis");
        Assert.NotNull(service);
    }

    [Fact]
    public void CreateDirectoryLayoutService_ReturnsSupportedService()
    {
        var service = new DirectoryLayoutService();
        Assert.NotNull(service);
    }

    [Fact]
    public void CreateEmulatorHostProtocolStdioRunner_ReturnsSupportedRunner()
    {
        var runner = CreateEmulatorHostProtocolStdioRunner();
        Assert.NotNull(runner);
    }

    [Fact]
    public void CreateReadOnlyD88PathScript_ReturnsSupportedSequence()
    {
        var script = CreateReadOnlyD88PathScript("/tmp/example.d88");
        Assert.NotEmpty(script);
    }

    [Fact]
    public void CreateReadOnlyD88BufferScript_ReturnsSupportedSequence()
    {
        var script = CreateReadOnlyD88BufferScript([0x00, 0x01]);
        Assert.NotEmpty(script);
    }

    [Fact]
    public void BuildAndCompareEmulatorHostProofReport_UsesSupportedBootstrap()
    {
        var transcript = new[]
        {
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null,
                        Capabilities: new EmulatorHostCapabilities(1, true, true, true, true, true)),
                    [])),
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: 0x41,
                        VisibleState: new FdcVisibleState(0, 0, 1, 0x41, 0, 0, false, true, true),
                        IrqAsserted: true,
                        DrqAsserted: true,
                        PendingAdvanceMicroseconds: null),
                    [])),
        };

        var report = BuildEmulatorHostProofReport(transcript, "OpenDiskPath", "observable");
        var mismatches = CompareEmulatorHostProofReport(
            report,
            EmulatorHostProofExpectationCatalog.EventDrivenSecondProofRaw());

        Assert.NotNull(report);
        Assert.NotNull(mismatches);
    }

    [Fact]
    public void ManagedBootstrap_CanOpenAndListKnownSample()
    {
        var imagePath = TestDiskFixtureFactory.CreateFormattedHuBasicDisk(
            "ManagedBootstrap_CanOpenAndListKnownSample.d88",
            writeSampleFile: true);

        using var service = CreateDiskService();
        service.OpenDisk(imagePath);
        var fileSystem = Assert.IsAssignableFrom<Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem.IFileSystem>(service.FileSystem);
        var files = fileSystem.GetFiles().ToList();
        Assert.NotEmpty(files);
    }

    [Fact]
    public void ManagedBootstrap_CanOpenKnownSampleFromBufferWithExplicitFormat()
    {
        using var service = CreateDiskService();
        var imagePath = TestDiskFixtureFactory.CreateFormattedHuBasicDisk(
            "ManagedBootstrap_CanOpenKnownSampleFromBufferWithExplicitFormat.d88",
            writeSampleFile: true);
        var imageData = File.ReadAllBytes(imagePath);

        service.OpenDisk(imageData, "d88");

        var fileSystem = Assert.IsAssignableFrom<Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem.IFileSystem>(service.FileSystem);
        var files = fileSystem.GetFiles().ToList();
        Assert.NotEmpty(files);
    }

    [Fact]
    public void ManagedBootstrap_CanCreateAndFormatHuBasicDisk()
    {
        var imagePath = Path.Combine(Path.GetTempPath(), $"ldk-managed-{Guid.NewGuid():N}.d88");
        try
        {
            using var service = CreateDiskService();
            service.CreateDisk(imagePath, Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD, "WORKDISK");

            var resolver = new ExplicitFileSystemResolver();
            var container = service.OpenDisk(imagePath, readOnly: false);
            using var fileSystem = resolver.Create("hu-basic", container);
            fileSystem.Format();
            resolver.InitializeForDetection(fileSystem);

            using var verify = CreateDiskService();
            verify.OpenDisk(imagePath);
            Assert.NotNull(verify.FileSystem);
        }
        finally
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }

    [Fact]
    public void ManagedBootstrap_CanExportAndValidateLayout()
    {
        var imagePath = TestDiskFixtureFactory.CreateFormattedHuBasicDisk(
            "ManagedBootstrap_CanExportAndValidateLayout.d88",
            writeSampleFile: true);

        using var service = CreateDiskService();
        var container = service.OpenDisk(imagePath);
        var resolver = new ExplicitFileSystemResolver();
        using var fileSystem = resolver.Create("hu-basic", container);

        var layoutService = new DirectoryLayoutService();
        var plan = layoutService.ExportPlan(fileSystem);
        var validation = layoutService.ValidatePlan(fileSystem, plan);

        Assert.True(validation.IsValid);
    }

    private static DiskService CreateDiskService()
    {
        return new DiskService(fsRegistry: CreateFileSystemRegistry());
    }

    private static FileTransferService CreateFileTransferService(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var encoder = ResolveEncoder(fsInfo, encodingOverride);
        return new FileTransferService(encoder);
    }

    private static EmulatorHostProtocolStdioRunner CreateEmulatorHostProtocolStdioRunner()
    {
        return new EmulatorHostProtocolStdioRunner(CreateEmulatorHostProtocolTextSession());
    }

    private static EmulatorHostProtocolTextSession CreateEmulatorHostProtocolTextSession()
    {
        return new EmulatorHostProtocolTextSession(CreateEmulatorHostProtocolEndpoint());
    }

    private static EmulatorHostProtocolEndpoint CreateEmulatorHostProtocolEndpoint()
    {
        return new EmulatorHostProtocolEndpoint(CreateEventDrivenEmulatorFdcHostAdapter());
    }

    private static Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter CreateEventDrivenEmulatorFdcHostAdapter()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter(
            new Legacy89DiskKit.Drive.Application.DriveMountService(),
            new Legacy89DiskKit.Drive.Application.MountedMediumBindingService(),
            new Legacy89DiskKit.DiskImage.Infrastructure.Factory.DiskContainerFactory());
    }

    private static IReadOnlyList<EmulatorHostRequest> CreateReadOnlyD88PathScript(string imagePath, int driveNumber = 0)
    {
        return EmulatorHostRequestScriptFactory.CreateReadOnlyD88ByPathSequence(imagePath, driveNumber);
    }

    private static IReadOnlyList<EmulatorHostRequest> CreateReadOnlyD88BufferScript(byte[] imageData, string imageFormat = "d88", int driveNumber = 0)
    {
        return EmulatorHostRequestScriptFactory.CreateReadOnlyD88ByBufferSequence(imageData, imageFormat, driveNumber);
    }

    private static EmulatorHostProofReport BuildEmulatorHostProofReport(IReadOnlyList<EmulatorHostTranscriptEntry> transcript, string openMode, string exchangeMode)
    {
        return EmulatorHostProofReportBuilder.Build(transcript, openMode, exchangeMode);
    }

    private static IReadOnlyList<string> CompareEmulatorHostProofReport(EmulatorHostProofReport report, EmulatorHostProofExpectation expectation)
    {
        return EmulatorHostProofReportComparer.Compare(report, expectation);
    }

    private static IFileSystemRegistry CreateFileSystemRegistry()
    {
        var registry = new FileSystemRegistry();
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider.XDosFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider.HuBasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Cpm.Provider.CpmFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Provider.N88BasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Msx.Provider.MsxDosFileSystemProvider());
        return registry;
    }

    private static ICharacterEncoder ResolveEncoder(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var registry = CreateEncoderRegistry();
        return new CharacterEncodingResolver(registry).ResolveEncoder(fsInfo, encodingOverride);
    }

    private static IEncoderRegistry CreateEncoderRegistry()
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
}
