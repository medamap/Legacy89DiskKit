namespace Legacy89DiskKit.FileSystem.Domain.Model.XDos;

public record XDosVolumeRecord(
    string DiskLabel,
    byte FormatType,
    byte YearBcd,
    byte MonthBcd,
    byte DayBcd
);
