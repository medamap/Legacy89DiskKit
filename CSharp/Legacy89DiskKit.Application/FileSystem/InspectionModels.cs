namespace Legacy89DiskKit.Application.FileSystem;

public sealed record InspectionItem(string Section, string Key, string Value);

public sealed record InspectionReport(
    string Title,
    IReadOnlyList<InspectionItem> Items
);
