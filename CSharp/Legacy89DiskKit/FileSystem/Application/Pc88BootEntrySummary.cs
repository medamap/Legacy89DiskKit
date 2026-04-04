using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.FileSystem.Application;
public enum Pc88BootEntryKind
{
    None,
    N88BasicSectorResident,
    CpmSectorResident,
    Unsupported
}

public sealed record Pc88BootEntrySummary(Pc88BootEntryKind Kind, string? DisplayName = null, ushort? LoadAddress = null, ushort? ExecutionAddress = null, MachineType MachineFamily = MachineType.PC8801);