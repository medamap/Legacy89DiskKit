using System.CommandLine;
using System.Text;
using Legacy89DiskKit.Application.CharacterEncoding;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Application.Services;
using Legacy89DiskKit.Cli.Presentation.FileSystem;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var requestedLanguage = TryGetRequestedLanguage(args);
if (requestedLanguage is { } languageCode && languageCode is not ("ja" or "en"))
{
    Console.Error.WriteLine("Unsupported language. Use 'ja' or 'en'.");
    return 1;
}

var effectiveArgs = RewriteLegacyArgs(args);

var localizer = FileListLocalizer.Create(requestedLanguage);
var archiveService = new ArchiveService();
var huBasicMetadataService = new HuBasicMetadataService();
var directoryLayoutService = new DirectoryLayoutService();
var explicitFileSystemResolver = new ExplicitFileSystemResolver();

var rootCommand = new RootCommand(localizer.RootDescription);

var languageOption = new Option<string?>(new[] { "--language", "-l" }, localizer.LanguageOptionDescription);
var encodingOption = new Option<string?>(new[] { "--encoding", "-e" }, localizer.EncodingOptionDescription);
rootCommand.AddGlobalOption(languageOption);
rootCommand.AddGlobalOption(encodingOption);

var imageArgument = new Argument<string>("image", localizer.ImageArgumentDescription);

var listCommand = new Command("list", localizer.ListCommandDescription);
listCommand.AddArgument(imageArgument);
listCommand.SetHandler((string imagePath, string? encodingOverride) =>
{
    Console.WriteLine($"{localizer.ListingFilesForMessage}: {imagePath}");
    try
    {
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, true);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        var (fsInfo, entries) = BuildFileListEntries(fs, archiveService, encodingOverride);
        var encodingId = encodingOverride ?? fsInfo.DefaultEncodingId;
        Console.WriteLine($"{localizer.UsingEncodingMessage}: {encodingId} (FS Default: {fsInfo.DefaultEncodingId})");

        var formatter = FileListFormatterFactory.Create(fsInfo.FileSystemName);
        var bootRecordInfo = huBasicMetadataService.GetBootRecordInfo(fs);
        var bootSummary = huBasicMetadataService.GetBootSummary(fs);
        var view = formatter.Format(new FileListFormatContext(fsInfo, entries, bootRecordInfo, bootSummary), localizer);
        RenderFileList(view, localizer);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, encodingOption);

var fileCommand = new Command("file", localizer.FileCommandDescription);
var diskFileArgument = new Argument<string>("disk-file", localizer.DiskFileArgumentDescription);
var hostFileArgument = new Argument<string>("host-file", localizer.HostFileArgumentDescription);
var hostPathArgument = new Argument<string>("host-path", localizer.HostPathArgumentDescription);
var sourceNameArgument = new Argument<string>("source", localizer.SourceNameArgumentDescription);
var targetNameArgument = new Argument<string>("target", localizer.TargetNameArgumentDescription);
var newNameArgument = new Argument<string>("new-name", localizer.NewNameArgumentDescription);
var targetFileNameOption = new Option<string?>(new[] { "--target-name", "-n" }, localizer.TargetFileNameOptionDescription);

var fileExtractCommand = new Command("extract", localizer.FileExtractCommandDescription);
fileExtractCommand.AddArgument(imageArgument);
fileExtractCommand.AddArgument(diskFileArgument);
fileExtractCommand.AddArgument(hostPathArgument);
fileExtractCommand.SetHandler((string imagePath, string diskFileName, string hostPath, string? encodingOverride) =>
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

        CreateFileTransferService(fs.GetFileSystemInfo(), encodingOverride).ExportFile(fs, diskFileName, hostPath);
        PrintSuccess(localizer, localizer.FileExtractedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, diskFileArgument, hostPathArgument, encodingOption);

var fileInjectCommand = new Command("inject", localizer.FileInjectCommandDescription);
fileInjectCommand.AddArgument(imageArgument);
fileInjectCommand.AddArgument(hostFileArgument);
fileInjectCommand.AddOption(targetFileNameOption);
fileInjectCommand.SetHandler((string imagePath, string hostFilePath, string? targetName, string? encodingOverride) =>
{
    try
    {
        archiveService.InjectFile(imagePath, hostFilePath, targetName, encodingOverride);
        PrintSuccess(localizer, localizer.FileInjectedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, hostFileArgument, targetFileNameOption, encodingOption);

var fileDeleteCommand = new Command("delete", localizer.FileDeleteCommandDescription);
fileDeleteCommand.AddArgument(imageArgument);
fileDeleteCommand.AddArgument(diskFileArgument);
fileDeleteCommand.SetHandler((string imagePath, string diskFileName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, false);
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
        diskService.OpenDisk(imagePath, false);
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
fileCopyCommand.SetHandler((string imagePath, string sourceName, string targetName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        diskService.OpenDisk(imagePath, false);
        var fs = RequireFileSystem(diskService.FileSystem, localizer);
        if (fs == null)
        {
            return;
        }

        fs.CopyFile(sourceName, targetName);
        PrintSuccess(localizer, localizer.FileCopiedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, sourceNameArgument, targetNameArgument);

fileCommand.AddCommand(fileExtractCommand);
fileCommand.AddCommand(fileInjectCommand);
fileCommand.AddCommand(fileDeleteCommand);
fileCommand.AddCommand(fileRenameCommand);
fileCommand.AddCommand(fileCopyCommand);

var diskCommand = new Command("disk", localizer.DiskCommandDescription);
var diskCreateCommand = new Command("create", localizer.DiskCreateCommandDescription);
var diskTypeOption = new Option<string>(new[] { "--disk-type", "-d" }, localizer.DiskCreateDiskTypeOptionDescription) { IsRequired = true };
var diskFileSystemOption = new Option<string>(new[] { "--file-system", "-f" }, localizer.DiskCreateFileSystemOptionDescription) { IsRequired = true };
var diskNameOption = new Option<string?>(new[] { "--name", "-n" }, localizer.DiskCreateNameOptionDescription);
diskCreateCommand.AddArgument(imageArgument);
diskCreateCommand.AddOption(diskTypeOption);
diskCreateCommand.AddOption(diskFileSystemOption);
diskCreateCommand.AddOption(diskNameOption);
diskCreateCommand.SetHandler((string imagePath, string diskTypeName, string fileSystemName, string? diskName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        var diskType = ParseDiskType(diskTypeName);
        var container = diskService.CreateDisk(imagePath, diskType, diskName ?? string.Empty);
        using var fs = explicitFileSystemResolver.Create(fileSystemName, container);
        fs.Format();
        explicitFileSystemResolver.InitializeForDetection(fs);
        PrintSuccess(localizer, localizer.DiskCreatedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, diskTypeOption, diskFileSystemOption, diskNameOption);

var diskFormatCommand = new Command("format", localizer.DiskFormatCommandDescription);
var explicitFormatFsOption = new Option<string?>(new[] { "--file-system", "-f" }, localizer.DiskFormatFsOptionDescription);
diskFormatCommand.AddArgument(imageArgument);
diskFormatCommand.AddOption(explicitFormatFsOption);
diskFormatCommand.SetHandler((string imagePath, string? explicitFileSystemName) =>
{
    try
    {
        using var diskService = CreateDiskService();
        var container = diskService.OpenDisk(imagePath, false);
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
diskCommand.AddCommand(diskCreateCommand);
diskCommand.AddCommand(diskFormatCommand);

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

        var boot = huBasicMetadataService.GetBootSummary(fs);
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
        diskService.OpenDisk(imagePath, false);
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
        archiveService.CloneBootable(src, dest, files.Split(','));
        PrintSuccess(localizer, localizer.BootableDiskCreatedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, cloneSrcArgument, cloneDestArgument, filesOption);

bootCommand.AddCommand(bootShowCommand);
bootCommand.AddCommand(bootClearCommand);
bootCommand.AddCommand(bootCloneCommand);

var layoutCommand = new Command("layout", localizer.LayoutCommandDescription);
var beforeOption = new Option<string>("--before", localizer.LayoutBeforeOptionDescription) { IsRequired = true };
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
            var displayName = item.Entry != null
                ? ResolveDisplayName(fs, item.Entry, archiveService, encodingOverride)
                : item.DisplayName;
            Console.WriteLine($"{item.Order:D3} [{item.Kind}] {DirectoryLayoutService.CreateStableId(item.Id)} {displayName}");
        }
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Directory layout"))
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
        diskService.OpenDisk(imagePath, false);
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
        diskService.OpenDisk(imagePath, false);
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
        diskService.OpenDisk(imagePath, false);
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
        diskService.OpenDisk(imagePath, false);
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
injectCommand.SetHandler((string imagePath, string hostFilePath, string? targetName, string? encodingOverride) =>
{
    try
    {
        archiveService.InjectFile(imagePath, hostFilePath, targetName, encodingOverride);
        PrintSuccess(localizer, localizer.FileInjectedMessage);
    }
    catch (Exception ex)
    {
        PrintError(localizer, ex.Message);
    }
}, imageArgument, hostFileArgument, targetFileNameOption, encodingOption);

rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(fileCommand);
rootCommand.AddCommand(diskCommand);
rootCommand.AddCommand(bootCommand);
rootCommand.AddCommand(layoutCommand);
rootCommand.AddCommand(injectCommand);

return await rootCommand.InvokeAsync(effectiveArgs);

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

static string[] RewriteLegacyArgs(string[] rawArgs)
{
    if (rawArgs.Length >= 3 &&
        string.Equals(rawArgs[0], "boot", StringComparison.OrdinalIgnoreCase) &&
        !IsBootSubcommand(rawArgs[1]))
    {
        return [rawArgs[0], "clone", .. rawArgs[1..]];
    }

    return rawArgs;
}

static bool IsBootSubcommand(string value)
{
    return value is "show" or "clear" or "clone" or "-h" or "--help";
}

static DiskService CreateDiskService()
{
    var registry = new FileSystemRegistry();
    registry.Register(new HuBasicFileSystemProvider());
    registry.Register(new N88BasicFileSystemProvider());
    registry.Register(new MsxDosFileSystemProvider());
    return new DiskService(fsRegistry: registry);
}

static DiskType ParseDiskType(string diskTypeName)
{
    return diskTypeName.Trim().ToLowerInvariant() switch
    {
        "2d" => DiskType.TwoD,
        "2dd" => DiskType.TwoDD,
        "2hd" => DiskType.TwoHD,
        _ => throw new InvalidOperationException($"Unsupported disk type: {diskTypeName}")
    };
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
    var registry = new EncoderRegistry();
    registry.Register("X1", new X1CharacterEncoder());
    registry.Register("SJIS", new ShiftJisCharacterEncoder());
    registry.Register("Shift-JIS", new ShiftJisCharacterEncoder());
    registry.Register("MSX", new ShiftJisCharacterEncoder());
    registry.Register("PC88", new ShiftJisCharacterEncoder());

    var encoder = registry.GetEncoder(encodingOverride ?? fsInfo.DefaultEncodingId)
        ?? registry.GetEncoder(fsInfo.PlatformId)
        ?? new ShiftJisCharacterEncoder();
    return new FileTransferService(encoder);
}

static void RenderFileList(FileListView view, IFileListLocalizer localizer)
{
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
    Console.WriteLine(string.Join("-+-", widths.Select(width => new string('-', width))));

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

static string FormatRow(IReadOnlyList<string> values, IReadOnlyList<FileListColumn> columns, IReadOnlyList<int> widths)
{
    var cells = new string[values.Count];
    for (int i = 0; i < values.Count; i++)
    {
        cells[i] = columns[i].RightAlign
            ? DisplayWidthUtility.PadLeft(values[i], widths[i])
            : DisplayWidthUtility.PadRight(values[i], widths[i]);
    }

    return string.Join(" | ", cells);
}

static (DiskFileSystemInfo fsInfo, FileListEntryContext[] entries) BuildFileListEntries(
    IFileSystem fs,
    ArchiveService archiveService,
    string? encodingOverride)
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
            actualSize = IsProbablyVirtualLabel(file)
                ? 0
                : fs.ReadFile(file.FullName).Length;
        }

        entries.Add(new FileListEntryContext(file, displayName, displayBaseName, displayExtension, actualSize));
    }

    return (fsInfo, entries.ToArray());
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
    var hasSentinelAddresses = entry.LoadAddress == 0xFFFF &&
                               entry.ExecutionAddress == 0xFFFF &&
                               (entry.EndAddress == 0xFFFF || entry.Size == 0);
    var suspiciousCluster = entry.StartCluster >= 0x7FFF;
    var labelFlags = metadata.HasPassword && metadata.IsWriteProtected && !metadata.IsHidden && !metadata.IsVerify;

    return (looksDecorative || suspiciousCluster || hasSentinelAddresses) &&
           (labelFlags || suspiciousCluster || hasSentinelAddresses);
}

static string ResolveDisplayName(
    IFileSystem fs,
    FileEntry file,
    ArchiveService archiveService,
    string? encodingOverride)
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
        var level = message.Severity == DirectoryLayoutValidationSeverity.Error
            ? localizer.ValidationErrorLabel
            : localizer.ValidationWarningLabel;
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
