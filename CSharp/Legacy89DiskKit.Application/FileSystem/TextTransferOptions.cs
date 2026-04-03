namespace Legacy89DiskKit.Application.FileSystem;

public sealed record TextTransferOptions(
    string TabMode = "keep",
    int TabWidth = 4,
    bool TruncateOnOverflow = false,
    string? NewlineOverride = null
);
