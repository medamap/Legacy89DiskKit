using System.Globalization;

namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public interface IFileListLocalizer
{
    string FileSystemLabel { get; }
    string PlatformLabel { get; }
    string FilesLabel { get; }
    string TotalLabel { get; }
    string UsedLabel { get; }
    string FreeLabel { get; }
    string BootLabel { get; }
    string BootFileLabel { get; }
    string BootLoadLabel { get; }
    string BootExecLabel { get; }
    string DirectoryAddressHeader { get; }
    string BodyAddressHeader { get; }
    string BootModeFileBacked { get; }
    string BootModeSectorResident { get; }
    string BootModeNone { get; }
    string NameHeader { get; }
    string AttrHeader { get; }
    string SizeHeader { get; }
    string ClusterHeader { get; }
    string TypeHeader { get; }
    string FlagsHeader { get; }
    string LoadHeader { get; }
    string EndHeader { get; }
    string ExecHeader { get; }
    string NoteHeader { get; }
    string LegendsLabel { get; }
    string FootnotesLabel { get; }
    string HuBasicAsciiNote { get; }
    string HuBasicBasicNote { get; }
    string HuBasicLabelEntryNote { get; }
    string HuBasicFlagPassword { get; }
    string HuBasicFlagHidden { get; }
    string HuBasicFlagVerify { get; }
    string HuBasicFlagWriteProtect { get; }
    string XDosFlagSecret { get; }
    string XDosFlagWriteProtect { get; }
    string XDosFlagSystem { get; }
    string XDosFlagKanji { get; }
    string XDosFlagUserNibble { get; }
}

public interface IConsoleLocalizer : IFileListLocalizer
{
    string LanguageOptionDescription { get; }
    string EncodingOptionDescription { get; }
    string FullHelpOptionDescription { get; }
    string OutputFormatOptionDescription { get; }
    string RootDescription { get; }
    string FullHelpCommandDescription { get; }
    string ListCommandDescription { get; }
    string LayoutCommandDescription { get; }
    string LayoutShowCommandDescription { get; }
    string LayoutMoveCommandDescription { get; }
    string LayoutInsertLabelCommandDescription { get; }
    string LayoutSortCommandDescription { get; }
    string LayoutExportCommandDescription { get; }
    string LayoutValidateCommandDescription { get; }
    string LayoutApplyCommandDescription { get; }
    string FileCommandDescription { get; }
    string FileExtractCommandDescription { get; }
    string FileInjectCommandDescription { get; }
    string FileDeleteCommandDescription { get; }
    string FileRenameCommandDescription { get; }
    string FileCopyCommandDescription { get; }
    string FileCrossCopyCommandDescription { get; }
    string FileInspectorCommandDescription { get; }
    string FileInspectorDetailOptionDescription { get; }
    string DiskCommandDescription { get; }
    string DiskInspectorCommandDescription { get; }
    string DiskInspectorDetailOptionDescription { get; }
    string DiskCreateCommandDescription { get; }
    string DiskFormatCommandDescription { get; }
    string SectorCommandDescription { get; }
    string SectorExportCommandDescription { get; }
    string SectorImportCommandDescription { get; }
    string DiskDumpCommandDescription { get; }
    string HostCommandDescription { get; }
    string HostStdioCommandDescription { get; }
    string HostObservableOptionDescription { get; }
    string HostScriptCommandDescription { get; }
    string HostScriptD88PathCommandDescription { get; }
    string HostScriptD88BufferCommandDescription { get; }
    string HostScriptRawBufferCommandDescription { get; }
    string HostScriptInspectCommandDescription { get; }
    string HostBundleCommandDescription { get; }
    string HostBundleInspectCommandDescription { get; }
    string HostBundleVerifyCommandDescription { get; }
    string HostBundlePackCommandDescription { get; }
    string HostTranscriptCommandDescription { get; }
    string HostTranscriptInspectCommandDescription { get; }
    string HostTranscriptReportCommandDescription { get; }
    string HostTranscriptVerifyCommandDescription { get; }
    string HostOutputArgumentDescription { get; }
    string HostDirectoryArgumentDescription { get; }
    string HostBaseNameArgumentDescription { get; }
    string HostBaselineArgumentDescription { get; }
    string HostTranscriptArgumentDescription { get; }
    string HostRequestScriptOptionDescription { get; }
    string HostOpenModeOptionDescription { get; }
    string HostExchangeModeOptionDescription { get; }
    string BootCommandDescription { get; }
    string BootShowCommandDescription { get; }
    string BootClearCommandDescription { get; }
    string BootCloneCommandDescription { get; }
    string ImageArgumentDescription { get; }
    string SourceImageArgumentDescription { get; }
    string DestinationImageArgumentDescription { get; }
    string DiskFileArgumentDescription { get; }
    string HostFileArgumentDescription { get; }
    string HostPathArgumentDescription { get; }
    string SourceNameArgumentDescription { get; }
    string TargetNameArgumentDescription { get; }
    string NewNameArgumentDescription { get; }
    string LabelTextArgumentDescription { get; }
    string FileCrossCopyFilesArgumentDescription { get; }
    string LayoutInputOptionDescription { get; }
    string LayoutOutputOptionDescription { get; }
    string LayoutStdinOptionDescription { get; }
    string LayoutStrictOptionDescription { get; }
    string LayoutBeforeOptionDescription { get; }
    string LayoutSortByOptionDescription { get; }
    string BootFilesOptionDescription { get; }
    string TargetFileNameOptionDescription { get; }
    string TabModeOptionDescription { get; }
    string TabWidthOptionDescription { get; }
    string TruncateTextOnOverflowOptionDescription { get; }
    string DiskCreateImageFormatOptionDescription { get; }
    string DiskCreateDiskTypeOptionDescription { get; }
    string DiskCreateFileSystemOptionDescription { get; }
    string DiskCreateNameOptionDescription { get; }
    string DiskFormatFsOptionDescription { get; }
    string ExplicitFileSystemOptionDescription { get; }
    string SectorLocationArgumentDescription { get; }
    string SectorCountArgumentDescription { get; }
    string DumpLocationArgumentDescription { get; }
    string DumpLengthArgumentDescription { get; }
    string DiskSectorCopyCommandDescription { get; }
    string DiskSectorCopyForceOptionDescription { get; }
    string ListingFilesForMessage { get; }
    string UsingEncodingMessage { get; }
    string SuccessPrefix { get; }
    string ErrorPrefix { get; }
    string FileInjectedMessage { get; }
    string FileExtractedMessage { get; }
    string FileDeletedMessage { get; }
    string FileRenamedMessage { get; }
    string FileCopiedMessage { get; }
    string DiskCreatedMessage { get; }
    string DiskFormattedMessage { get; }
    string DiskSectorCopiedMessage { get; }
    string LayoutUpdatedMessage { get; }
    string LabelInsertedMessage { get; }
    string DirectoryEntriesSortedMessage { get; }
    string LayoutValidMessage { get; }
    string LayoutAppliedMessage { get; }
    string BootableDiskCreatedMessage { get; }
    string BootClearedMessage { get; }
    string UnsupportedLanguageMessage { get; }
    string FileSystemNotDetectedMessage { get; }
    string LayoutNotSupportedMessage { get; }
    string StdinInputConflictMessage { get; }
    string InputRequiredMessage { get; }
    string ValidationSummaryFormat { get; }
    string ValidationErrorLabel { get; }
    string ValidationWarningLabel { get; }
    string BootShowTitle { get; }
    string OverwriteConfirmationMessage { get; }
    string BootExportCommandDescription { get; }
    string BootExportOutputOptionDescription { get; }
    string BootEntriesExportedMessage { get; }
    string BootNoEntriesFoundMessage { get; }
    string BootImportCommandDescription { get; }
    string BootImportBinaryOptionDescription { get; }
    string BootImportMetadataOptionDescription { get; }
    string BootEntryImportedMessage { get; }
    string CheckUpdateCommandDescription { get; }
    string VersionCommandDescription { get; }
    string CheckUpdateCurrentVersionLabel { get; }
    string CheckUpdateLatestVersionLabel { get; }
    string CheckUpdateReleaseUrlLabel { get; }
    string CheckUpdateWindowsMsiLabel { get; }
    string CheckUpdateAvailableMessage { get; }
    string CheckUpdateUpToDateMessage { get; }
    string MultiSlotD88WriteNotSupportedMessage { get; }
    string ContainerLabel { get; }
    string DiskTypeLabel { get; }
    string MachineProfileLabel { get; }
    string GeometryLabel { get; }
    string ImageSizeLabel { get; }
    string WriteProtectedLabel { get; }
    string EncodingLabel { get; }
    string FullHelpFooter { get; }
    string ImageFileOverwriteOptionDescription { get; }
    string LogOptionDescription { get; }
    string LogOptionWithPathDescription { get; }
    string ImageFileOverwriteIgnoredWarning { get; }
}

public static class FileListLocalizer
{
    public static IConsoleLocalizer CreateCurrent()
    {
        return Create(null);
    }

    public static IConsoleLocalizer Create(string? language)
    {
        var code = language?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(code))
        {
            code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        }

        return code switch
        {
            "ja" => new JapaneseConsoleLocalizer(),
            "en" => new EnglishConsoleLocalizer(),
            _ => throw new InvalidOperationException($"Unsupported language: {language}")
        };
    }

    private abstract class ConsoleLocalizerBase : IConsoleLocalizer
    {
        public abstract string FileSystemLabel { get; }
        public abstract string PlatformLabel { get; }
        public abstract string FilesLabel { get; }
        public abstract string TotalLabel { get; }
        public abstract string UsedLabel { get; }
        public abstract string FreeLabel { get; }
        public abstract string BootLabel { get; }
        public abstract string BootFileLabel { get; }
        public abstract string BootLoadLabel { get; }
        public abstract string BootExecLabel { get; }
        public abstract string DirectoryAddressHeader { get; }
        public abstract string BodyAddressHeader { get; }
        public abstract string NameHeader { get; }
        public abstract string AttrHeader { get; }
        public abstract string SizeHeader { get; }
        public abstract string ClusterHeader { get; }
        public abstract string TypeHeader { get; }
        public abstract string FlagsHeader { get; }
        public abstract string LoadHeader { get; }
        public abstract string EndHeader { get; }
        public abstract string ExecHeader { get; }
        public abstract string NoteHeader { get; }
        public abstract string LegendsLabel { get; }
        public abstract string FootnotesLabel { get; }
        public abstract string HuBasicAsciiNote { get; }
        public abstract string HuBasicBasicNote { get; }
        public abstract string HuBasicLabelEntryNote { get; }
        public abstract string HuBasicFlagPassword { get; }
        public abstract string HuBasicFlagHidden { get; }
        public abstract string HuBasicFlagVerify { get; }
        public abstract string HuBasicFlagWriteProtect { get; }
        public abstract string XDosFlagSecret { get; }
        public abstract string XDosFlagWriteProtect { get; }
        public abstract string XDosFlagSystem { get; }
        public abstract string XDosFlagKanji { get; }
        public abstract string XDosFlagUserNibble { get; }
        public abstract string LanguageOptionDescription { get; }
        public abstract string EncodingOptionDescription { get; }
        public abstract string FullHelpOptionDescription { get; }
        public abstract string OutputFormatOptionDescription { get; }
        public abstract string RootDescription { get; }
        public abstract string FullHelpCommandDescription { get; }
        public abstract string ListCommandDescription { get; }
        public abstract string LayoutCommandDescription { get; }
        public abstract string LayoutShowCommandDescription { get; }
        public abstract string LayoutMoveCommandDescription { get; }
        public abstract string LayoutInsertLabelCommandDescription { get; }
        public abstract string LayoutSortCommandDescription { get; }
        public abstract string LayoutExportCommandDescription { get; }
        public abstract string LayoutValidateCommandDescription { get; }
        public abstract string LayoutApplyCommandDescription { get; }
        public abstract string FileCommandDescription { get; }
        public abstract string FileExtractCommandDescription { get; }
        public abstract string FileInjectCommandDescription { get; }
        public abstract string FileDeleteCommandDescription { get; }
        public abstract string FileRenameCommandDescription { get; }
        public abstract string FileCopyCommandDescription { get; }
        public abstract string FileCrossCopyCommandDescription { get; }
        public abstract string FileInspectorCommandDescription { get; }
        public abstract string FileInspectorDetailOptionDescription { get; }
        public abstract string DiskCommandDescription { get; }
        public abstract string DiskInspectorCommandDescription { get; }
        public abstract string DiskInspectorDetailOptionDescription { get; }
        public abstract string DiskCreateCommandDescription { get; }
        public abstract string DiskFormatCommandDescription { get; }
        public abstract string SectorCommandDescription { get; }
        public abstract string SectorExportCommandDescription { get; }
        public abstract string SectorImportCommandDescription { get; }
        public abstract string DiskDumpCommandDescription { get; }
        public abstract string HostCommandDescription { get; }
        public abstract string HostStdioCommandDescription { get; }
        public abstract string HostObservableOptionDescription { get; }
        public abstract string HostScriptCommandDescription { get; }
        public abstract string HostScriptD88PathCommandDescription { get; }
        public abstract string HostScriptD88BufferCommandDescription { get; }
        public abstract string HostScriptRawBufferCommandDescription { get; }
        public abstract string HostScriptInspectCommandDescription { get; }
        public abstract string HostBundleCommandDescription { get; }
        public abstract string HostBundleInspectCommandDescription { get; }
        public abstract string HostBundleVerifyCommandDescription { get; }
        public abstract string HostBundlePackCommandDescription { get; }
        public abstract string HostTranscriptCommandDescription { get; }
        public abstract string HostTranscriptInspectCommandDescription { get; }
        public abstract string HostTranscriptReportCommandDescription { get; }
        public abstract string HostTranscriptVerifyCommandDescription { get; }
        public abstract string HostOutputArgumentDescription { get; }
        public abstract string HostDirectoryArgumentDescription { get; }
        public abstract string HostBaseNameArgumentDescription { get; }
        public abstract string HostBaselineArgumentDescription { get; }
        public abstract string HostTranscriptArgumentDescription { get; }
        public abstract string HostRequestScriptOptionDescription { get; }
        public abstract string HostOpenModeOptionDescription { get; }
        public abstract string HostExchangeModeOptionDescription { get; }
        public abstract string BootCommandDescription { get; }
        public abstract string BootShowCommandDescription { get; }
        public abstract string BootClearCommandDescription { get; }
        public abstract string BootCloneCommandDescription { get; }
        public abstract string ImageArgumentDescription { get; }
        public abstract string SourceImageArgumentDescription { get; }
        public abstract string DestinationImageArgumentDescription { get; }
        public abstract string DiskFileArgumentDescription { get; }
        public abstract string HostFileArgumentDescription { get; }
        public abstract string HostPathArgumentDescription { get; }
        public abstract string SourceNameArgumentDescription { get; }
        public abstract string TargetNameArgumentDescription { get; }
        public abstract string NewNameArgumentDescription { get; }
        public abstract string LabelTextArgumentDescription { get; }
        public abstract string FileCrossCopyFilesArgumentDescription { get; }
        public abstract string LayoutInputOptionDescription { get; }
        public abstract string LayoutOutputOptionDescription { get; }
        public abstract string LayoutStdinOptionDescription { get; }
        public abstract string LayoutStrictOptionDescription { get; }
        public abstract string LayoutBeforeOptionDescription { get; }
        public abstract string LayoutSortByOptionDescription { get; }
        public abstract string BootFilesOptionDescription { get; }
        public abstract string TargetFileNameOptionDescription { get; }
        public abstract string TabModeOptionDescription { get; }
        public abstract string TabWidthOptionDescription { get; }
        public abstract string TruncateTextOnOverflowOptionDescription { get; }
        public abstract string DiskCreateImageFormatOptionDescription { get; }
        public abstract string DiskCreateDiskTypeOptionDescription { get; }
        public abstract string DiskCreateFileSystemOptionDescription { get; }
        public abstract string DiskCreateNameOptionDescription { get; }
        public abstract string DiskFormatFsOptionDescription { get; }
        public abstract string ExplicitFileSystemOptionDescription { get; }
        public abstract string SectorLocationArgumentDescription { get; }
        public abstract string SectorCountArgumentDescription { get; }
        public abstract string DumpLocationArgumentDescription { get; }
        public abstract string DumpLengthArgumentDescription { get; }
        public abstract string DiskSectorCopyCommandDescription { get; }
        public abstract string DiskSectorCopyForceOptionDescription { get; }
        public abstract string ListingFilesForMessage { get; }
        public abstract string UsingEncodingMessage { get; }
        public abstract string SuccessPrefix { get; }
        public abstract string ErrorPrefix { get; }
        public abstract string FileInjectedMessage { get; }
        public abstract string FileExtractedMessage { get; }
        public abstract string FileDeletedMessage { get; }
        public abstract string FileRenamedMessage { get; }
        public abstract string FileCopiedMessage { get; }
        public abstract string DiskCreatedMessage { get; }
        public abstract string DiskFormattedMessage { get; }
        public abstract string DiskSectorCopiedMessage { get; }
        public abstract string LayoutUpdatedMessage { get; }
        public abstract string LabelInsertedMessage { get; }
        public abstract string DirectoryEntriesSortedMessage { get; }
        public abstract string LayoutValidMessage { get; }
        public abstract string LayoutAppliedMessage { get; }
        public abstract string BootableDiskCreatedMessage { get; }
        public abstract string BootClearedMessage { get; }
        public abstract string UnsupportedLanguageMessage { get; }
        public abstract string FileSystemNotDetectedMessage { get; }
        public abstract string LayoutNotSupportedMessage { get; }
        public abstract string StdinInputConflictMessage { get; }
        public abstract string InputRequiredMessage { get; }
        public abstract string ValidationSummaryFormat { get; }
        public abstract string ValidationErrorLabel { get; }
        public abstract string ValidationWarningLabel { get; }
        public abstract string BootModeFileBacked { get; }
        public abstract string BootModeSectorResident { get; }
        public abstract string BootModeNone { get; }
        public abstract string BootShowTitle { get; }
        public abstract string OverwriteConfirmationMessage { get; }
        public abstract string BootExportCommandDescription { get; }
        public abstract string BootExportOutputOptionDescription { get; }
        public abstract string BootEntriesExportedMessage { get; }
        public abstract string BootNoEntriesFoundMessage { get; }
        public abstract string BootImportCommandDescription { get; }
        public abstract string BootImportBinaryOptionDescription { get; }
        public abstract string BootImportMetadataOptionDescription { get; }
        public abstract string BootEntryImportedMessage { get; }
        public abstract string CheckUpdateCommandDescription { get; }
        public abstract string VersionCommandDescription { get; }
        public abstract string CheckUpdateCurrentVersionLabel { get; }
        public abstract string CheckUpdateLatestVersionLabel { get; }
        public abstract string CheckUpdateReleaseUrlLabel { get; }
        public abstract string CheckUpdateWindowsMsiLabel { get; }
        public abstract string CheckUpdateAvailableMessage { get; }
        public abstract string CheckUpdateUpToDateMessage { get; }
        public abstract string MultiSlotD88WriteNotSupportedMessage { get; }
        public abstract string ContainerLabel { get; }
        public abstract string DiskTypeLabel { get; }
        public abstract string MachineProfileLabel { get; }
        public abstract string GeometryLabel { get; }
        public abstract string ImageSizeLabel { get; }
        public abstract string WriteProtectedLabel { get; }
        public abstract string EncodingLabel { get; }
        public abstract string FullHelpFooter { get; }
        public abstract string ImageFileOverwriteOptionDescription { get; }
        public abstract string LogOptionDescription { get; }
        public abstract string LogOptionWithPathDescription { get; }
        public abstract string ImageFileOverwriteIgnoredWarning { get; }
    }

    private sealed class EnglishConsoleLocalizer : ConsoleLocalizerBase
    {
        public override string FileSystemLabel => "File System";
        public override string PlatformLabel => "Platform";
        public override string FilesLabel => "Files";
        public override string TotalLabel => "Total";
        public override string UsedLabel => "Used";
        public override string FreeLabel => "Free";
        public override string BootLabel => "Boot";
        public override string BootFileLabel => "Boot File";
        public override string BootLoadLabel => "Boot Load";
        public override string BootExecLabel => "Boot Exec";
        public override string DirectoryAddressHeader => "DIR-ADR";
        public override string BodyAddressHeader => "BDY-ADR";
        public override string NameHeader => "Name";
        public override string AttrHeader => "Attr";
        public override string SizeHeader => "Size";
        public override string ClusterHeader => "Cluster";
        public override string TypeHeader => "Type";
        public override string FlagsHeader => "Flags";
        public override string LoadHeader => "Load";
        public override string EndHeader => "End";
        public override string ExecHeader => "Exec";
        public override string NoteHeader => "Note";
        public override string LegendsLabel => "Legends";
        public override string FootnotesLabel => "Footnotes";
        public override string HuBasicAsciiNote => "ASCII files may use EOF-based logical length; displayed size or address range can differ from raw directory metadata.";
        public override string HuBasicBasicNote => "BASIC files may use load/end/exec metadata differently from machine-language files.";
        public override string HuBasicLabelEntryNote => "This entry may be a non-data label used as a separator or heading in the directory listing.";
        public override string HuBasicFlagPassword => "Password";
        public override string HuBasicFlagHidden => "Hidden";
        public override string HuBasicFlagVerify => "Verify";
        public override string HuBasicFlagWriteProtect => "Write-protect";
        public override string XDosFlagSecret => "Secret";
        public override string XDosFlagWriteProtect => "Write-protect";
        public override string XDosFlagSystem => "System";
        public override string XDosFlagKanji => "Kanji";
        public override string XDosFlagUserNibble => "User attribute nibble (bits 0-3)";
        public override string LanguageOptionDescription => "Override UI language: ja or en";
        public override string EncodingOptionDescription => "Override disk filename decoding and text I/O encoding (accepted examples: X1, SJIS, Shift-JIS, ShiftJIS, shift_jis)";
        public override string FullHelpOptionDescription => "Print the complete command reference";
        public override string OutputFormatOptionDescription => "Output format: table or csv";
        public override string RootDescription => "Legacy89DiskKit CLI. Use --full-help for the complete command reference.";
        public override string FullHelpCommandDescription => "Print the complete command reference";
        public override string ListCommandDescription => "List files and disk summary information";
        public override string LayoutCommandDescription => "Inspect and edit directory entry layout. Use 'layout export IMAGE > plan.txt', then 'cat plan.txt | layout validate IMAGE --stdin' or '... apply IMAGE --stdin'.";
        public override string LayoutShowCommandDescription => "Show the current directory entry order";
        public override string LayoutMoveCommandDescription => "Move an entry before another entry";
        public override string LayoutInsertLabelCommandDescription => "Insert a label-like directory entry";
        public override string LayoutSortCommandDescription => "Sort directory entries while preserving label positions";
        public override string LayoutExportCommandDescription => "Export the current layout as an editable text plan. Writes to stdout unless --output is specified.";
        public override string LayoutValidateCommandDescription => "Validate a layout text plan without writing changes. Use --stdin to read from standard input.";
        public override string LayoutApplyCommandDescription => "Apply a validated layout text plan. Use --stdin to read from standard input.";
        public override string FileCommandDescription => "File operations on an existing disk image";
        public override string FileExtractCommandDescription => "Extract one disk file to a host path";
        public override string FileInjectCommandDescription => "Inject a host file into a disk image";
        public override string FileDeleteCommandDescription => "Delete one disk file";
        public override string FileRenameCommandDescription => "Rename one disk file";
        public override string FileCopyCommandDescription => "Duplicate a file inside the same disk image";
        public override string FileCrossCopyCommandDescription => "Copy files between different disk images with filename auto-shortening";
        public override string FileInspectorCommandDescription => "Inspect one file in detail";
        public override string FileInspectorDetailOptionDescription => "Detail level: short, normal, or full";
        public override string DiskCommandDescription => "Disk-level operations";
        public override string DiskInspectorCommandDescription => "Inspect disk/container, file system, and boot metadata";
        public override string DiskInspectorDetailOptionDescription => "Detail level: short, normal, or full";
        public override string DiskCreateCommandDescription => "Create a new disk image. When --file-system is omitted, it remains unformatted";
        public override string DiskFormatCommandDescription => "Reinitialize an existing disk image, preferably with an explicit file system";
        public override string SectorCommandDescription => "Sector-level import/export operations";
        public override string SectorExportCommandDescription => "Export sectors from a disk image to a host file";
        public override string SectorImportCommandDescription => "Import host bytes into sectors of a disk image";
        public override string DiskDumpCommandDescription => "Dump bytes or sectors from a disk image";
        public override string HostCommandDescription => "External host integration operations";
        public override string HostStdioCommandDescription => "Run the emulator host protocol over standard input/output";
        public override string HostObservableOptionDescription => "Emit notification-aware exchanges that include IRQ, DRQ, and advance-request notifications";
        public override string HostScriptCommandDescription => "Generate reusable request scripts for external host bridges";
        public override string HostScriptD88PathCommandDescription => "Write a read-only D88-by-path request script";
        public override string HostScriptD88BufferCommandDescription => "Write a read-only D88-by-buffer request script";
        public override string HostScriptRawBufferCommandDescription => "Write a read-only raw-sector-image-by-buffer request script";
        public override string HostScriptInspectCommandDescription => "Read a request script and print its summary";
        public override string HostBundleCommandDescription => "Inspect portable host-proof bundles";
        public override string HostBundleInspectCommandDescription => "Read a host-proof bundle and print its summary";
        public override string HostBundleVerifyCommandDescription => "Validate a host-proof bundle against a built-in baseline";
        public override string HostBundlePackCommandDescription => "Pack a transcript and optional request script into a host-proof bundle";
        public override string HostTranscriptCommandDescription => "Inspect and validate raw host-proof transcripts";
        public override string HostTranscriptInspectCommandDescription => "Read a transcript and print its proof summary";
        public override string HostTranscriptReportCommandDescription => "Render a markdown proof report from a transcript";
        public override string HostTranscriptVerifyCommandDescription => "Validate a transcript against a built-in baseline";
        public override string HostOutputArgumentDescription => "Output file path";
        public override string HostDirectoryArgumentDescription => "Bundle directory path";
        public override string HostBaseNameArgumentDescription => "Bundle base name";
        public override string HostBaselineArgumentDescription => "Baseline name: event-d88 or event-raw";
        public override string HostTranscriptArgumentDescription => "Transcript file path";
        public override string HostRequestScriptOptionDescription => "Optional request script file path";
        public override string HostOpenModeOptionDescription => "Open mode label to record in the bundle";
        public override string HostExchangeModeOptionDescription => "Exchange mode label to record in the bundle";
        public override string BootCommandDescription => "Boot metadata operations";
        public override string BootShowCommandDescription => "Show boot metadata for this disk";
        public override string BootClearCommandDescription => "Clear file-backed boot metadata without erasing the whole boot sector";
        public override string BootCloneCommandDescription => "Create a bootable clone of a disk image";
        public override string ImageArgumentDescription => "Path to the disk image";
        public override string SourceImageArgumentDescription => "Source disk image path";
        public override string DestinationImageArgumentDescription => "Destination disk image path";
        public override string DiskFileArgumentDescription => "File name stored on disk";
        public override string HostFileArgumentDescription => "Host file path";
        public override string HostPathArgumentDescription => "Destination host path";
        public override string SourceNameArgumentDescription => "Existing disk file name";
        public override string TargetNameArgumentDescription => "New or target disk file name";
        public override string NewNameArgumentDescription => "New disk file name";
        public override string LabelTextArgumentDescription => "Label text";
        public override string FileCrossCopyFilesArgumentDescription => "Files to copy (comma separated, or 'all')";
        public override string LayoutInputOptionDescription => "Read the layout plan from a file";
        public override string LayoutOutputOptionDescription => "Write the exported layout plan to a file";
        public override string LayoutStdinOptionDescription => "Read the layout plan from standard input";
        public override string LayoutStrictOptionDescription => "Treat warnings as errors";
        public override string LayoutBeforeOptionDescription => "Entry name to place before";
        public override string LayoutSortByOptionDescription => "Sort key: name, ext, type";
        public override string BootFilesOptionDescription => "Comma-separated list of files to copy or 'all'";
        public override string TargetFileNameOptionDescription => "Override the filename on the target disk";
        public override string TabModeOptionDescription => "Tab handling for plain text files: keep, spaces, or remove";
        public override string TabWidthOptionDescription => "Tab stop width when --tab-mode spaces is used";
        public override string TruncateTextOnOverflowOptionDescription => "When tab expansion exceeds the filesystem text limit, truncate instead of failing";
        public override string DiskCreateImageFormatOptionDescription => "Container/image format: d88, d77, 2d, or dsk (default: d88)";
        public override string DiskCreateDiskTypeOptionDescription => "Disk media type: 2d, 2dd, or 2hd (default: 2d)";
        public override string DiskCreateFileSystemOptionDescription => "Optional file system to initialize: hu-basic, n88-basic, msx-dos, or xdos. When omitted, the disk remains unformatted";
        public override string DiskCreateNameOptionDescription => "Optional disk name for image containers that support it";
        public override string DiskFormatFsOptionDescription => "Explicit file system to format: hu-basic, n88-basic, msx-dos, or xdos";
        public override string ExplicitFileSystemOptionDescription => "Explicit file system to use: hu-basic, n88-basic, msx-dos, or xdos";
        public override string SectorLocationArgumentDescription => "Starting linear sector number";
        public override string SectorCountArgumentDescription => "Number of sectors";
        public override string DumpLocationArgumentDescription => "Location: offset (0x...), linear sector, or cylinderN,sideN,sectorN";
        public override string DumpLengthArgumentDescription => "Length in bytes, or '<N>sector' to dump sectors";
        public override string DiskSectorCopyCommandDescription => "Perform a sector-by-sector physical copy between disk images";
        public override string DiskSectorCopyForceOptionDescription => "Skip overwrite confirmation";
        public override string ListingFilesForMessage => "Listing files for";
        public override string UsingEncodingMessage => "Using Encoding";
        public override string SuccessPrefix => "Success";
        public override string ErrorPrefix => "Error";
        public override string FileInjectedMessage => "File injected.";
        public override string FileExtractedMessage => "File extracted.";
        public override string FileDeletedMessage => "File deleted.";
        public override string FileRenamedMessage => "File renamed.";
        public override string FileCopiedMessage => "File copied.";
        public override string DiskCreatedMessage => "Disk created.";
        public override string DiskFormattedMessage => "Disk formatted.";
        public override string DiskSectorCopiedMessage => "Disk sector copy completed. {0} tracks copied, {1} sectors skipped.";
        public override string LayoutUpdatedMessage => "Directory layout updated.";
        public override string LabelInsertedMessage => "Label entry inserted.";
        public override string DirectoryEntriesSortedMessage => "Directory entries sorted.";
        public override string LayoutValidMessage => "Layout plan is valid.";
        public override string LayoutAppliedMessage => "Layout plan applied.";
        public override string BootableDiskCreatedMessage => "Bootable disk created.";
        public override string BootClearedMessage => "Boot metadata cleared.";
        public override string UnsupportedLanguageMessage => "Unsupported language. Use 'ja' or 'en'.";
        public override string FileSystemNotDetectedMessage => "Could not detect a supported file system on this disk.";
        public override string LayoutNotSupportedMessage => "Directory layout is not supported for this file system.";
        public override string StdinInputConflictMessage => "Specify either --input or --stdin, not both.";
        public override string InputRequiredMessage => "Specify --input or --stdin.";
        public override string ValidationSummaryFormat => "{0} errors, {1} warnings";
        public override string ValidationErrorLabel => "ERROR";
        public override string ValidationWarningLabel => "WARNING";
        public override string BootModeFileBacked => "file-backed";
        public override string BootModeSectorResident => "sector-resident";
        public override string BootModeNone => "none";
        public override string BootShowTitle => "Boot Information";
        public override string OverwriteConfirmationMessage => "File '{0}' already exists. Overwrite? (y/N): ";
        public override string BootExportCommandDescription => "Export boot entries as binary payloads and JSON sidecars";
        public override string BootExportOutputOptionDescription => "Directory to write exported files to";
        public override string BootEntriesExportedMessage => "Boot entries exported to: {0}";
        public override string BootNoEntriesFoundMessage => "No exportable boot entries found on this disk.";
        public override string BootImportCommandDescription => "Import a boot entry payload and its JSON sidecar metadata to the disk";
        public override string BootImportBinaryOptionDescription => "Path to the .bin payload file";
        public override string BootImportMetadataOptionDescription => "Path to the .json sidecar metadata file";
        public override string BootEntryImportedMessage => "Boot entry successfully imported.";
        public override string CheckUpdateCommandDescription => "Check GitHub Releases for a newer public version";
        public override string VersionCommandDescription => "Show the CLI version";
        public override string CheckUpdateCurrentVersionLabel => "Current version";
        public override string CheckUpdateLatestVersionLabel => "Latest version";
        public override string CheckUpdateReleaseUrlLabel => "Release URL";
        public override string CheckUpdateWindowsMsiLabel => "Windows MSI";
        public override string CheckUpdateAvailableMessage => "An update is available.";
        public override string CheckUpdateUpToDateMessage => "You are using the latest version.";
        public override string MultiSlotD88WriteNotSupportedMessage => "Write operations for multi-slot D88 containers are not supported yet. Split the container into a single-slot image and try again. Read operations currently target only the first slot.";
        public override string ContainerLabel => "Container";
        public override string DiskTypeLabel => "Disk Type";
        public override string MachineProfileLabel => "Machine Profile";
        public override string GeometryLabel => "Geometry";
        public override string ImageSizeLabel => "Image Size";
        public override string WriteProtectedLabel => "Write Protected";
        public override string EncodingLabel => "Encoding";
        public override string FullHelpFooter => "Use '<command> --help' for detailed help on one command.";
        public override string ImageFileOverwriteOptionDescription => "Allow overwriting an existing file with the same name instead of generating a numbered alias";
        public override string LogOptionDescription => "Write logs to the default system log location (OS-specific, dated, 7-day rotation)";
        public override string LogOptionWithPathDescription => "Write logs to the specified file (appends if the file exists)";
        public override string ImageFileOverwriteIgnoredWarning => "Image file overwrite is not possible for this filename due to filesystem constraints. Using alias generation instead.";
    }

    private sealed class JapaneseConsoleLocalizer : ConsoleLocalizerBase
    {
        public override string FileSystemLabel => "ファイルシステム";
        public override string PlatformLabel => "プラットフォーム";
        public override string FilesLabel => "ファイル数";
        public override string TotalLabel => "総容量";
        public override string UsedLabel => "使用量";
        public override string FreeLabel => "空き容量";
        public override string BootLabel => "ブート";
        public override string BootFileLabel => "ブートファイル";
        public override string BootLoadLabel => "ブートLoad";
        public override string BootExecLabel => "ブートExec";
        public override string DirectoryAddressHeader => "DIR-ADR";
        public override string BodyAddressHeader => "BDY-ADR";
        public override string NameHeader => "名前";
        public override string AttrHeader => "属性";
        public override string SizeHeader => "サイズ";
        public override string ClusterHeader => "クラスタ";
        public override string TypeHeader => "種別";
        public override string FlagsHeader => "フラグ";
        public override string LoadHeader => "Load";
        public override string EndHeader => "End";
        public override string ExecHeader => "Exec";
        public override string NoteHeader => "注記";
        public override string LegendsLabel => "凡例";
        public override string FootnotesLabel => "注釈";
        public override string HuBasicAsciiNote => "ASCII ファイルは EOF ベースで論理長が決まる場合があり、表示サイズやアドレス範囲がディレクトリ情報と一致しないことがあります。";
        public override string HuBasicBasicNote => "BASIC ファイルの Load/End/Exec はマシン語ファイルほど強い意味を持たない場合があります。";
        public override string HuBasicLabelEntryNote => "このエントリは実体のないラベル用途で、区切りや見出しのために使われている可能性があります。";
        public override string HuBasicFlagPassword => "パスワード";
        public override string HuBasicFlagHidden => "隠し";
        public override string HuBasicFlagVerify => "ベリファイ";
        public override string HuBasicFlagWriteProtect => "書き込み保護";
        public override string XDosFlagSecret => "シークレット";
        public override string XDosFlagWriteProtect => "書き込み保護";
        public override string XDosFlagSystem => "システム";
        public override string XDosFlagKanji => "漢字";
        public override string XDosFlagUserNibble => "ユーザー属性ニブル (bit 0-3)";
        public override string LanguageOptionDescription => "UI 表示言語を指定します: ja または en";
        public override string EncodingOptionDescription => "ディスク上ファイル名の表示デコードやテキスト入出力の文字エンコーディングを上書きします（指定例: X1, SJIS, Shift-JIS, ShiftJIS, shift_jis）";
        public override string FullHelpOptionDescription => "全コマンドのヘルプをまとめて表示します";
        public override string OutputFormatOptionDescription => "出力形式: table または csv";
        public override string RootDescription => "Legacy89DiskKit CLI。--full-help で全コマンドのヘルプを表示できます。";
        public override string FullHelpCommandDescription => "全コマンドのヘルプをまとめて表示します";
        public override string ListCommandDescription => "ファイル一覧とディスク概要を表示します";
        public override string LayoutCommandDescription => "ディレクトリエントリ順を確認・編集します。'layout export IMAGE > plan.txt' のあと、'cat plan.txt | layout validate IMAGE --stdin' や '... apply IMAGE --stdin' で流し込めます";
        public override string LayoutShowCommandDescription => "現在のディレクトリエントリ順を表示します";
        public override string LayoutMoveCommandDescription => "指定エントリを別のエントリの前へ移動します";
        public override string LayoutInsertLabelCommandDescription => "ラベル風のディレクトリエントリを挿入します";
        public override string LayoutSortCommandDescription => "ラベル位置を維持したままディレクトリエントリをソートします";
        public override string LayoutExportCommandDescription => "現在の並び順を編集用テキストとして出力します。--output 未指定時は標準出力へ出します";
        public override string LayoutValidateCommandDescription => "レイアウト計画テキストを検証します。--stdin で標準入力から読めます";
        public override string LayoutApplyCommandDescription => "検証済みレイアウト計画を適用します。--stdin で標準入力から読めます";
        public override string FileCommandDescription => "既存ディスクイメージ上のファイル操作";
        public override string FileExtractCommandDescription => "ディスク上のファイルをホストへ書き出します";
        public override string FileInjectCommandDescription => "ホストファイルをディスクへ注入します";
        public override string FileDeleteCommandDescription => "ディスク上のファイルを削除します";
        public override string FileRenameCommandDescription => "ディスク上のファイル名を変更します";
        public override string FileCopyCommandDescription => "同一ディスク内でファイルを複製します";
        public override string FileCrossCopyCommandDescription => "異なるディスクイメージ間でファイルをコピーします（ファイル名自動短縮機能付き）";
        public override string FileInspectorCommandDescription => "単一ファイルの詳細情報を確認します";
        public override string FileInspectorDetailOptionDescription => "詳細レベル: short, normal, full";
        public override string DiskCommandDescription => "ディスク単位の操作";
        public override string DiskInspectorCommandDescription => "コンテナ、ファイルシステム、ブート情報をまとめて確認します";
        public override string DiskInspectorDetailOptionDescription => "詳細レベル: short, normal, full";
        public override string DiskCreateCommandDescription => "新しいディスクイメージを作成します。--file-system を省略した場合は未フォーマットのままです";
        public override string DiskFormatCommandDescription => "既存ディスクイメージを再初期化します。明示的なファイルシステム指定を推奨します";
        public override string SectorCommandDescription => "セクタ単位の入出力";
        public override string SectorExportCommandDescription => "ディスクイメージからセクタをホストファイルへ出力します";
        public override string SectorImportCommandDescription => "ホストファイルの内容をディスクイメージのセクタへ書き戻します";
        public override string DiskDumpCommandDescription => "ディスクイメージからバイト列やセクタ列をダンプ表示します";
        public override string HostCommandDescription => "外部ホスト連携の操作";
        public override string HostStdioCommandDescription => "標準入出力でエミュレータホストプロトコルを実行";
        public override string HostObservableOptionDescription => "IRQ、DRQ、advance-request 通知を含む通知対応 exchange を出力します";
        public override string HostScriptCommandDescription => "外部ホストブリッジ向けの再利用可能な要求スクリプトを生成";
        public override string HostScriptD88PathCommandDescription => "読み取り専用の D88 パス指定要求スクリプトを書き出す";
        public override string HostScriptD88BufferCommandDescription => "読み取り専用の D88 バッファ指定要求スクリプトを書き出す";
        public override string HostScriptRawBufferCommandDescription => "読み取り専用の raw セクタイメージ用要求スクリプトを書き出す";
        public override string HostScriptInspectCommandDescription => "要求スクリプトを読んで要約を表示する";
        public override string HostBundleCommandDescription => "ホスト検証 bundle を確認する";
        public override string HostBundleInspectCommandDescription => "host-proof bundle を読んで要約を表示する";
        public override string HostBundleVerifyCommandDescription => "内蔵 baseline と照合して host-proof bundle を検証する";
        public override string HostBundlePackCommandDescription => "transcript と任意の request script から host-proof bundle を組み立てる";
        public override string HostTranscriptCommandDescription => "生の host-proof transcript を確認・検証する";
        public override string HostTranscriptInspectCommandDescription => "transcript を読んで proof 要約を表示する";
        public override string HostTranscriptReportCommandDescription => "transcript から markdown の proof report を生成する";
        public override string HostTranscriptVerifyCommandDescription => "内蔵 baseline と照合して transcript を検証する";
        public override string HostOutputArgumentDescription => "出力ファイルパス";
        public override string HostDirectoryArgumentDescription => "bundle ディレクトリパス";
        public override string HostBaseNameArgumentDescription => "bundle ベース名";
        public override string HostBaselineArgumentDescription => "baseline 名: event-d88 または event-raw";
        public override string HostTranscriptArgumentDescription => "transcript ファイルパス";
        public override string HostRequestScriptOptionDescription => "任意の request script ファイルパス";
        public override string HostOpenModeOptionDescription => "bundle に記録する open mode 名";
        public override string HostExchangeModeOptionDescription => "bundle に記録する exchange mode 名";
        public override string BootCommandDescription => "ブート情報の操作";
        public override string BootShowCommandDescription => "このディスクのブート情報を表示します";
        public override string BootClearCommandDescription => "ブートセクタ全消去ではなく、ファイル参照型のブート情報だけを無効化します";
        public override string BootCloneCommandDescription => "ブート可能なディスクを複製します";
        public override string ImageArgumentDescription => "ディスクイメージへのパス";
        public override string SourceImageArgumentDescription => "元ディスクイメージのパス";
        public override string DestinationImageArgumentDescription => "出力先ディスクイメージのパス";
        public override string DiskFileArgumentDescription => "ディスク上のファイル名";
        public override string HostFileArgumentDescription => "ホスト側ファイルのパス";
        public override string HostPathArgumentDescription => "書き出し先ホストパス";
        public override string SourceNameArgumentDescription => "元のディスクファイル名";
        public override string TargetNameArgumentDescription => "新しい名前または複製先のファイル名";
        public override string NewNameArgumentDescription => "新しいディスクファイル名";
        public override string LabelTextArgumentDescription => "ラベル文字列";
        public override string FileCrossCopyFilesArgumentDescription => "コピーするファイル一覧（カンマ区切り、または 'all'）";
        public override string LayoutInputOptionDescription => "レイアウト計画をファイルから読み込みます";
        public override string LayoutOutputOptionDescription => "書き出したレイアウト計画の保存先ファイル";
        public override string LayoutStdinOptionDescription => "レイアウト計画を標準入力から読み込みます";
        public override string LayoutStrictOptionDescription => "警告もエラーとして扱います";
        public override string LayoutBeforeOptionDescription => "このエントリの前へ配置します";
        public override string LayoutSortByOptionDescription => "ソートキー: name, ext, type";
        public override string BootFilesOptionDescription => "コピーするファイル一覧。all も指定可能";
        public override string TargetFileNameOptionDescription => "ターゲットディスク上のファイル名を上書きします";
        public override string TabModeOptionDescription => "プレーンテキストのタブ処理: keep, spaces, remove";
        public override string TabWidthOptionDescription => "--tab-mode spaces 使用時のタブ幅";
        public override string TruncateTextOnOverflowOptionDescription => "タブ展開でファイルシステム上限を超えた場合にエラーではなく切り詰めます";
        public override string DiskCreateImageFormatOptionDescription => "コンテナ/イメージ形式: d88, d77, 2d, dsk（既定値: d88）";
        public override string DiskCreateDiskTypeOptionDescription => "ディスク種別: 2d, 2dd, 2hd（既定値: 2d）";
        public override string DiskCreateFileSystemOptionDescription => "初期化する任意のファイルシステム: hu-basic, n88-basic, msx-dos, xdos。省略時は未フォーマットのままです";
        public override string DiskCreateNameOptionDescription => "対応コンテナに設定する任意のディスク名";
        public override string DiskFormatFsOptionDescription => "明示的にフォーマットするファイルシステム: hu-basic, n88-basic, msx-dos, xdos";
        public override string ExplicitFileSystemOptionDescription => "明示的に使用するファイルシステム: hu-basic, n88-basic, msx-dos, xdos";
        public override string SectorLocationArgumentDescription => "開始する線形セクタ番号";
        public override string SectorCountArgumentDescription => "対象セクタ数";
        public override string DumpLocationArgumentDescription => "位置指定: offset (0x...), 線形セクタ番号, または cylinderN,sideN,sectorN";
        public override string DumpLengthArgumentDescription => "バイト数、または '<N>sector' 形式のセクタ数";
        public override string DiskSectorCopyCommandDescription => "ディスクイメージ間でセクタ単位の物理コピーを実行します";
        public override string DiskSectorCopyForceOptionDescription => "上書き確認をスキップします";
        public override string ListingFilesForMessage => "Listing files for";
        public override string UsingEncodingMessage => "Using Encoding";
        public override string SuccessPrefix => "Success";
        public override string ErrorPrefix => "Error";
        public override string FileInjectedMessage => "ファイルを注入しました。";
        public override string FileExtractedMessage => "ファイルを書き出しました。";
        public override string FileDeletedMessage => "ファイルを削除しました。";
        public override string FileRenamedMessage => "ファイル名を変更しました。";
        public override string FileCopiedMessage => "ファイルを複製しました。";
        public override string DiskCreatedMessage => "ディスクを作成しました。";
        public override string DiskFormattedMessage => "ディスクをフォーマットしました。";
        public override string DiskSectorCopiedMessage => "セクタコピーが完了しました（{0} トラックをコピー、{1} セクタをスキップ）。";
        public override string LayoutUpdatedMessage => "ディレクトリエントリ順を更新しました。";
        public override string LabelInsertedMessage => "ラベルエントリを挿入しました。";
        public override string DirectoryEntriesSortedMessage => "ディレクトリエントリをソートしました。";
        public override string LayoutValidMessage => "レイアウト計画は有効です。";
        public override string LayoutAppliedMessage => "レイアウト計画を適用しました。";
        public override string BootableDiskCreatedMessage => "ブート可能ディスクを作成しました。";
        public override string BootClearedMessage => "ブート情報を無効化しました。";
        public override string UnsupportedLanguageMessage => "未対応の言語です。ja または en を指定してください。";
        public override string FileSystemNotDetectedMessage => "このディスクから対応ファイルシステムを検出できませんでした。";
        public override string LayoutNotSupportedMessage => "このファイルシステムではディレクトリレイアウト編集をサポートしていません。";
        public override string StdinInputConflictMessage => "--input と --stdin は同時に指定できません。";
        public override string InputRequiredMessage => "--input または --stdin を指定してください。";
        public override string ValidationSummaryFormat => "{0} errors, {1} warnings";
        public override string ValidationErrorLabel => "ERROR";
        public override string ValidationWarningLabel => "WARNING";
        public override string BootModeFileBacked => "ファイル参照型";
        public override string BootModeSectorResident => "セクタ常駐型";
        public override string BootModeNone => "なし";
        public override string BootShowTitle => "ブート情報";
        public override string OverwriteConfirmationMessage => "ファイル '{0}' は既に存在します。上書きしますか？ (y/N): ";
        public override string BootExportCommandDescription => "ブートエントリをバイナリデータとJSON情報ファイルとして出力します";
        public override string BootExportOutputOptionDescription => "出力先ディレクトリ";
        public override string BootEntriesExportedMessage => "ブートエントリを出力しました: {0}";
        public override string BootNoEntriesFoundMessage => "出力可能なブートエントリが見つかりませんでした。";
        public override string BootImportCommandDescription => "ブートエントリのバイナリとJSONメタデータをディスクにインポートします";
        public override string BootImportBinaryOptionDescription => ".bin ペイロードファイルのパス";
        public override string BootImportMetadataOptionDescription => ".json メタデータファイルのパス";
        public override string BootEntryImportedMessage => "ブートエントリをインポートしました。";
        public override string CheckUpdateCommandDescription => "GitHub Releases を確認して新しい公開版があるか調べます";
        public override string VersionCommandDescription => "CLI のバージョンを表示します";
        public override string CheckUpdateCurrentVersionLabel => "現在のバージョン";
        public override string CheckUpdateLatestVersionLabel => "最新バージョン";
        public override string CheckUpdateReleaseUrlLabel => "リリースURL";
        public override string CheckUpdateWindowsMsiLabel => "Windows MSI";
        public override string CheckUpdateAvailableMessage => "新しいバージョンがあります。";
        public override string CheckUpdateUpToDateMessage => "現在のバージョンは最新です。";
        public override string MultiSlotD88WriteNotSupportedMessage => "複数スロットを含む D88 コンテナへの書き込みはまだ未対応です。1 スロットのイメージへ分割してから、もう一度試してください。読み込みは現在先頭スロットのみ対応しています。";
        public override string ContainerLabel => "コンテナ";
        public override string DiskTypeLabel => "ディスク種別";
        public override string MachineProfileLabel => "マシンプロファイル";
        public override string GeometryLabel => "物理フォーマット";
        public override string ImageSizeLabel => "イメージサイズ";
        public override string WriteProtectedLabel => "書き込み保護";
        public override string EncodingLabel => "文字コード";
        public override string FullHelpFooter => "個別の詳細は '<command> --help' でも確認できます。";
        public override string ImageFileOverwriteOptionDescription => "同名ファイルの上書きを番号付きエイリアス生成ではなく許可します";
        public override string LogOptionDescription => "デフォルトのシステムログ位置へログを出力します（OS固有、日付付き、7日間ローテーション）";
        public override string LogOptionWithPathDescription => "指定ファイルへログを出力します（ファイルが存在する場合は追記）";
        public override string ImageFileOverwriteIgnoredWarning => "ファイル名の制約により同名ファイルの上書きができません。エイリアス生成を代わりに使用します。";
    }
}
