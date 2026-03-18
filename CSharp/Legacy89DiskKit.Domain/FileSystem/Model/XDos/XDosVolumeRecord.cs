namespace Legacy89DiskKit.Domain.FileSystem.Model.XDos;

public record XDosVolumeRecord(
    string DiskLabel,
    byte FormatType,
    byte YearBcd,
    byte MonthBcd,
    byte DayBcd
);
