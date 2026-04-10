namespace Legacy89DiskKit.DiskOperation.Domain.Model;

public class FileNameConversionResult
{
    public string OriginalName { get; init; } = string.Empty;
    public string ConvertedName { get; init; } = string.Empty;
    public bool RequiresConversion { get; init; }
    public FileNameConversionType ConversionType { get; init; }
    public string? ConversionReason { get; init; }
}