using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.Application.FileSystem;

public enum Pc88BootEntryKind
{
    None,
    N88BasicSectorResident,
    CpmSectorResident,
    Unsupported
}

public sealed record Pc88BootEntrySummary(
    Pc88BootEntryKind Kind,
    string? DisplayName = null,
    ushort? LoadAddress = null,
    ushort? ExecutionAddress = null,
    MachineType MachineFamily = MachineType.PC8801
);
