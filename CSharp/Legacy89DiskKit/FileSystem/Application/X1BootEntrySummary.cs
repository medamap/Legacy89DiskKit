using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.FileSystem.Application;
public enum X1BootEntryKind
{
    None,
    HuBasicFileBacked,
    XDosSectorResident,
    Unsupported
}

public sealed record X1BootEntrySummary(X1BootEntryKind Kind, string? DisplayName = null, ushort? LoadAddress = null, ushort? ExecutionAddress = null, MachineType MachineFamily = MachineType.X1);