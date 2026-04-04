using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.FileSystem.Application;
public enum BootInfoMode
{
    None,
    FileBacked,
    SectorResident
}

public sealed record BootInfoSummary(BootInfoMode Mode, string? FileName = null, ushort? LoadAddress = null, ushort? ExecutionAddress = null, string? DisplayName = null, MachineType MachineFamily = MachineType.Unknown);