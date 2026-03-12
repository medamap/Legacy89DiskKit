using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Application.FileSystem;

namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public sealed record FileListColumn(string Header, bool RightAlign = false);

public sealed record FileListRow(IReadOnlyList<string> Values);

public sealed record FileListSummaryItem(string Label, string Value);

public sealed record FileListLegendItem(string Key, string Description);

public sealed record FileListFootnote(int Number, string Text);

public sealed record FileListView(
    IReadOnlyList<FileListSummaryItem> Summary,
    IReadOnlyList<FileListColumn> Columns,
    IReadOnlyList<FileListRow> Rows,
    IReadOnlyList<FileListLegendItem> Legends,
    IReadOnlyList<FileListFootnote> Footnotes
);

public sealed record FileListEntryContext(
    FileEntry Entry,
    string DisplayName,
    string DisplayBaseName,
    string DisplayExtension,
    long? ActualSize = null
);

public sealed record FileListFormatContext(
    DiskFileSystemInfo FileSystemInfo,
    IReadOnlyList<FileListEntryContext> Entries,
    HuBasicBootRecordInfo? BootRecordInfo = null,
    BootInfoSummary? BootSummary = null
);
