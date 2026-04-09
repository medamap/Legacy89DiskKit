using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using Legacy89DiskKit.Cli;
using Legacy89DiskKit.Cli.Logging;
using Legacy89DiskKit.CharacterEncoding.Application;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Archive.Application;
using Legacy89DiskKit.Fdc.Application.Hosts.Scripting;
using Legacy89DiskKit.Cli.Presentation.FileSystem;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
CliLogSystem? _logSystem = null;
string? _logPath = null;
var requestedLanguage = TryGetRequestedLanguage(args);
if (requestedLanguage is { } languageCode && languageCode is not ("ja" or "en"))
{
    Console.Error.WriteLine("Unsupported language. Use 'ja' or 'en'.");
    return 1;
}

var logPath = ExtractLogPath(args);
if (logPath is not null || args.Contains("--log"))
{
    _logSystem = CliLogSystem.CreateWithConsoleAndFileLogger(logPath);
    _logPath = logPath;
}

var effectiveArgs = RewriteVersionArgs(RewriteFullHelpArgs(RewriteUpdateCheckArgs(RewriteImplicitInspectorArgs(RewriteLegacyArgs(args)))));
var localizer = FileListLocalizer.Create(requestedLanguage);
var archiveService = new ArchiveService();
var huBasicMetadataService = new HuBasicMetadataService();
var directoryLayoutService = new DirectoryLayoutService();
var explicitFileSystemResolver = new ExplicitFileSystemResolver();
var bootProfileService = Legacy89DiskKitApplication.CreateBootProfileService();
var diskInspectionService = Legacy89DiskKitApplication.CreateDiskInspectionService();
var fileInspectionService = Legacy89DiskKitApplication.CreateFileInspectionService();
var rootCommand = new RootCommand(localizer.RootDescription);
var languageOption = new Option<string?>(new[] { "--language", "-l" }, localizer.LanguageOptionDescription);
var encodingOption = new Option<string?>(new[] { "--encoding", "-e" }, localizer.EncodingOptionDescription);
var nativeOption = new Option<bool>(new[] { "--native" }, "Use C++ native implementation via NativeBridge");
var checkUpdateOption = new Option<bool>("--check-update", localizer.CheckUpdateCommandDescription);
var fullHelpOption = new Option<bool>("--full-help", localizer.FullHelpOptionDescription);
var outputFormatOption = new Option<string>("--output-format", () => "table", localizer.OutputFormatOptionDescription);
var logOption = new Option<string?>(new[] { "--log" }, localizer.LogOptionDescription)
{
    Arity = ArgumentArity.ZeroOrOne
};
rootCommand.AddGlobalOption(languageOption);
rootCommand.AddGlobalOption(encodingOption);
rootCommand.AddGlobalOption(nativeOption);
rootCommand.AddGlobalOption(checkUpdateOption);
rootCommand.AddGlobalOption(fullHelpOption);
rootCommand.AddGlobalOption(outputFormatOption);
rootCommand.AddGlobalOption(logOption);
// Initialize backend based on command line args
if (effectiveArgs.Contains("--native"))
{
    try
    {
        NativeBridgeBackend.SetCurrent(new Legacy89DiskKit.NativeInterop.Core.CppLibraryNativeBridgeBackend());
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Warning: Failed to initialize native backend: {ex.Message}");
        Console.Error.WriteLine("Falling back to managed implementation.");
        NativeBridgeBackend.SetCurrent(new Legacy89DiskKit.Native.Application.ManagedNativeBridgeBackend());
    }
}
else
{
    NativeBridgeBackend.SetCurrent(new Legacy89DiskKit.Native.Application.ManagedNativeBridgeBackend());
}

var imageArgument = new Argument<string>("image", localizer.ImageArgumentDescription);
var explicitFileSystemOption = new Option<string?>(new[] { "--file-system", "-f" }, localizer.ExplicitFileSystemOptionDescription);
var fullHelpCommand = new Command("full-help", localizer.FullHelpCommandDescription);
fullHelpCommand.SetHandler(() =>
{
    PrintFullHelp(rootCommand, localizer);
});
var versionCommand = new Command("version", localizer.VersionCommandDescription);
versionCommand.SetHandler(() =>
{
    Console.WriteLine(VersionDisplay.GetDisplayVersion());
});
var listCommand = new Command("list", localizer.ListCommandDescription);
listCommand.AddArgument(imageArgument);
listCommand.AddOption(explicitFileSystemOption);
listCommand.SetHandler((string imagePath, string? fileSystemName, string? encodingOverride, string outputFormat) =>
{
    Console.WriteLine($"{localizer.ListingFilesForMessage}: {imagePath}");
    try
    {
        using var diskService = CreateDiskService();
        var container = diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(ResolveFileSystem(diskService, container, fileSystemName, explicitFileSystemResolver), localizer);
        if (fs == null)
        {
            return;
        }

        var(fsInfo, entries) = BuildFileListEntries(fs, archiveService, encodingOverride);
        var encodingId = encodingOverride ?? fsInfo.DefaultEncodingId;
        Console.WriteLine($"{localizer.UsingEncodingMessage}: {encodingId} (FS Default: {fsInfo.DefaultEncodingId})");
        var formatter = FileListFormatterFactory.Create(fsInfo.FileSystemName);
        var bootRecordInfo = huBasicMetadataService.GetBootRecordInfo(fs);
        var bootSummary = bootProfileService.GetBootProfile(fs);
        var view = formatter.Format(new FileListFormatContext(fsInfo, entries, bootRecordInfo, bootSummary), localizer);
        RenderFileList(view, localizer, outputFormat);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, explicitFileSystemOption, encodingOption, outputFormatOption);
var fileCommand = new Command("file", localizer.FileCommandDescription);
var fileInspectorDetailOption = new Option<string>("--detail", () => "normal", localizer.FileInspectorDetailOptionDescription);
var diskFileArgument = new Argument<string>("disk-file", localizer.DiskFileArgumentDescription);
var hostFileArgument = new Argument<string>("host-file", localizer.HostFileArgumentDescription);
var hostPathArgument = new Argument<string>("host-path", localizer.HostPathArgumentDescription);
var sourceImageArgument = new Argument<string>("src-image", localizer.SourceImageArgumentDescription);
var destImageArgument = new Argument<string>("dest-image", localizer.DestinationImageArgumentDescription);
var filesArgument = new Argument<string[]>("files", () => new[] { "all" }, localizer.FileCrossCopyFilesArgumentDescription);
var sourceNameArgument = new Argument<string>("source", localizer.SourceNameArgumentDescription);
var targetNameArgument = new Argument<string>("target", localizer.TargetNameArgumentDescription);
var newNameArgument = new Argument<string>("new-name", localizer.NewNameArgumentDescription);
var targetFileNameOption = new Option<string?>(new[] { "--target-name", "-n" }, localizer.TargetFileNameOptionDescription);
var imageFileOverwriteOption = new Option<bool>("--image-file-overwrite", localizer.ImageFileOverwriteOptionDescription);
var tabModeOption = new Option<string>("--tab-mode", () => "keep", localizer.TabModeOptionDescription);
var tabWidthOption = new Option<int>("--tab-width", () => 4, localizer.TabWidthOptionDescription);
var truncateTextOnOverflowOption = new Option<bool>("--truncate-text-on-overflow", localizer.TruncateTextOnOverflowOptionDescription);
var fileExtractCommand = new Command("extract", localizer.FileExtractCommandDescription);
fileExtractCommand.AddAlias("export");
fileExtractCommand.AddArgument(imageArgument);
fileExtractCommand.AddArgument(diskFileArgument);
fileExtractCommand.AddArgument(hostPathArgument);
fileExtractCommand.AddOption(explicitFileSystemOption);
fileExtractCommand.AddOption(tabModeOption);
fileExtractCommand.AddOption(tabWidthOption);
fileExtractCommand.SetHandler((string imagePath, string diskFileName, string hostPath, string? fileSystemName, string tabMode, int tabWidth, string? encodingOverride) =>
{
    try
    {
        using var diskService = CreateDiskService();
        var container = diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(ResolveFileSystem(diskService, container, fileSystemName, explicitFileSystemResolver), localizer);
        if (fs == null)
        {
            return;
        }

        CreateFileTransferService(fs.GetFileSystemInfo(), encodingOverride).ExportFile(fs, diskFileName, hostPath, new TextTransferOptions(TabMode: tabMode, TabWidth: tabWidth));
        PrintSuccess(localizer, localizer.FileExtractedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, diskFileArgument, hostPathArgument, explicitFileSystemOption, tabModeOption, tabWidthOption, encodingOption);
var fileInjectCommand = new Command("inject", localizer.FileInjectCommandDescription);
fileInjectCommand.AddAlias("import");
fileInjectCommand.AddArgument(imageArgument);
fileInjectCommand.AddArgument(hostFileArgument);
fileInjectCommand.AddOption(targetFileNameOption);
fileInjectCommand.AddOption(explicitFileSystemOption);
fileInjectCommand.AddOption(tabModeOption);
fileInjectCommand.AddOption(tabWidthOption);
fileInjectCommand.AddOption(truncateTextOnOverflowOption);
fileInjectCommand.AddOption(imageFileOverwriteOption);
fileInjectCommand.SetHandler((string imagePath, string hostFilePath, string? targetName, string? fileSystemName, string tabMode, int tabWidth, bool truncateTextOnOverflow) =>
{
    var imageFileOverwrite = args.Contains("--image-file-overwrite");
    var encodingOverride = TryGetRequestedEncoding(args);

    try
    {
        RejectWriteToMultiSlotD88(imagePath, localizer);
        using var diskService = CreateDiskService();
        var container = OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(ResolveFileSystem(diskService, container, fileSystemName, explicitFileSystemResolver), localizer);
        if (fs == null)
        {
            return;
        }

        var fsInfo = fs.GetFileSystemInfo();
        var encodingId = fsInfo.DefaultEncodingId;
        var existingNames = new HashSet<string>(fs.GetFiles().Select(f => f.FullName.ToUpperInvariant()));
        var sourceName = targetName ?? Path.GetFileName(hostFilePath);
        var canOverwriteExactName = imageFileOverwrite;
        if (canOverwriteExactName && fs.FileExists(sourceName))
        {
            if (TryDeleteExistingFile(fs, sourceName))
            {
                existingNames.Remove(sourceName.ToUpperInvariant());
            }
            else
            {
                canOverwriteExactName = false;
                _logSystem?.Warning(localizer.ImageFileOverwriteIgnoredWarning, "file-inject");
            }
        }

        var normalizationService = new FileNameNormalizationService(archiveService.EncoderRegistry);
        var (normalizedName, wasOverwritten) = ResolveImageFileTargetName(
            sourceName, encodingId, fsInfo, existingNames, normalizationService, canOverwriteExactName, _logSystem);

        if (wasOverwritten)
        {
            _logSystem?.Info($"Overwriting existing file: {normalizedName}", "file-inject");
        }

        var data = File.ReadAllBytes(hostFilePath);
        var isAscii = IsLikelyAsciiPayload(data);
        if (isAscii)
        {
            CreateFileTransferService(fsInfo, encodingOverride).ImportFile(fs, hostFilePath, normalizedName, true, new TextTransferOptions(TabMode: tabMode, TabWidth: tabWidth, TruncateOnOverflow: truncateTextOnOverflow));
        }
        else
        {
            var attributes = fs.CreateDefaultAttributes(false);
            fs.WriteFile(normalizedName, data, attributes);
        }

        diskService.Session?.Save();
        PrintSuccess(localizer, localizer.FileInjectedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, hostFileArgument, targetFileNameOption, explicitFileSystemOption, tabModeOption, tabWidthOption, truncateTextOnOverflowOption);
var fileDeleteCommand = new Command("delete", localizer.FileDeleteCommandDescription);
fileDeleteCommand.AddArgument(imageArgument);
fileDeleteCommand.AddArgument(diskFileArgument);
fileDeleteCommand.SetHandler((string imagePath, string diskFileName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        fs.DeleteFile(diskFileName);
        PrintSuccess(localizer, localizer.FileDeletedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, diskFileArgument);
var fileRenameCommand = new Command("rename", localizer.FileRenameCommandDescription);
fileRenameCommand.AddArgument(imageArgument);
fileRenameCommand.AddArgument(sourceNameArgument);
fileRenameCommand.AddArgument(newNameArgument);
fileRenameCommand.SetHandler((string imagePath, string sourceName, string newName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        fs.RenameFile(sourceName, newName);
        PrintSuccess(localizer, localizer.FileRenamedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, sourceNameArgument, newNameArgument);
var fileCopyCommand = new Command("copy", localizer.FileCopyCommandDescription);
fileCopyCommand.AddArgument(imageArgument);
fileCopyCommand.AddArgument(sourceNameArgument);
fileCopyCommand.AddArgument(targetNameArgument);
fileCopyCommand.AddOption(imageFileOverwriteOption);
fileCopyCommand.SetHandler((string imagePath, string sourceName, string targetName, bool imageFileOverwrite) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        if (imageFileOverwrite && sourceName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
        {
            PrintSuccess(localizer, localizer.FileCopiedMessage);
            return;
        }

        var fsInfo = fs.GetFileSystemInfo();
        var existingNames = new HashSet<string>(fs.GetFiles().Select(f => f.FullName.ToUpperInvariant()));
        var canOverwriteExactName = imageFileOverwrite;
        if (canOverwriteExactName && fs.FileExists(targetName))
        {
            if (TryDeleteExistingFile(fs, targetName))
            {
                existingNames.Remove(targetName.ToUpperInvariant());
            }
            else
            {
                canOverwriteExactName = false;
                _logSystem?.Warning(localizer.ImageFileOverwriteIgnoredWarning, "file-copy");
            }
        }

        var normalizationService = new FileNameNormalizationService(archiveService.EncoderRegistry);
        var (normalizedTargetName, wasOverwritten) = ResolveImageFileTargetName(
            targetName, fsInfo.DefaultEncodingId, fsInfo, existingNames, normalizationService, canOverwriteExactName, _logSystem);

        if (wasOverwritten)
        {
            _logSystem?.Info($"Overwriting existing file: {normalizedTargetName}", "file-copy");
        }

        fs.CopyFile(sourceName, normalizedTargetName);
        PrintSuccess(localizer, localizer.FileCopiedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, sourceNameArgument, targetNameArgument, imageFileOverwriteOption);
var fileCrossCopyCommand = new Command("cross-copy", localizer.FileCrossCopyCommandDescription);
fileCrossCopyCommand.AddArgument(sourceImageArgument);
fileCrossCopyCommand.AddArgument(destImageArgument);
fileCrossCopyCommand.AddArgument(filesArgument);
fileCrossCopyCommand.AddOption(imageFileOverwriteOption);
fileCrossCopyCommand.AddOption(encodingOption);
fileCrossCopyCommand.SetHandler((string srcPath, string destPath, string[] files, bool imageFileOverwrite, string? encodingOverride) =>
{
    try
    {
        using var srcDisk = CreateDiskService();
        srcDisk.OpenDisk(srcPath, true);
        var srcFs = RequireFileSystem(srcDisk.FileSystem, localizer);
        if (srcFs == null)
            return;
        using var destDisk = CreateDiskService();
        OpenWritableDisk(destDisk, destPath, localizer);
        var destFs = RequireFileSystem(destDisk.FileSystem, localizer);
        if (destFs == null)
            return;
        IEnumerable<string> targetFiles = files;
        if (files.Length == 1 && files[0].Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            targetFiles = srcFs.GetFiles().Select(f => f.FullName);
        }
        else if (files.Length == 1 && files[0].Contains(','))
        {
            targetFiles = files[0].Split(',');
        }

        var srcAdapter = CreateTransferAdapter(srcFs);
        var destAdapter = CreateTransferAdapter(destFs);
        if (srcAdapter is null)
        {
            throw new InvalidOperationException("Source file system does not support file transfer.");
        }

        if (destAdapter is null)
        {
            throw new InvalidOperationException("Destination file system does not support file transfer.");
        }

        var normalizationService = new FileNameNormalizationService(archiveService.EncoderRegistry);
        var destInfo = destFs.GetFileSystemInfo();
        var existingNames = new HashSet<string>(destFs.GetFiles().Select(f => f.FullName.ToUpperInvariant()));
        var encodingId = encodingOverride ?? destInfo.DefaultEncodingId;

        foreach (var fileName in targetFiles)
        {
            var sourceEntry = srcFs.GetFiles().FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (sourceEntry == null)
            {
                throw new FileNotFoundException($"Source file not found: {fileName}");
            }

            var envelope = srcAdapter.Export(sourceEntry);
            var targetName = envelope.FileName;
            var canOverwriteExactName = imageFileOverwrite;
            if (canOverwriteExactName && destFs.FileExists(targetName))
            {
                if (TryDeleteExistingFile(destFs, targetName))
                {
                    existingNames.Remove(targetName.ToUpperInvariant());
                }
                else
                {
                    canOverwriteExactName = false;
                    _logSystem?.Warning(localizer.ImageFileOverwriteIgnoredWarning, "file-cross-copy");
                }
            }

            var (resolvedTargetName, wasOverwritten) = ResolveImageFileTargetName(
                targetName, encodingId, destInfo, existingNames, normalizationService, canOverwriteExactName, _logSystem);

            if (wasOverwritten)
            {
                _logSystem?.Info($"Overwriting existing file: {resolvedTargetName}", "file-cross-copy");
            }

            destAdapter.Import(envelope, resolvedTargetName);
            existingNames.Add(resolvedTargetName.ToUpperInvariant());
        }

        PrintSuccess(localizer, localizer.FileCopiedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, sourceImageArgument, destImageArgument, filesArgument, imageFileOverwriteOption, encodingOption);
var fileInspectorCommand = new Command("inspector", localizer.FileInspectorCommandDescription);
fileInspectorCommand.AddArgument(imageArgument);
fileInspectorCommand.AddArgument(diskFileArgument);
fileInspectorCommand.AddOption(fileInspectorDetailOption);
fileInspectorCommand.AddOption(outputFormatOption);
fileInspectorCommand.AddOption(explicitFileSystemOption);
fileInspectorCommand.AddOption(encodingOption);
fileInspectorCommand.SetHandler((string imagePath, string diskFileName, string detail, string outputFormat, string? fileSystemName, string? encodingOverride) =>
{
    try
    {
        PrintFileInspector(imagePath, diskFileName, detail, outputFormat, fileSystemName, encodingOverride, localizer, archiveService, fileInspectionService, explicitFileSystemResolver);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, diskFileArgument, fileInspectorDetailOption, outputFormatOption, explicitFileSystemOption, encodingOption);
fileCommand.AddCommand(fileExtractCommand);
fileCommand.AddCommand(fileInjectCommand);
fileCommand.AddCommand(fileDeleteCommand);
fileCommand.AddCommand(fileRenameCommand);
fileCommand.AddCommand(fileCopyCommand);
fileCommand.AddCommand(fileCrossCopyCommand);
fileCommand.AddCommand(fileInspectorCommand);
var diskCommand = new Command("disk", localizer.DiskCommandDescription);
var diskInspectorCommand = new Command("inspector", localizer.DiskInspectorCommandDescription);
var diskInspectorDetailOption = new Option<string>("--detail", () => "short", localizer.DiskInspectorDetailOptionDescription);
diskInspectorCommand.AddArgument(imageArgument);
diskInspectorCommand.AddOption(diskInspectorDetailOption);
diskInspectorCommand.AddOption(outputFormatOption);
diskInspectorCommand.AddOption(explicitFileSystemOption);
diskInspectorCommand.SetHandler((string imagePath, string detail, string outputFormat, string? fileSystemName, string? encodingOverride) =>
{
    try
    {
        PrintInspector(imagePath, detail, outputFormat, fileSystemName, encodingOverride, localizer, diskInspectionService, archiveService, huBasicMetadataService, bootProfileService, explicitFileSystemResolver);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, diskInspectorDetailOption, outputFormatOption, explicitFileSystemOption, encodingOption);
var diskCreateCommand = new Command("create", localizer.DiskCreateCommandDescription);
var diskCreateImageFormatOption = new Option<string?>(new[] { "--image-format", "-i" }, localizer.DiskCreateImageFormatOptionDescription);
var diskTypeOption = new Option<string>(new[] { "--disk-type", "-d" }, () => "2d", localizer.DiskCreateDiskTypeOptionDescription);
var diskFileSystemOption = new Option<string?>(new[] { "--file-system", "-f" }, localizer.DiskCreateFileSystemOptionDescription);
var diskNameOption = new Option<string?>(new[] { "--name", "-n" }, localizer.DiskCreateNameOptionDescription);
diskCreateCommand.AddArgument(imageArgument);
diskCreateCommand.AddOption(diskCreateImageFormatOption);
diskCreateCommand.AddOption(diskTypeOption);
diskCreateCommand.AddOption(diskFileSystemOption);
diskCreateCommand.AddOption(diskNameOption);
diskCreateCommand.SetHandler((string imagePath, string? imageFormatName, string diskTypeName, string? fileSystemName, string? diskName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        var diskType = ParseDiskType(diskTypeName);
        var resolvedImagePath = ResolveCreatePath(imagePath, imageFormatName);
        var container = diskService.CreateDisk(resolvedImagePath, diskType, diskName ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(fileSystemName))
        {
            using var fs = explicitFileSystemResolver.Create(fileSystemName, container);
            fs.Format();
            explicitFileSystemResolver.InitializeForDetection(fs);
        }

        PrintSuccess(localizer, localizer.DiskCreatedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, diskCreateImageFormatOption, diskTypeOption, diskFileSystemOption, diskNameOption);
var diskFormatCommand = new Command("format", localizer.DiskFormatCommandDescription);
var explicitFormatFsOption = new Option<string?>(new[] { "--file-system", "-f" }, localizer.DiskFormatFsOptionDescription);
diskFormatCommand.AddArgument(imageArgument);
diskFormatCommand.AddOption(explicitFormatFsOption);
diskFormatCommand.SetHandler((string imagePath, string? explicitFileSystemName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        var container = OpenWritableDisk(diskService, imagePath, localizer);
        if (!string.IsNullOrWhiteSpace(explicitFileSystemName))
        {
            using var explicitFs = explicitFileSystemResolver.Create(explicitFileSystemName, container);
            explicitFs.Format();
            explicitFileSystemResolver.InitializeForDetection(explicitFs);
            PrintSuccess(localizer, localizer.DiskFormattedMessage);
            return;
        }

        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        fs.Format();
        PrintSuccess(localizer, localizer.DiskFormattedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, explicitFormatFsOption);
diskCommand.AddCommand(diskInspectorCommand);
diskCommand.AddCommand(diskCreateCommand);
diskCommand.AddCommand(diskFormatCommand);
var diskSectorCopyCommand = new Command("sector-copy", localizer.DiskSectorCopyCommandDescription);
diskSectorCopyCommand.AddArgument(sourceImageArgument);
diskSectorCopyCommand.AddArgument(destImageArgument);
var forceOption = new Option<bool>(new[] { "--force", "-f" }, localizer.DiskSectorCopyForceOptionDescription);
diskSectorCopyCommand.AddOption(forceOption);
diskSectorCopyCommand.SetHandler((string srcPath, string destPath, bool force) =>
{
    try
    {
        if (!force && File.Exists(destPath))
        {
            if (!ConfirmOverwrite(localizer, destPath))
                return;
        }

        using var diskService = CreateDiskService();
        using var srcDisk = diskService.OpenDisk(srcPath, true);
        IDiskContainer destDisk;
        if (File.Exists(destPath))
        {
            destDisk = OpenWritableDisk(diskService, destPath, localizer);
        }
        else
        {
            destDisk = diskService.CreateDisk(destPath, srcDisk.DiskType, "");
        }

        using (destDisk)
        {
            var cloneService = new DiskCloneService(null !, null !);
            var result = cloneService.CopySectors(srcDisk, destDisk);
            destDisk.Save();
            PrintSuccess(localizer, string.Format(localizer.DiskSectorCopiedMessage, result.tracksCopied, result.sectorsSkipped));
        }
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, sourceImageArgument, destImageArgument, forceOption);
diskCommand.AddCommand(diskSectorCopyCommand);
var sectorCommand = new Command("sector", localizer.SectorCommandDescription);
var sectorLocationArgument = new Argument<int>("sector", localizer.SectorLocationArgumentDescription);
var sectorCountArgument = new Argument<int>("count", localizer.SectorCountArgumentDescription);
var sectorExportCommand = new Command("export", localizer.SectorExportCommandDescription);
sectorExportCommand.AddArgument(imageArgument);
sectorExportCommand.AddArgument(sectorLocationArgument);
sectorExportCommand.AddArgument(sectorCountArgument);
sectorExportCommand.AddArgument(hostPathArgument);
sectorExportCommand.SetHandler((string imagePath, int sector, int count, string hostPath) =>
{
    try
    {
        using var diskService = CreateDiskService();
        using var container = diskService.OpenDisk(imagePath, true);
        var payload = ReadLinearSectors(container, sector, count);
        File.WriteAllBytes(hostPath, payload);
        PrintSuccess(localizer, localizer.FileExtractedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, sectorLocationArgument, sectorCountArgument, hostPathArgument);
var sectorImportCountOption = new Option<int?>(new[] { "--count", "-c" }, "Number of sectors to write. If omitted, infer from file length.");
var sectorImportCommand = new Command("import", localizer.SectorImportCommandDescription);
sectorImportCommand.AddArgument(imageArgument);
sectorImportCommand.AddArgument(sectorLocationArgument);
sectorImportCommand.AddArgument(hostFileArgument);
sectorImportCommand.AddOption(sectorImportCountOption);
sectorImportCommand.SetHandler((string imagePath, int sector, string hostFilePath, int? count) =>
{
    try
    {
        var data = File.ReadAllBytes(hostFilePath);
        using var diskService = CreateDiskService();
        using var container = OpenWritableDisk(diskService, imagePath, localizer);
        WriteLinearSectors(container, sector, data, count);
        container.Save();
        PrintSuccess(localizer, localizer.FileInjectedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, sectorLocationArgument, hostFileArgument, sectorImportCountOption);
sectorCommand.AddCommand(sectorExportCommand);
sectorCommand.AddCommand(sectorImportCommand);
diskCommand.AddCommand(sectorCommand);
var dumpLocationArgument = new Argument<string>("location", localizer.DumpLocationArgumentDescription);
var dumpLengthArgument = new Argument<string>("length", localizer.DumpLengthArgumentDescription);
var diskDumpCommand = new Command("dump", localizer.DiskDumpCommandDescription);
diskDumpCommand.AddArgument(imageArgument);
diskDumpCommand.AddArgument(dumpLocationArgument);
diskDumpCommand.AddArgument(dumpLengthArgument);
diskDumpCommand.SetHandler((string imagePath, string location, string length) =>
{
    try
    {
        var bytes = ReadDumpBytes(imagePath, location, length);
        PrintHexDump(bytes);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, dumpLocationArgument, dumpLengthArgument);
diskCommand.AddCommand(diskDumpCommand);
var diskBootCopyCommand = new Command("boot-copy", "Copy boot area (IPL) from source disk to destination disk");
diskBootCopyCommand.AddArgument(sourceImageArgument);
diskBootCopyCommand.AddArgument(destImageArgument);
diskBootCopyCommand.AddOption(forceOption);
diskBootCopyCommand.SetHandler((string srcPath, string destPath, bool force) =>
{
    try
    {
        if (!force && File.Exists(destPath))
        {
            if (!ConfirmOverwrite(localizer, destPath))
                return;
        }

        using var srcDiskService = CreateDiskService();
        using var srcDisk = srcDiskService.OpenDisk(srcPath, true);
        var srcFs = RequireFileSystem(srcDiskService.FileSystem, localizer);
        if (srcFs == null)
            return;
        using var destDiskService = CreateDiskService();
        IDiskContainer destDisk;
        IFileSystem? destFs;
        if (File.Exists(destPath))
        {
            destDisk = OpenWritableDisk(destDiskService, destPath, localizer);
            destFs = RequireFileSystem(destDiskService.FileSystem, localizer);
            if (destFs == null)
                return;
        }
        else
        {
            destDisk = destDiskService.CreateDisk(destPath, srcDisk.DiskType, "");
            var srcFsName = srcFs.GetFileSystemInfo().FileSystemName;
            destFs = explicitFileSystemResolver.Create(srcFsName, destDisk);
            destFs.Format();
        }

        using (destDisk)
        {
            var cloneService = new DiskCloneService(null !, null !);
            cloneService.TransferBootArea(srcFs, destFs);
            destDisk.Save();
            PrintSuccess(localizer, "Boot area copied successfully.");
        }
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, sourceImageArgument, destImageArgument, forceOption);
diskCommand.AddCommand(diskBootCopyCommand);
var hostCommand = new Command("host", localizer.HostCommandDescription);
var hostStdioCommand = new Command("stdio", localizer.HostStdioCommandDescription);
var hostObservableOption = new Option<bool>("--observable", localizer.HostObservableOptionDescription);
var hostScriptCommand = new Command("script", localizer.HostScriptCommandDescription);
var hostScriptD88PathCommand = new Command("d88-path", localizer.HostScriptD88PathCommandDescription);
var hostScriptD88BufferCommand = new Command("d88-buffer", localizer.HostScriptD88BufferCommandDescription);
var hostScriptRawBufferCommand = new Command("raw-buffer", localizer.HostScriptRawBufferCommandDescription);
var hostScriptInspectCommand = new Command("inspect", localizer.HostScriptInspectCommandDescription);
var hostBundleCommand = new Command("bundle", localizer.HostBundleCommandDescription);
var hostBundleInspectCommand = new Command("inspect", localizer.HostBundleInspectCommandDescription);
var hostBundleVerifyCommand = new Command("verify", localizer.HostBundleVerifyCommandDescription);
var hostBundlePackCommand = new Command("pack", localizer.HostBundlePackCommandDescription);
var hostTranscriptCommand = new Command("transcript", localizer.HostTranscriptCommandDescription);
var hostTranscriptInspectCommand = new Command("inspect", localizer.HostTranscriptInspectCommandDescription);
var hostTranscriptReportCommand = new Command("report", localizer.HostTranscriptReportCommandDescription);
var hostTranscriptVerifyCommand = new Command("verify", localizer.HostTranscriptVerifyCommandDescription);
var hostOutputArgument = new Argument<string>("output", localizer.HostOutputArgumentDescription);
var hostDirectoryArgument = new Argument<string>("directory", localizer.HostDirectoryArgumentDescription);
var hostBaseNameArgument = new Argument<string>("base-name", localizer.HostBaseNameArgumentDescription);
var hostBaselineArgument = new Argument<string>("baseline", localizer.HostBaselineArgumentDescription);
var hostTranscriptArgument = new Argument<string>("transcript", localizer.HostTranscriptArgumentDescription);
var hostRequestScriptOption = new Option<string?>("--request-script", localizer.HostRequestScriptOptionDescription);
var hostOpenModeOption = new Option<string>("--open-mode", () => "OpenDiskPath", localizer.HostOpenModeOptionDescription);
var hostExchangeModeOption = new Option<string>("--exchange-mode", () => "observable", localizer.HostExchangeModeOptionDescription);
hostStdioCommand.AddOption(hostObservableOption);
hostStdioCommand.SetHandler(async (bool observable) =>
{
    if (observable)
    {
        await Legacy89DiskKitApplication.CreateEmulatorHostObservableProtocolStdioRunner().RunAsync();
        return;
    }

    await Legacy89DiskKitApplication.CreateEmulatorHostProtocolStdioRunner().RunAsync();
}, hostObservableOption);
hostScriptD88PathCommand.AddArgument(imageArgument);
hostScriptD88PathCommand.AddArgument(hostOutputArgument);
hostScriptD88PathCommand.SetHandler(async (string imagePath, string outputPath) =>
{
    try
    {
        var requests = Legacy89DiskKitApplication.CreateReadOnlyD88PathScript(imagePath);
        await EmulatorHostRequestScriptFileStore.SaveAsync(outputPath, requests);
        PrintSuccess(localizer, $"Host request script written: {outputPath}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, hostOutputArgument);
hostScriptD88BufferCommand.AddArgument(imageArgument);
hostScriptD88BufferCommand.AddArgument(hostOutputArgument);
hostScriptD88BufferCommand.SetHandler(async (string imagePath, string outputPath) =>
{
    try
    {
        var imageData = await File.ReadAllBytesAsync(imagePath);
        var requests = Legacy89DiskKitApplication.CreateReadOnlyD88BufferScript(imageData);
        await EmulatorHostRequestScriptFileStore.SaveAsync(outputPath, requests);
        PrintSuccess(localizer, $"Host request script written: {outputPath}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, hostOutputArgument);
hostScriptRawBufferCommand.AddArgument(imageArgument);
hostScriptRawBufferCommand.AddArgument(hostOutputArgument);
hostScriptRawBufferCommand.SetHandler(async (string imagePath, string outputPath) =>
{
    try
    {
        var imageData = await File.ReadAllBytesAsync(imagePath);
        var requests = Legacy89DiskKitApplication.CreateReadOnlyRawBufferScript(imageData);
        await EmulatorHostRequestScriptFileStore.SaveAsync(outputPath, requests);
        PrintSuccess(localizer, $"Host request script written: {outputPath}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, hostOutputArgument);
hostScriptInspectCommand.AddArgument(hostFileArgument);
hostScriptInspectCommand.SetHandler(async (string scriptPath) =>
{
    try
    {
        var requests = await Legacy89DiskKitApplication.ReadEmulatorHostRequestScriptAsync(scriptPath);
        Console.WriteLine($"RequestEntries: {requests.Count}");
        Console.WriteLine($"FirstKind: {requests.FirstOrDefault()?.Kind}");
        Console.WriteLine($"LastKind: {requests.LastOrDefault()?.Kind}");
        Console.WriteLine($"Kinds: {string.Join(", ", requests.Select(x => x.Kind))}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, hostFileArgument);
hostScriptCommand.AddCommand(hostScriptD88PathCommand);
hostScriptCommand.AddCommand(hostScriptD88BufferCommand);
hostScriptCommand.AddCommand(hostScriptRawBufferCommand);
hostScriptCommand.AddCommand(hostScriptInspectCommand);
hostBundleInspectCommand.AddArgument(hostDirectoryArgument);
hostBundleInspectCommand.AddArgument(hostBaseNameArgument);
hostBundleInspectCommand.SetHandler(async (string directoryPath, string baseName) =>
{
    try
    {
        var bundle = await EmulatorHostBundleReader.ReadAsync(directoryPath, baseName);
        var report = Legacy89DiskKitApplication.BuildEmulatorHostProofReport(bundle.Transcript, bundle.Manifest.OpenMode, bundle.Manifest.ExchangeMode);
        Console.WriteLine($"BaseName: {bundle.Manifest.BaseName}");
        Console.WriteLine($"OpenMode: {bundle.Manifest.OpenMode}");
        Console.WriteLine($"ExchangeMode: {bundle.Manifest.ExchangeMode}");
        Console.WriteLine($"TranscriptEntries: {bundle.Transcript.Count}");
        Console.WriteLine($"RequestEntries: {bundle.RequestScript.Count}");
        Console.WriteLine($"CapabilityHandshakeSucceeded: {report.CapabilityHandshakeSucceeded}");
        Console.WriteLine($"SupportsPathOpen: {report.SupportsPathOpen}");
        Console.WriteLine($"SupportsBufferOpen: {report.SupportsBufferOpen}");
        Console.WriteLine($"SupportsNotificationExchange: {report.SupportsNotificationExchange}");
        Console.WriteLine($"SupportsPlainStdio: {report.SupportsPlainStdio}");
        Console.WriteLine($"SupportsObservableStdio: {report.SupportsObservableStdio}");
        Console.WriteLine($"DiskOpenSucceeded: {report.DiskOpenSucceeded}");
        Console.WriteLine($"BusyObserved: {report.BusyObserved}");
        Console.WriteLine($"IrqObserved: {report.IrqObserved}");
        Console.WriteLine($"DrqObserved: {report.DrqObserved}");
        Console.WriteLine($"DataReadSucceeded: {report.DataReadSucceeded}");
        Console.WriteLine($"CloseSucceeded: {report.CloseSucceeded}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, hostDirectoryArgument, hostBaseNameArgument);
hostBundleVerifyCommand.AddArgument(hostDirectoryArgument);
hostBundleVerifyCommand.AddArgument(hostBaseNameArgument);
hostBundleVerifyCommand.AddArgument(hostBaselineArgument);
hostBundleVerifyCommand.SetHandler(async (string directoryPath, string baseName, string baselineName) =>
{
    var bundle = await Legacy89DiskKitApplication.ReadEmulatorHostBundleAsync(directoryPath, baseName);
    var expectation = ParseHostBaseline(baselineName);
    var mismatches = Legacy89DiskKitApplication.CompareEmulatorHostBundle(bundle, expectation);
    if (mismatches.Count == 0)
    {
        PrintSuccess(localizer, $"Host-proof bundle matched baseline: {baselineName}");
        return;
    }

    throw new InvalidOperationException($"Host-proof bundle mismatches for baseline '{baselineName}': {string.Join(" ", mismatches)}");
}, hostDirectoryArgument, hostBaseNameArgument, hostBaselineArgument);
hostBundlePackCommand.AddArgument(hostTranscriptArgument);
hostBundlePackCommand.AddArgument(hostDirectoryArgument);
hostBundlePackCommand.AddArgument(hostBaseNameArgument);
hostBundlePackCommand.AddOption(hostRequestScriptOption);
hostBundlePackCommand.AddOption(hostOpenModeOption);
hostBundlePackCommand.AddOption(hostExchangeModeOption);
hostBundlePackCommand.SetHandler(async (string transcriptPath, string directoryPath, string baseName, string? requestScriptPath, string openMode, string exchangeMode) =>
{
    try
    {
        var transcript = await Legacy89DiskKitApplication.ReadEmulatorHostTranscriptAsync(transcriptPath);
        var requestScript = string.IsNullOrWhiteSpace(requestScriptPath) ? null : await Legacy89DiskKitApplication.ReadEmulatorHostRequestScriptAsync(requestScriptPath);
        var report = Legacy89DiskKitApplication.BuildEmulatorHostProofReport(transcript, openMode, exchangeMode);
        await Legacy89DiskKitApplication.WriteEmulatorHostBundleAsync(directoryPath, baseName, report, transcript, requestScript);
        PrintSuccess(localizer, $"Host-proof bundle written: {Path.Combine(directoryPath, $"{baseName}.manifest.json")}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, hostTranscriptArgument, hostDirectoryArgument, hostBaseNameArgument, hostRequestScriptOption, hostOpenModeOption, hostExchangeModeOption);
hostTranscriptInspectCommand.AddArgument(hostTranscriptArgument);
hostTranscriptInspectCommand.AddOption(hostOpenModeOption);
hostTranscriptInspectCommand.AddOption(hostExchangeModeOption);
hostTranscriptInspectCommand.SetHandler(async (string transcriptPath, string openMode, string exchangeMode) =>
{
    try
    {
        var transcript = await Legacy89DiskKitApplication.ReadEmulatorHostTranscriptAsync(transcriptPath);
        var report = Legacy89DiskKitApplication.BuildEmulatorHostProofReport(transcript, openMode, exchangeMode);
        Console.WriteLine($"TranscriptEntries: {transcript.Count}");
        Console.WriteLine($"OpenMode: {report.OpenMode}");
        Console.WriteLine($"ExchangeMode: {report.ExchangeMode}");
        Console.WriteLine($"CapabilityHandshakeSucceeded: {report.CapabilityHandshakeSucceeded}");
        Console.WriteLine($"SupportsPathOpen: {report.SupportsPathOpen}");
        Console.WriteLine($"SupportsBufferOpen: {report.SupportsBufferOpen}");
        Console.WriteLine($"SupportsNotificationExchange: {report.SupportsNotificationExchange}");
        Console.WriteLine($"SupportsPlainStdio: {report.SupportsPlainStdio}");
        Console.WriteLine($"SupportsObservableStdio: {report.SupportsObservableStdio}");
        Console.WriteLine($"DiskOpenSucceeded: {report.DiskOpenSucceeded}");
        Console.WriteLine($"BusyObserved: {report.BusyObserved}");
        Console.WriteLine($"IrqObserved: {report.IrqObserved}");
        Console.WriteLine($"DrqObserved: {report.DrqObserved}");
        Console.WriteLine($"DataReadSucceeded: {report.DataReadSucceeded}");
        Console.WriteLine($"CloseSucceeded: {report.CloseSucceeded}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, hostTranscriptArgument, hostOpenModeOption, hostExchangeModeOption);
hostTranscriptReportCommand.AddArgument(hostTranscriptArgument);
hostTranscriptReportCommand.AddArgument(hostOutputArgument);
hostTranscriptReportCommand.AddOption(hostOpenModeOption);
hostTranscriptReportCommand.AddOption(hostExchangeModeOption);
hostTranscriptReportCommand.SetHandler(async (string transcriptPath, string outputPath, string openMode, string exchangeMode) =>
{
    try
    {
        var transcript = await Legacy89DiskKitApplication.ReadEmulatorHostTranscriptAsync(transcriptPath);
        var report = Legacy89DiskKitApplication.BuildEmulatorHostProofReport(transcript, openMode, exchangeMode);
        var markdown = EmulatorHostProofReportMarkdownRenderer.Render(report);
        await File.WriteAllTextAsync(outputPath, markdown, Encoding.UTF8);
        PrintSuccess(localizer, $"Host-proof report written: {outputPath}");
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, hostTranscriptArgument, hostOutputArgument, hostOpenModeOption, hostExchangeModeOption);
hostTranscriptVerifyCommand.AddArgument(hostTranscriptArgument);
hostTranscriptVerifyCommand.AddArgument(hostBaselineArgument);
hostTranscriptVerifyCommand.AddOption(hostOpenModeOption);
hostTranscriptVerifyCommand.AddOption(hostExchangeModeOption);
hostTranscriptVerifyCommand.SetHandler(async (string transcriptPath, string baselineName, string openMode, string exchangeMode) =>
{
    var transcript = await Legacy89DiskKitApplication.ReadEmulatorHostTranscriptAsync(transcriptPath);
    var report = Legacy89DiskKitApplication.BuildEmulatorHostProofReport(transcript, openMode, exchangeMode);
    var expectation = ParseHostBaseline(baselineName);
    var mismatches = Legacy89DiskKitApplication.CompareEmulatorHostProofReport(report, expectation);
    if (mismatches.Count == 0)
    {
        PrintSuccess(localizer, $"Host-proof transcript matched baseline: {baselineName}");
        return;
    }

    throw new InvalidOperationException($"Host-proof transcript mismatches for baseline '{baselineName}': {string.Join(" ", mismatches)}");
}, hostTranscriptArgument, hostBaselineArgument, hostOpenModeOption, hostExchangeModeOption);
hostBundleCommand.AddCommand(hostBundleInspectCommand);
hostBundleCommand.AddCommand(hostBundleVerifyCommand);
hostBundleCommand.AddCommand(hostBundlePackCommand);
hostTranscriptCommand.AddCommand(hostTranscriptInspectCommand);
hostTranscriptCommand.AddCommand(hostTranscriptReportCommand);
hostTranscriptCommand.AddCommand(hostTranscriptVerifyCommand);
hostCommand.AddCommand(hostStdioCommand);
hostCommand.AddCommand(hostScriptCommand);
hostCommand.AddCommand(hostBundleCommand);
hostCommand.AddCommand(hostTranscriptCommand);
var bootCommand = new Command("boot", localizer.BootCommandDescription);
var filesOption = new Option<string>("--files", () => "all", localizer.BootFilesOptionDescription);
var bootShowCommand = new Command("show", localizer.BootShowCommandDescription);
bootShowCommand.AddArgument(imageArgument);
bootShowCommand.SetHandler((string imagePath) =>
{
    try
    {
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var boot = bootProfileService.GetBootProfile(fs);
        Console.WriteLine(localizer.BootShowTitle);
        Console.WriteLine($"{localizer.BootLabel}: {FormatBootMode(localizer, boot.Mode)}");
        if (!string.IsNullOrWhiteSpace(boot.FileName))
        {
            Console.WriteLine($"{localizer.BootFileLabel}: {boot.FileName}");
        }

        if (boot.LoadAddress.HasValue)
        {
            Console.WriteLine($"{localizer.BootLoadLabel}: {boot.LoadAddress.Value:X4}");
        }

        if (boot.ExecutionAddress.HasValue)
        {
            Console.WriteLine($"{localizer.BootExecLabel}: {boot.ExecutionAddress.Value:X4}");
        }
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument);
var bootClearCommand = new Command("clear", localizer.BootClearCommandDescription);
bootClearCommand.AddArgument(imageArgument);
bootClearCommand.SetHandler((string imagePath) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        huBasicMetadataService.ClearBootRecord(fs);
        PrintSuccess(localizer, localizer.BootClearedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument);
var bootCloneCommand = new Command("clone", localizer.BootCloneCommandDescription);
var cloneSrcArgument = new Argument<string>("src", localizer.SourceImageArgumentDescription);
var cloneDestArgument = new Argument<string>("dest", localizer.DestinationImageArgumentDescription);
bootCloneCommand.AddArgument(cloneSrcArgument);
bootCloneCommand.AddArgument(cloneDestArgument);
bootCloneCommand.AddOption(filesOption);
bootCloneCommand.SetHandler((string src, string dest, string files) =>
{
    try
    {
        if (File.Exists(dest))
        {
            RejectWriteToMultiSlotD88(dest, localizer);
        }

        archiveService.CloneBootable(src, dest, files.Split(','));
        PrintSuccess(localizer, localizer.BootableDiskCreatedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, cloneSrcArgument, cloneDestArgument, filesOption);
var bootExportOutputOption = new Option<string>(new[] { "--output", "-o" }, localizer.BootExportOutputOptionDescription)
{
    IsRequired = true
};
var bootExportCommand = new Command("export", localizer.BootExportCommandDescription);
bootExportCommand.AddArgument(imageArgument);
bootExportCommand.AddOption(bootExportOutputOption);
bootExportCommand.SetHandler(async (string imagePath, string outputPath) =>
{
    try
    {
        using var diskService = CreateDiskService();
        var container = diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var exportService = Legacy89DiskKitApplication.CreateBootEntryExportService();
        var entries = exportService.ExportEntries(container, fs);
        if (entries.Count == 0)
        {
            PrintError(localizer, localizer.BootNoEntriesFoundMessage);
            return;
        }

        Directory.CreateDirectory(outputPath);
        foreach (var entry in entries)
        {
            var binPath = Path.Combine(outputPath, entry.SuggestedBinaryFileName);
            var jsonPath = Path.Combine(outputPath, entry.SuggestedMetadataFileName);
            await File.WriteAllBytesAsync(binPath, entry.Payload);
            var sidecar = new System.Text.Json.Nodes.JsonObject
            {
                ["machineFamily"] = entry.MachineFamily.ToString(),
                ["mode"] = entry.Mode.ToString(),
                ["displayName"] = entry.DisplayName,
                ["suggestedBinaryFileName"] = entry.SuggestedBinaryFileName,
                ["payloadLength"] = entry.Payload.Length,
                ["loadAddress"] = entry.LoadAddress,
                ["executionAddress"] = entry.ExecutionAddress
            };
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = sidecar.ToJsonString(options);
            await File.WriteAllTextAsync(jsonPath, json);
        }

        PrintSuccess(localizer, string.Format(localizer.BootEntriesExportedMessage, outputPath));
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, bootExportOutputOption);
var bootImportBinaryOption = new Option<string>(new[] { "--binary", "-b" }, localizer.BootImportBinaryOptionDescription)
{
    IsRequired = true
};
var bootImportMetadataOption = new Option<string>(new[] { "--metadata", "-m" }, localizer.BootImportMetadataOptionDescription)
{
    IsRequired = true
};
var bootImportStartRecordOption = new Option<int?>("--start-record", "Explicit start record for file-backed boot import.");
var bootImportCommand = new Command("import", localizer.BootImportCommandDescription);
bootImportCommand.AddArgument(imageArgument);
bootImportCommand.AddOption(bootImportBinaryOption);
bootImportCommand.AddOption(bootImportMetadataOption);
bootImportCommand.AddOption(bootImportStartRecordOption);
bootImportCommand.SetHandler(async (string imagePath, string binaryPath, string metadataPath, int? startRecord) =>
{
    try
    {
        var binary = await File.ReadAllBytesAsync(binaryPath);
        var json = await File.ReadAllTextAsync(metadataPath);
        var metadata = System.Text.Json.JsonSerializer.Deserialize(
            json,
            Legacy89DiskKit.FileSystem.Application.BootEntryImportJsonContext.Default.BootEntryImportMetadata);
        if (metadata == null)
        {
            throw new InvalidOperationException("Failed to deserialize boot metadata from sidecar file.");
        }

        if (startRecord != null)
        {
            metadata = metadata with
            {
                StartRecord = (ushort)startRecord.Value
            };
        }

        using var diskService = CreateDiskService();
        var container = OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var importService = Legacy89DiskKitApplication.CreateBootEntryImportService();
        importService.ImportEntry(container, fs, metadata, binary);
        container.Save();
        PrintSuccess(localizer, localizer.BootEntryImportedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, bootImportBinaryOption, bootImportMetadataOption, bootImportStartRecordOption);
bootCommand.AddCommand(bootExportCommand);
bootCommand.AddCommand(bootImportCommand);
bootCommand.AddCommand(bootShowCommand);
bootCommand.AddCommand(bootClearCommand);
bootCommand.AddCommand(bootCloneCommand);
var layoutCommand = new Command("layout", localizer.LayoutCommandDescription);
var beforeOption = new Option<string>("--before", localizer.LayoutBeforeOptionDescription)
{
    IsRequired = true
};
var sortByOption = new Option<string>("--by", () => "name", localizer.LayoutSortByOptionDescription);
var layoutInputOption = new Option<string?>("--input", localizer.LayoutInputOptionDescription);
var layoutOutputOption = new Option<string?>("--output", localizer.LayoutOutputOptionDescription);
var layoutStdinOption = new Option<bool>("--stdin", localizer.LayoutStdinOptionDescription);
var layoutStrictOption = new Option<bool>("--strict", localizer.LayoutStrictOptionDescription);
var labelArgument = new Argument<string>("text", localizer.LabelTextArgumentDescription);
var layoutShowCommand = new Command("show", localizer.LayoutShowCommandDescription);
layoutShowCommand.AddArgument(imageArgument);
layoutShowCommand.SetHandler((string imagePath, string? encodingOverride) =>
{
    try
    {
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var layout = directoryLayoutService.GetLayout(fs);
        foreach (var item in layout.Items.OrderBy(item => item.Order))
        {
            var displayName = item.Entry != null ? ResolveDisplayName(fs, item.Entry, archiveService, encodingOverride) : item.DisplayName;
            Console.WriteLine($"{item.Order:D3} [{item.Kind}] {DirectoryLayoutService.CreateStableId(item.Id)} {displayName}");
        }
    }
    catch (InvalidOperationException ex)when (ex.Message.Contains("Directory layout"))
    {
        PrintError(localizer, localizer.LayoutNotSupportedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, encodingOption);
var layoutMoveCommand = new Command("move", localizer.LayoutMoveCommandDescription);
layoutMoveCommand.AddArgument(imageArgument);
layoutMoveCommand.AddArgument(sourceNameArgument);
layoutMoveCommand.AddOption(beforeOption);
layoutMoveCommand.SetHandler((string imagePath, string sourceName, string beforeName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        directoryLayoutService.MoveEntryBefore(fs, sourceName, beforeName);
        PrintSuccess(localizer, localizer.LayoutUpdatedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, sourceNameArgument, beforeOption);
var layoutInsertLabelCommand = new Command("insert-label", localizer.LayoutInsertLabelCommandDescription);
layoutInsertLabelCommand.AddArgument(imageArgument);
layoutInsertLabelCommand.AddArgument(labelArgument);
layoutInsertLabelCommand.AddOption(beforeOption);
layoutInsertLabelCommand.SetHandler((string imagePath, string labelText, string beforeName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        directoryLayoutService.InsertLabelBefore(fs, labelText, beforeName);
        PrintSuccess(localizer, localizer.LabelInsertedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, labelArgument, beforeOption);
var layoutSortCommand = new Command("sort", localizer.LayoutSortCommandDescription);
layoutSortCommand.AddArgument(imageArgument);
layoutSortCommand.AddOption(sortByOption);
layoutSortCommand.SetHandler((string imagePath, string sortBy) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var mode = sortBy.ToLowerInvariant() switch
        {
            "ext" => DirectorySortBy.Extension,
            "type" => DirectorySortBy.Type,
            _ => DirectorySortBy.Name
        };
        directoryLayoutService.SortEntries(fs, mode);
        PrintSuccess(localizer, localizer.DirectoryEntriesSortedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, sortByOption);
var layoutExportCommand = new Command("export", localizer.LayoutExportCommandDescription);
layoutExportCommand.AddArgument(imageArgument);
layoutExportCommand.AddOption(layoutOutputOption);
layoutExportCommand.SetHandler((string imagePath, string? outputPath) =>
{
    try
    {
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var text = directoryLayoutService.ExportPlan(fs);
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Console.Write(text);
        }
        else
        {
            File.WriteAllText(outputPath, text, Encoding.UTF8);
        }
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, layoutOutputOption);
var layoutValidateCommand = new Command("validate", localizer.LayoutValidateCommandDescription);
layoutValidateCommand.AddArgument(imageArgument);
layoutValidateCommand.AddOption(layoutInputOption);
layoutValidateCommand.AddOption(layoutStdinOption);
layoutValidateCommand.AddOption(layoutStrictOption);
layoutValidateCommand.SetHandler((string imagePath, string? inputPath, bool fromStdin, bool strict) =>
{
    try
    {
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var text = ReadLayoutInput(inputPath, fromStdin, localizer);
        var result = directoryLayoutService.ValidatePlan(fs, text);
        RenderValidationResult(result, strict, localizer);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, layoutInputOption, layoutStdinOption, layoutStrictOption);
var layoutApplyCommand = new Command("apply", localizer.LayoutApplyCommandDescription);
layoutApplyCommand.AddArgument(imageArgument);
layoutApplyCommand.AddOption(layoutInputOption);
layoutApplyCommand.AddOption(layoutStdinOption);
layoutApplyCommand.AddOption(layoutStrictOption);
layoutApplyCommand.SetHandler((string imagePath, string? inputPath, bool fromStdin, bool strict) =>
{
    try
    {
        using var diskService = CreateDiskService();
        OpenWritableDisk(diskService, imagePath, localizer);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var text = ReadLayoutInput(inputPath, fromStdin, localizer);
        var result = directoryLayoutService.ApplyPlan(fs, text, strict);
        RenderValidationResult(result, strict, localizer);
        if (result.IsValid && (!strict || result.WarningCount == 0))
        {
            PrintSuccess(localizer, localizer.LayoutAppliedMessage);
        }
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, layoutInputOption, layoutStdinOption, layoutStrictOption);
layoutCommand.AddCommand(layoutShowCommand);
layoutCommand.AddCommand(layoutMoveCommand);
layoutCommand.AddCommand(layoutInsertLabelCommand);
layoutCommand.AddCommand(layoutSortCommand);
layoutCommand.AddCommand(layoutExportCommand);
layoutCommand.AddCommand(layoutValidateCommand);
layoutCommand.AddCommand(layoutApplyCommand);
var injectCommand = new Command("inject", localizer.FileInjectCommandDescription);
injectCommand.AddArgument(imageArgument);
injectCommand.AddArgument(hostFileArgument);
injectCommand.AddOption(targetFileNameOption);
injectCommand.AddOption(imageFileOverwriteOption);
injectCommand.SetHandler((string imagePath, string hostFilePath, string? targetName, bool imageFileOverwrite, string? encodingOverride) =>
{
    try
    {
        RejectWriteToMultiSlotD88(imagePath, localizer);
        
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, false);
        var fs = diskService.FileSystem;
        if (fs == null)
            throw new Exception("Unsupported file system on target disk.");
        
        var fsInfo = fs.GetFileSystemInfo();
        string encId = encodingOverride ?? fsInfo.DefaultEncodingId;
        var existingNames = new HashSet<string>(fs.GetFiles().Select(f => f.FullName.ToUpperInvariant()));
        string sourceName = targetName ?? Path.GetFileName(hostFilePath);
        var canOverwriteExactName = imageFileOverwrite;
        if (canOverwriteExactName && fs.FileExists(sourceName))
        {
            if (TryDeleteExistingFile(fs, sourceName))
            {
                existingNames.Remove(sourceName.ToUpperInvariant());
            }
            else
            {
                canOverwriteExactName = false;
                _logSystem?.Warning(localizer.ImageFileOverwriteIgnoredWarning, "inject");
            }
        }
        var normalizationService = new FileNameNormalizationService(archiveService.EncoderRegistry);
        var (normalizedName, overwritten) = ResolveImageFileTargetName(
            sourceName, encId, fsInfo, existingNames, normalizationService, canOverwriteExactName, _logSystem);
        
        if (overwritten)
        {
            _logSystem?.Info($"Overwriting existing file: {normalizedName}", "inject");
        }

        byte[] data = File.ReadAllBytes(hostFilePath);
        bool isAscii = IsLikelyAsciiPayload(data);
        Console.WriteLine($"Injecting '{sourceName}' as '{normalizedName}' (Encoding: {encId}, {(isAscii ? "ASCII" : "BIN")})...");
        if (isAscii)
        {
            var encoder = archiveService.EncoderRegistry.GetEncoder(encId) ?? throw new InvalidOperationException($"Unsupported encoding: {encId}");
            var transferService = new FileTransferService(encoder);
            transferService.ImportFile(fs, hostFilePath, normalizedName, true, null);
        }
        else
        {
            var attributes = fs.CreateDefaultAttributes(false);
            fs.WriteFile(normalizedName, data, attributes);
        }

        diskService.Session?.Save();
        PrintSuccess(localizer, localizer.FileInjectedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, hostFileArgument, targetFileNameOption, imageFileOverwriteOption, encodingOption);
var checkUpdateCommand = new Command("check-update", localizer.CheckUpdateCommandDescription);
checkUpdateCommand.SetHandler(async () =>
{
    try
    {
        var checker = new ReleaseUpdateChecker();
        var result = await checker.CheckAsync();
        Console.WriteLine($"{localizer.CheckUpdateCurrentVersionLabel}: {result.CurrentVersion}");
        Console.WriteLine($"{localizer.CheckUpdateLatestVersionLabel}: {result.LatestVersion ?? "unknown"}");
        if (!string.IsNullOrWhiteSpace(result.ReleaseUrl))
        {
            Console.WriteLine($"{localizer.CheckUpdateReleaseUrlLabel}: {result.ReleaseUrl}");
        }

        if (!string.IsNullOrWhiteSpace(result.WindowsMsiUrl))
        {
            Console.WriteLine($"{localizer.CheckUpdateWindowsMsiLabel}: {result.WindowsMsiUrl}");
        }

        Console.WriteLine(result.IsUpdateAvailable ? localizer.CheckUpdateAvailableMessage : localizer.CheckUpdateUpToDateMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
});
rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(fullHelpCommand);
rootCommand.AddCommand(versionCommand);
rootCommand.AddCommand(fileCommand);
rootCommand.AddCommand(diskCommand);
rootCommand.AddCommand(hostCommand);
rootCommand.AddCommand(bootCommand);
rootCommand.AddCommand(layoutCommand);
rootCommand.AddCommand(injectCommand);
rootCommand.AddCommand(checkUpdateCommand);
try
{
    return await rootCommand.InvokeAsync(effectiveArgs);
}
finally
{
    _logSystem?.Dispose();
}
static string? TryGetRequestedLanguage(string[] rawArgs)
{
    for (var i = 0; i < rawArgs.Length; i++)
    {
        var arg = rawArgs[i];
        if (arg is "--language" or "-l")
        {
            if (i + 1 < rawArgs.Length)
            {
                return rawArgs[i + 1].Trim().ToLowerInvariant();
            }

            return null;
        }

        if (arg.StartsWith("--language=", StringComparison.OrdinalIgnoreCase))
        {
            return arg["--language=".Length..].Trim().ToLowerInvariant();
        }
    }

    return null;
}

static string? TryGetRequestedEncoding(string[] rawArgs)
{
    for (var i = 0; i < rawArgs.Length; i++)
    {
        var arg = rawArgs[i];
        if (arg is "--encoding" or "-e")
        {
            if (i + 1 < rawArgs.Length)
            {
                return rawArgs[i + 1].Trim();
            }

            return null;
        }

        if (arg.StartsWith("--encoding=", StringComparison.OrdinalIgnoreCase))
        {
            var value = arg["--encoding=".Length..].Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        if (arg.StartsWith("-e=", StringComparison.OrdinalIgnoreCase))
        {
            var value = arg["-e=".Length..].Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    return null;
}

static string? ExtractLogPath(string[] rawArgs)
{
    for (var i = 0; i < rawArgs.Length; i++)
    {
        var arg = rawArgs[i];
        if (arg == "--log")
        {
            if (i + 1 < rawArgs.Length && !rawArgs[i + 1].StartsWith("-"))
            {
                return rawArgs[i + 1];
            }
            return null;
        }

        if (arg.StartsWith("--log=", StringComparison.OrdinalIgnoreCase))
        {
            var value = arg["--log=".Length..];
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    return null;
}

static string[] RewriteLegacyArgs(string[] rawArgs)
{
    if (rawArgs.Length >= 3 && string.Equals(rawArgs[0], "boot", StringComparison.OrdinalIgnoreCase) && !IsBootSubcommand(rawArgs[1]))
    {
        return[rawArgs[0], "clone", ..rawArgs[1..]];
    }

    return rawArgs;
}

static string[] RewriteUpdateCheckArgs(string[] rawArgs)
{
    if (!rawArgs.Any(arg => arg == "--check-update"))
    {
        return rawArgs;
    }

    var filteredArgs = rawArgs.Where(arg => arg != "--check-update").ToArray();
    return["check-update", ..filteredArgs];
}

static string[] RewriteFullHelpArgs(string[] rawArgs)
{
    if (!rawArgs.Any(arg => arg == "--full-help"))
    {
        return rawArgs;
    }

    var filteredArgs = rawArgs.Where(arg => arg != "--full-help").ToArray();
    return["full-help", ..filteredArgs];
}

static string[] RewriteVersionArgs(string[] rawArgs)
{
    if (!rawArgs.Any(arg => arg is "--version" or "-v"))
    {
        return rawArgs;
    }

    var filteredArgs = rawArgs.Where(arg => arg is not "--version" and not "-v").ToArray();
    return["version", ..filteredArgs];
}

static string[] RewriteImplicitInspectorArgs(string[] rawArgs)
{
    if (rawArgs.Length == 0)
    {
        return rawArgs;
    }

    var first = rawArgs[0];
    if (first.StartsWith("-", StringComparison.Ordinal) || IsTopLevelCommand(first) || !LooksLikeDiskImagePath(first))
    {
        return rawArgs;
    }

    return["disk", "inspector", ..rawArgs];
}

static bool IsBootSubcommand(string value)
{
    return value is "export" or "import" or "show" or "clear" or "clone" or "-h" or "--help";
}

static bool IsTopLevelCommand(string value)
{
    return value is "list" or "full-help" or "file" or "disk" or "host" or "boot" or "layout" or "inject" or "check-update" or "-h" or "--help";
}

static bool LooksLikeDiskImagePath(string value)
{
    if (File.Exists(value))
    {
        return true;
    }

    var extension = Path.GetExtension(value);
    return extension.Equals(".d88", StringComparison.OrdinalIgnoreCase) || extension.Equals(".d77", StringComparison.OrdinalIgnoreCase) || extension.Equals(".2d", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase);
}

static DiskService CreateDiskService()
{
    return new DiskService();
}

static DiskType ParseDiskType(string diskTypeName)
{
    return diskTypeName.Trim().ToLowerInvariant() switch
    {
        "2d" => DiskType.TwoD,
        "2dd" => DiskType.TwoDD,
        "2hd" => DiskType.TwoHD,
        _ => throw new InvalidOperationException($"Unsupported disk type: {diskTypeName}")};
}

static EmulatorHostProofExpectation ParseHostBaseline(string baselineName)
{
    return baselineName.Trim().ToLowerInvariant() switch
    {
        "event-d88" => EmulatorHostProofExpectationCatalog.EventDrivenFirstProofD88(),
        "event-raw" => EmulatorHostProofExpectationCatalog.EventDrivenSecondProofRaw(),
        _ => throw new InvalidOperationException($"Unsupported host baseline: {baselineName}")};
}

static IFileSystem? RequireFileSystem(IFileSystem? fileSystem, IConsoleLocalizer localizer)
{
    if (fileSystem != null)
    {
        return fileSystem;
    }

    PrintError(localizer, localizer.FileSystemNotDetectedMessage);
    return null;
}

static FileTransferService CreateFileTransferService(DiskFileSystemInfo fsInfo, string? encodingOverride)
{
    return Legacy89DiskKitApplication.CreateFileTransferService(fsInfo, encodingOverride);
}

static void RenderFileList(FileListView view, IFileListLocalizer localizer, string outputFormat)
{
    if (string.Equals(outputFormat, "csv", StringComparison.OrdinalIgnoreCase))
    {
        RenderFileListCsv(view);
        return;
    }

    foreach (var item in view.Summary)
    {
        Console.WriteLine($"{item.Label}: {item.Value}");
    }

    if (view.Summary.Count > 0)
    {
        Console.WriteLine();
    }

    if (view.Columns.Count == 0)
    {
        return;
    }

    var widths = new int[view.Columns.Count];
    for (int i = 0; i < view.Columns.Count; i++)
    {
        widths[i] = DisplayWidthUtility.GetWidth(view.Columns[i].Header);
    }

    foreach (var row in view.Rows)
    {
        for (int i = 0; i < row.Values.Count; i++)
        {
            widths[i] = Math.Max(widths[i], DisplayWidthUtility.GetWidth(row.Values[i]));
        }
    }

    Console.WriteLine(FormatRow(view.Columns.Select(column => column.Header).ToArray(), view.Columns, widths));
    Console.WriteLine(string.Join("-+-", widths.Select(width => new string ('-', width))));
    foreach (var row in view.Rows)
    {
        Console.WriteLine(FormatRow(row.Values, view.Columns, widths));
    }

    if (view.Footnotes.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"{localizer.FootnotesLabel}:");
        foreach (var footnote in view.Footnotes)
        {
            Console.WriteLine($"*{footnote.Number} {footnote.Text}");
        }
    }

    if (view.Legends.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"{localizer.LegendsLabel}:");
        foreach (var legend in view.Legends)
        {
            Console.WriteLine($"{legend.Key} = {legend.Description}");
        }
    }
}

static void RenderFileListCsv(FileListView view)
{
    var headers = view.Columns.Select(column => EscapeCsv(column.Header)).ToArray();
    Console.WriteLine(string.Join(",", headers));
    foreach (var row in view.Rows)
    {
        Console.WriteLine(string.Join(",", row.Values.Select(EscapeCsv)));
    }
}

static void RenderInspectionReport(InspectionReport report, string outputFormat)
{
    if (string.Equals(outputFormat, "csv", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Section,Key,Value");
        foreach (var item in report.Items)
        {
            Console.WriteLine($"{EscapeCsv(item.Section)},{EscapeCsv(item.Key)},{EscapeCsv(item.Value)}");
        }

        return;
    }

    string? currentSection = null;
    foreach (var item in report.Items)
    {
        if (!string.Equals(currentSection, item.Section, StringComparison.Ordinal))
        {
            if (currentSection != null)
            {
                Console.WriteLine();
            }

            Console.WriteLine($"[{item.Section}]");
            currentSection = item.Section;
        }

        Console.WriteLine($"{item.Key}: {item.Value}");
    }
}

static string EscapeCsv(string value)
{
    if (value.Contains('"'))
    {
        value = value.Replace("\"", "\"\"");
    }

    return value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{value}\"" : value;
}

static string FormatRow(IReadOnlyList<string> values, IReadOnlyList<FileListColumn> columns, IReadOnlyList<int> widths)
{
    var cells = new string[values.Count];
    for (int i = 0; i < values.Count; i++)
    {
        cells[i] = columns[i].RightAlign ? DisplayWidthUtility.PadLeft(values[i], widths[i]) : DisplayWidthUtility.PadRight(values[i], widths[i]);
    }

    return string.Join(" | ", cells);
}

static (DiskFileSystemInfo fsInfo, FileListEntryContext[] entries) BuildFileListEntries(IFileSystem fs, ArchiveService archiveService, string? encodingOverride)
{
    var fsInfo = fs.GetFileSystemInfo();
    var files = fs.GetFiles().ToArray();
    var entries = new List<FileListEntryContext>(files.Length);
    var encoder = archiveService.EncoderRegistry.GetEncoder(encodingOverride ?? fsInfo.DefaultEncodingId);
    foreach (var file in files)
    {
        string displayName = file.FullName;
        string displayBaseName = file.FileName;
        string displayExtension = file.Extension;
        if (encoder != null && file.RawFileName != null)
        {
            string name = encoder.DecodeText(file.RawFileName).TrimEnd(' ');
            string ext = file.RawExtension != null ? encoder.DecodeText(file.RawExtension).TrimEnd(' ') : "";
            displayName = string.IsNullOrEmpty(ext) ? name : $"{name}.{ext}";
            displayBaseName = name;
            displayExtension = ext;
        }

        long? actualSize = null;
        if (fsInfo.FileSystemName == "Hu-BASIC")
        {
            actualSize = IsProbablyVirtualLabel(file) ? 0 : fs.ReadFile(file.FullName).Length;
        }

        var(directoryOffset, bodyOffset) = ResolveFileOffsets(fs, file);
        entries.Add(new FileListEntryContext(file, displayName, displayBaseName, displayExtension, actualSize, directoryOffset, bodyOffset));
    }

    return (fsInfo, entries.ToArray());
}

static (long? DirectoryOffset, long? BodyOffset) ResolveFileOffsets(IFileSystem fs, FileEntry file)
{
    return fs switch
    {
        HuBasicFileSystem huBasic => ResolveHuBasicOffsets(huBasic, file),
        XDosFileSystem xdos => ResolveXDosOffsets(xdos, file),
        _ => (null, null)};
}

static (long? DirectoryOffset, long? BodyOffset) ResolveHuBasicOffsets(HuBasicFileSystem fs, FileEntry file)
{
    var slot = fs.FindDirectorySlot(file.FullName);
    long? dirOffset = null;
    if (slot is { } dirSlot)
    {
        dirOffset = ((long)fs.GetDirectoryRecordNumber(dirSlot.SectorIndex) * 256) + dirSlot.Offset;
    }

    var bodyOffset = (long)fs.GetStartRecordForCluster(file.StartCluster) * 256;
    return (dirOffset, bodyOffset);
}

static (long? DirectoryOffset, long? BodyOffset) ResolveXDosOffsets(XDosFileSystem fs, FileEntry file)
{
    if (file.FileSystemMetadata is not XDosFileMetadata metadata || file.RawFileName == null)
    {
        return (null, null);
    }

    long? dirOffset = null;
    var slot = fs.FindDirectorySlot(file.RawFileName, metadata.RawFileType);
    if (slot is { } dirSlot)
    {
        dirOffset = ((long)fs.Geometry.DataSectorSize * (fs.Geometry.DataSectorsPerTrack + (dirSlot.Sector - 1))) + dirSlot.Offset;
    }

    long? bodyOffset = null;
    var dirEntry = fs.FindDirectoryEntry(file.RawFileName, metadata.RawFileType);
    if (dirEntry != null)
    {
        var fam = fs.GetFamEntries(dirEntry);
        if (fam.Count > 0)
        {
            bodyOffset = (((long)fam[0].Track * fs.Geometry.DataSectorsPerTrack) + (fam[0].Sector - 1)) * fs.Geometry.DataSectorSize;
        }
    }

    return (dirOffset, bodyOffset);
}

static bool IsProbablyVirtualLabel(FileEntry entry)
{
    if (entry.FileSystemMetadata is not HuBasicFileMetadata metadata)
    {
        return false;
    }

    if (metadata.FileType != HuBasicFileType.Ascii)
    {
        return false;
    }

    var looksDecorative = entry.FullName.All(ch => ch is '-' or '.' or ' ');
    var hasSentinelAddresses = entry.LoadAddress == 0xFFFF && entry.ExecutionAddress == 0xFFFF && (entry.EndAddress == 0xFFFF || entry.Size == 0);
    var suspiciousCluster = entry.StartCluster >= 0x7FFF;
    var labelFlags = metadata.HasPassword && metadata.IsWriteProtected && !metadata.IsHidden && !metadata.IsVerify;
    return (looksDecorative || suspiciousCluster || hasSentinelAddresses) && (labelFlags || suspiciousCluster || hasSentinelAddresses);
}

static bool IsLikelyAsciiPayload(byte[] data)
{
    if (data.Length == 0)
    {
        return true;
    }

    int count = Math.Min(data.Length, 1024);
    int nonPrintable = 0;
    for (int i = 0; i < count; i++)
    {
        if (data[i] == 0)
        {
            return false;
        }

        if (data[i] < 32 && data[i] != 9 && data[i] != 10 && data[i] != 13 && data[i] != 0x1A)
        {
            nonPrintable++;
        }
    }

    return (double)nonPrintable / count < 0.1;
}

static string ResolveCreatePath(string imagePath, string? imageFormatName)
{
    if (string.IsNullOrWhiteSpace(imageFormatName))
    {
        return imagePath;
    }

    var normalizedExtension = ParseImageFormat(imageFormatName);
    var currentExtension = Path.GetExtension(imagePath);
    if (string.IsNullOrWhiteSpace(currentExtension))
    {
        return $"{imagePath}{normalizedExtension}";
    }

    if (!currentExtension.Equals(normalizedExtension, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Image path extension '{currentExtension}' does not match requested image format '{normalizedExtension}'.");
    }

    return imagePath;
}

static string ParseImageFormat(string imageFormatName)
{
    var normalized = imageFormatName.Trim().ToLowerInvariant();
    return normalized switch
    {
        "d88" or ".d88" => ".d88",
        "d77" or ".d77" => ".d77",
        "2d" or ".2d" => ".2d",
        "dsk" or ".dsk" => ".dsk",
        _ => throw new InvalidOperationException($"Unsupported image format: {imageFormatName}")};
}

static void PrintFullHelp(Command rootCommand, IConsoleLocalizer localizer)
{
    Console.WriteLine(localizer.RootDescription);
    Console.WriteLine();
    PrintCommandHelpRecursive(rootCommand, "l89", 0);
    Console.WriteLine();
    Console.WriteLine(localizer.FullHelpFooter);
}

static void PrintCommandHelpRecursive(Command command, string commandPath, int depth)
{
    var indent = new string (' ', depth * 2);
    var commandAliases = string.Join(", ", command.Aliases.Where(x => !string.Equals(x, command.Name, StringComparison.OrdinalIgnoreCase)));
    Console.WriteLine(string.IsNullOrWhiteSpace(commandAliases) ? $"{indent}{commandPath}" : $"{indent}{commandPath} (aliases: {commandAliases})");
    if (!string.IsNullOrWhiteSpace(command.Description))
    {
        Console.WriteLine($"{indent}  {command.Description}");
    }

    var arguments = command.Arguments.ToArray();
    if (arguments.Length > 0)
    {
        Console.WriteLine($"{indent}  Arguments:");
        foreach (var argument in arguments)
        {
            Console.WriteLine($"{indent}    <{argument.Name}>  {argument.Description}");
        }
    }

    var options = command.Options.ToArray();
    if (options.Length > 0)
    {
        Console.WriteLine($"{indent}  Options:");
        foreach (var option in options.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            var aliases = string.Join(", ", option.Aliases);
            Console.WriteLine($"{indent}    {aliases}  {option.Description}");
        }
    }

    var subcommands = command.Subcommands.Where(x => !string.Equals(x.Name, "help", StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    if (subcommands.Length == 0)
    {
        return;
    }

    Console.WriteLine($"{indent}  Commands:");
    foreach (var subcommand in subcommands)
    {
        var aliases = string.Join(", ", subcommand.Aliases.Where(x => !string.Equals(x, subcommand.Name, StringComparison.OrdinalIgnoreCase)));
        var suffix = string.IsNullOrWhiteSpace(aliases) ? string.Empty : $" [{aliases}]";
        Console.WriteLine($"{indent}    {subcommand.Name}{suffix}  {subcommand.Description}");
    }

    foreach (var subcommand in subcommands)
    {
        Console.WriteLine();
        PrintCommandHelpRecursive(subcommand, $"{commandPath} {subcommand.Name}", depth + 1);
    }
}

static void PrintInspector(string imagePath, string detail, string outputFormat, string? fileSystemName, string? encodingOverride, IConsoleLocalizer localizer, DiskInspectionService diskInspectionService, ArchiveService archiveService, HuBasicMetadataService huBasicMetadataService, IBootProfileService bootProfileService, ExplicitFileSystemResolver explicitFileSystemResolver)
{
    using var diskService = CreateDiskService();
    var container = diskService.OpenDisk(imagePath, true);
    var metadata = diskService.GetContainerMetadata() ?? throw new InvalidOperationException("Container metadata is unavailable.");
    var fileSystem = ResolveFileSystem(diskService, container, fileSystemName, explicitFileSystemResolver);
    var normalizedDetail = detail.Trim().ToLowerInvariant();
    if (normalizedDetail is not ("short" or "normal" or "full"))
    {
        throw new InvalidOperationException($"Unsupported detail level: {detail}");
    }

    var report = diskInspectionService.BuildReport(metadata, fileSystem, fileSystem != null ? bootProfileService.GetBootProfile(fileSystem) : null, normalizedDetail, encodingOverride ?? fileSystem?.GetFileSystemInfo().DefaultEncodingId ?? "unknown");
    var mergedItems = report.Items.ToList();
    if (fileSystem != null && normalizedDetail == "full")
    {
        if (fileSystem.GetFileSystemInfo().FileSystemName == "Hu-BASIC")
        {
            var bootRecord = huBasicMetadataService.GetBootRecordInfo(fileSystem);
            if (bootRecord is not null)
            {
                var fullName = string.IsNullOrWhiteSpace(bootRecord.Extension) ? bootRecord.Name : $"{bootRecord.Name}.{bootRecord.Extension}";
                mergedItems.Add(new InspectionItem("Boot", "Boot File", fullName));
            }
        }

        var(_, entries) = BuildFileListEntries(fileSystem, archiveService, encodingOverride);
        foreach (var entry in entries.Take(10))
        {
            mergedItems.Add(new InspectionItem("Preview", "File", entry.DisplayName));
        }
    }

    RenderInspectionReport(report with { Items = mergedItems }, outputFormat);
}

static void PrintFileInspector(string imagePath, string diskFileName, string detail, string outputFormat, string? fileSystemName, string? encodingOverride, IConsoleLocalizer localizer, ArchiveService archiveService, FileInspectionService fileInspectionService, ExplicitFileSystemResolver explicitFileSystemResolver)
{
    var normalizedDetail = detail.Trim().ToLowerInvariant();
    if (normalizedDetail is not ("short" or "normal" or "full"))
    {
        throw new InvalidOperationException($"Unsupported detail level: {detail}");
    }

    using var diskService = CreateDiskService();
    var container = diskService.OpenDisk(imagePath, true);
    var fs = RequireFileSystem(ResolveFileSystem(diskService, container, fileSystemName, explicitFileSystemResolver), localizer);
    if (fs == null)
    {
        return;
    }

    var(_, entries) = BuildFileListEntries(fs, archiveService, encodingOverride);
    var target = entries.FirstOrDefault(x => x.DisplayName.Equals(diskFileName, StringComparison.OrdinalIgnoreCase) || x.Entry.FullName.Equals(diskFileName, StringComparison.OrdinalIgnoreCase));
    if (target == null)
    {
        throw new FileNotFoundException($"File not found: {diskFileName}");
    }

    var report = fileInspectionService.BuildReport(fs, target.Entry, target.DisplayName, normalizedDetail);
    RenderInspectionReport(report, outputFormat);
}

static IFileSystem? ResolveFileSystem(DiskService diskService, IDiskContainer container, string? fileSystemName, ExplicitFileSystemResolver explicitFileSystemResolver)
{
    if (string.IsNullOrWhiteSpace(fileSystemName))
    {
        return diskService.FileSystem;
    }

    return explicitFileSystemResolver.Create(fileSystemName, container);
}

static IDiskContainer OpenWritableDisk(DiskService diskService, string imagePath, IConsoleLocalizer localizer)
{
    RejectWriteToMultiSlotD88(imagePath, localizer);
    return diskService.OpenDisk(imagePath, false);
}

static void RejectWriteToMultiSlotD88(string imagePath, IConsoleLocalizer localizer)
{
    if (!File.Exists(imagePath))
    {
        return;
    }

    if (!string.Equals(Path.GetExtension(imagePath), ".d88", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var slotCount = CountD88Slots(File.ReadAllBytes(imagePath));
    if (slotCount > 1)
    {
        throw new InvalidOperationException(localizer.MultiSlotD88WriteNotSupportedMessage);
    }
}

static int CountD88Slots(byte[] imageData)
{
    if (imageData.Length < 0x2b0)
    {
        return 0;
    }

    var offset = 0;
    var count = 0;
    while (offset + 0x2b0 <= imageData.Length)
    {
        var mediaTypeOffset = offset + 0x1b;
        var diskSizeOffset = offset + 0x1c;
        if (mediaTypeOffset >= imageData.Length || diskSizeOffset + 4 > imageData.Length)
        {
            break;
        }

        var mediaType = imageData[mediaTypeOffset];
        if (!Enum.IsDefined(typeof(DiskType), mediaType))
        {
            break;
        }

        var diskSize = BitConverter.ToUInt32(imageData, diskSizeOffset);
        if (diskSize < 0x2b0 || offset + diskSize > imageData.Length)
        {
            break;
        }

        count++;
        offset += (int)diskSize;
        if (offset == imageData.Length)
        {
            break;
        }
    }

    return count;
}

static byte[] ReadLinearSectors(IDiskContainer container, int startSector, int count)
{
    if (startSector < 0)
    {
        throw new InvalidOperationException("Sector number must be zero or greater.");
    }

    if (count <= 0)
    {
        throw new InvalidOperationException("Sector count must be greater than zero.");
    }

    var sectors = GetOrderedSectors(container);
    if (startSector + count > sectors.Count)
    {
        throw new InvalidOperationException("Requested sector range is outside the disk image.");
    }

    using var stream = new MemoryStream();
    for (var i = 0; i < count; i++)
    {
        var sectorInfo = sectors[startSector + i];
        var data = container.ReadSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector);
        stream.Write(data, 0, data.Length);
    }

    return stream.ToArray();
}

static void WriteLinearSectors(IDiskContainer container, int startSector, byte[] data, int? explicitSectorCount)
{
    if (startSector < 0)
    {
        throw new InvalidOperationException("Sector number must be zero or greater.");
    }

    var sectors = GetOrderedSectors(container);
    if (startSector >= sectors.Count)
    {
        throw new InvalidOperationException("Requested sector range is outside the disk image.");
    }

    var sectorSize = sectors[startSector].Size;
    var sectorCount = explicitSectorCount ?? (data.Length + sectorSize - 1) / sectorSize;
    if (sectorCount <= 0)
    {
        throw new InvalidOperationException("Sector count must be greater than zero.");
    }

    if (startSector + sectorCount > sectors.Count)
    {
        throw new InvalidOperationException("Requested sector range is outside the disk image.");
    }

    if (data.Length > sectorCount * sectorSize)
    {
        throw new InvalidOperationException("Input file is larger than the requested sector range.");
    }

    for (var i = 0; i < sectorCount; i++)
    {
        var offset = i * sectorSize;
        var chunkSize = Math.Min(sectorSize, Math.Max(0, data.Length - offset));
        var buffer = new byte[sectorSize];
        if (chunkSize > 0)
        {
            Array.Copy(data, offset, buffer, 0, chunkSize);
        }

        var sectorInfo = sectors[startSector + i];
        container.WriteSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector, buffer);
    }
}

static byte[] ReadDumpBytes(string imagePath, string locationText, string lengthText)
{
    var location = ParseDumpLocation(locationText);
    var length = ParseDumpLength(lengthText);
    if (location.Kind == "offset")
    {
        if (length.Kind != "bytes")
        {
            throw new InvalidOperationException("Offset dump length must be specified in bytes.");
        }

        var imageData = File.ReadAllBytes(imagePath);
        if (location.Offset + length.ByteCount > imageData.LongLength)
        {
            throw new InvalidOperationException("Requested dump range is outside the image file.");
        }

        return imageData.AsSpan((int)location.Offset, length.ByteCount).ToArray();
    }

    using var diskService = CreateDiskService();
    using var container = diskService.OpenDisk(imagePath, true);
    var sectors = GetOrderedSectors(container);
    var startIndex = location.Kind switch
    {
        "linear-sector" => location.LinearSector,
        "chs" => FindSectorIndex(sectors, location.Cylinder, location.Head, location.Sector),
        _ => throw new InvalidOperationException("Unsupported dump location.")};
    if (startIndex < 0 || startIndex >= sectors.Count)
    {
        throw new InvalidOperationException("Requested dump location is outside the disk image.");
    }

    if (length.Kind == "sectors")
    {
        return ReadLinearSectors(container, startIndex, length.SectorCount);
    }

    using var stream = new MemoryStream();
    var remainingBytes = length.ByteCount;
    for (var index = startIndex; index < sectors.Count && remainingBytes > 0; index++)
    {
        var sectorInfo = sectors[index];
        var data = container.ReadSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector);
        var copyLength = Math.Min(remainingBytes, data.Length);
        stream.Write(data, 0, copyLength);
        remainingBytes -= copyLength;
    }

    if (remainingBytes > 0)
    {
        throw new InvalidOperationException("Requested dump range is outside the disk image.");
    }

    return stream.ToArray();
}

static List<SectorInfo> GetOrderedSectors(IDiskContainer container)
{
    return container.GetAllSectors().OrderBy(x => x.Cylinder).ThenBy(x => x.Head).ThenBy(x => x.Sector).ToList();
}

static int FindSectorIndex(IReadOnlyList<SectorInfo> sectors, int cylinder, int head, int sector)
{
    for (var i = 0; i < sectors.Count; i++)
    {
        var value = sectors[i];
        if (value.Cylinder == cylinder && value.Head == head && value.Sector == sector)
        {
            return i;
        }
    }

    return -1;
}

static (string Kind, long Offset, int LinearSector, int Cylinder, int Head, int Sector) ParseDumpLocation(string text)
{
    var value = text.Trim();
    if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
    {
        return ("offset", Convert.ToInt64(value, 16), 0, 0, 0, 0);
    }

    if (value.Contains(',', StringComparison.Ordinal))
    {
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            throw new InvalidOperationException($"Unsupported dump location: {text}");
        }

        return ("chs", 0, 0, ParsePrefixedInt(parts[0], "cylinder"), ParsePrefixedInt(parts[1], "side"), ParsePrefixedInt(parts[2], "sector"));
    }

    if (int.TryParse(value, out var linearSector))
    {
        return ("linear-sector", 0, linearSector, 0, 0, 0);
    }

    throw new InvalidOperationException($"Unsupported dump location: {text}");
}

static (string Kind, int ByteCount, int SectorCount) ParseDumpLength(string text)
{
    var value = text.Trim().ToLowerInvariant();
    if (value.EndsWith("sectors", StringComparison.Ordinal))
    {
        return ("sectors", 0, int.Parse(value[..^7]));
    }

    if (value.EndsWith("sector", StringComparison.Ordinal))
    {
        return ("sectors", 0, int.Parse(value[..^6]));
    }

    if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
    {
        return ("bytes", Convert.ToInt32(value, 16), 0);
    }

    return ("bytes", int.Parse(value), 0);
}

static int ParsePrefixedInt(string text, string prefix)
{
    if (!text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Unsupported dump location component: {text}");
    }

    return int.Parse(text[prefix.Length..]);
}

static void PrintHexDump(byte[] bytes)
{
    const int bytesPerLine = 16;
    for (var offset = 0; offset < bytes.Length; offset += bytesPerLine)
    {
        var chunk = bytes.Skip(offset).Take(bytesPerLine).ToArray();
        var hex = string.Join(" ", chunk.Select(x => x.ToString("X2")));
        var ascii = new string (chunk.Select(x => x >= 0x20 && x <= 0x7E ? (char)x : '.').ToArray());
        Console.WriteLine($"{offset:X8}  {hex.PadRight(bytesPerLine * 3 - 1)}  {ascii}");
    }
}

static string ResolveDisplayName(IFileSystem fs, FileEntry file, ArchiveService archiveService, string? encodingOverride)
{
    var fsInfo = fs.GetFileSystemInfo();
    var encoder = archiveService.EncoderRegistry.GetEncoder(encodingOverride ?? fsInfo.DefaultEncodingId);
    var displayName = file.FullName;
    if (encoder != null && file.RawFileName != null)
    {
        string name = encoder.DecodeText(file.RawFileName).TrimEnd(' ');
        string ext = file.RawExtension != null ? encoder.DecodeText(file.RawExtension).TrimEnd(' ') : "";
        displayName = string.IsNullOrEmpty(ext) ? name : $"{name}.{ext}";
    }

    return displayName;
}

static string ReadLayoutInput(string? inputPath, bool fromStdin, IConsoleLocalizer localizer)
{
    if (!string.IsNullOrWhiteSpace(inputPath) && fromStdin)
    {
        throw new InvalidOperationException(localizer.StdinInputConflictMessage);
    }

    if (!string.IsNullOrWhiteSpace(inputPath))
    {
        return File.ReadAllText(inputPath, Encoding.UTF8);
    }

    if (fromStdin)
    {
        return Console.In.ReadToEnd();
    }

    throw new InvalidOperationException(localizer.InputRequiredMessage);
}

static void RenderValidationResult(DirectoryLayoutValidationResult result, bool strict, IConsoleLocalizer localizer)
{
    foreach (var message in result.Messages)
    {
        var level = message.Severity == DirectoryLayoutValidationSeverity.Error ? localizer.ValidationErrorLabel : localizer.ValidationWarningLabel;
        var line = message.LineNumber > 0 ? $"L{message.LineNumber}: " : string.Empty;
        Console.WriteLine($"{level}: {line}{message.Message}");
    }

    Console.WriteLine(string.Format(localizer.ValidationSummaryFormat, result.ErrorCount, result.WarningCount));
    if (result.IsValid && (!strict || result.WarningCount == 0))
    {
        PrintSuccess(localizer, localizer.LayoutValidMessage);
    }
}

static string FormatBootMode(IConsoleLocalizer localizer, BootInfoMode mode)
{
    return mode switch
    {
        BootInfoMode.FileBacked => localizer.BootModeFileBacked,
        BootInfoMode.SectorResident => localizer.BootModeSectorResident,
        _ => localizer.BootModeNone
    };
}

static void PrintSuccess(IConsoleLocalizer localizer, string message)
{
    Console.WriteLine($"{localizer.SuccessPrefix}: {message}");
}

static void PrintError(IConsoleLocalizer localizer, string message)
{
    Console.Error.WriteLine($"{localizer.ErrorPrefix}: {message}");
}

static IFileSystemTransferAdapter? CreateTransferAdapter(IFileSystem fs)
{
    return fs switch
    {
        XDosFileSystem xdos => new XDosTransferAdapter(xdos),
        _ => null
    };
}

static bool ConfirmOverwrite(IConsoleLocalizer localizer, string path)
{
    Console.Write(string.Format(localizer.OverwriteConfirmationMessage, path));
    var input = Console.ReadLine()?.Trim().ToLowerInvariant();
    return input == "y" || input == "yes";
}

static (string Name, bool WasOverwrite) ResolveImageFileTargetName(
    string sourceName,
    string encodingId,
    DiskFileSystemInfo fsInfo,
    HashSet<string> existingNames,
    FileNameNormalizationService normalizationService,
    bool allowOverwriteExactName,
    CliLogSystem? logger)
{
    if (allowOverwriteExactName && CanRepresentRequestedImageFileName(sourceName, encodingId, fsInfo, normalizationService))
    {
        var normalizedSource = normalizationService.Normalize(
            sourceName, encodingId, fsInfo.MaxBaseNameLength, fsInfo.MaxExtensionLength, null);

        if (normalizedSource.Equals(sourceName, StringComparison.OrdinalIgnoreCase))
        {
            return (sourceName, true);
        }

        logger?.Warning(
            $"Image file overwrite is not possible for '{sourceName}' due to filesystem constraints. Using alias generation instead.",
            "image-file-overwrite");
    }

    var normalized = normalizationService.Normalize(
        sourceName, encodingId, fsInfo.MaxBaseNameLength, fsInfo.MaxExtensionLength, existingNames);
    return (normalized, false);
}

static bool TryDeleteExistingFile(IFileSystem fs, string fileName)
{
    try
    {
        if (fs.FileExists(fileName))
        {
            fs.DeleteFile(fileName);
        }

        return true;
    }
    catch (NotSupportedException)
    {
        return false;
    }
    catch (Exception)
    {
        return false;
    }
}

static bool CanRepresentRequestedImageFileName(
    string sourceName,
    string encodingId,
    DiskFileSystemInfo fsInfo,
    FileNameNormalizationService normalizationService)
{
    var encoder = normalizationService is FileNameNormalizationService svc
        ? GetEncoderForNormalization(svc, encodingId)
        : null;
    
    if (encoder == null) return false;

    string basePart;
    string extPart = "";
    
    if (fsInfo.MaxExtensionLength > 0)
    {
        int lastDot = sourceName.LastIndexOf('.');
        if (lastDot > 0)
        {
            basePart = sourceName.Substring(0, lastDot);
            extPart = sourceName.Substring(lastDot + 1);
        }
        else
        {
            basePart = sourceName;
        }
    }
    else
    {
        basePart = sourceName;
    }

    var sanitizedBase = SanitizeForFilesystem(basePart, fsInfo.MaxExtensionLength > 0);
    var sanitizedExt = fsInfo.MaxExtensionLength > 0 ? SanitizeForFilesystem(extPart, false) : "";

    var baseBytes = encoder.EncodeText(sanitizedBase);
    var extBytes = fsInfo.MaxExtensionLength > 0 ? encoder.EncodeText(sanitizedExt) : Array.Empty<byte>();

    if (baseBytes.Length > fsInfo.MaxBaseNameLength)
        return false;
    
    if (fsInfo.MaxExtensionLength > 0 && extBytes.Length > fsInfo.MaxExtensionLength)
        return false;

    var reconstructed = fsInfo.MaxExtensionLength > 0 && !string.IsNullOrEmpty(extPart)
        ? $"{sanitizedBase}.{sanitizedExt}"
        : sanitizedBase;

    return string.Equals(reconstructed, sourceName, StringComparison.OrdinalIgnoreCase);
}

static string SanitizeForFilesystem(string input, bool allowPeriods)
{
    var pattern = allowPeriods
        ? @"[<>:""/\\|?* ]"
        : @"[<>:""/\\|?* .]";
    return System.Text.RegularExpressions.Regex.Replace(input, pattern, "_");
}

static ICharacterEncoder? GetEncoderForNormalization(
    FileNameNormalizationService service, string encodingId)
{
    var field = typeof(FileNameNormalizationService).GetField("_encoderRegistry",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    if (field == null) return null;
    
    var registry = field.GetValue(service) as IEncoderRegistry;
    return registry?.GetEncoder(encodingId);
}
