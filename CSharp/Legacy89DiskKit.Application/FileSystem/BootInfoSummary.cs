namespace Legacy89DiskKit.Application.FileSystem;

public enum BootInfoMode
{
    None,
    FileBacked,
    SectorResident
}

public sealed record BootInfoSummary(
    BootInfoMode Mode,
    string? FileName = null,
    ushort? LoadAddress = null,
    ushort? ExecutionAddress = null
);
