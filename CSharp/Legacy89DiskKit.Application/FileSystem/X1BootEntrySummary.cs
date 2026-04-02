namespace Legacy89DiskKit.Application.FileSystem;

public enum X1BootEntryKind
{
    None,
    HuBasicFileBacked,
    XDosSectorResident,
    Unsupported
}

public sealed record X1BootEntrySummary(
    X1BootEntryKind Kind,
    string? DisplayName = null,
    ushort? LoadAddress = null,
    ushort? ExecutionAddress = null
);
