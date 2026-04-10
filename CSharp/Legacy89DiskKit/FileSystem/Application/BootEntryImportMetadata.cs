using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.FileSystem.Application;
public sealed record BootEntryImportMetadata(MachineType MachineFamily, string Mode, string? DisplayName, string? SuggestedBinaryFileName, int? PayloadLength, ushort? LoadAddress, ushort? ExecutionAddress, ushort? StartRecord = null);