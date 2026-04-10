using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public sealed record BootEntryExportArtifact(MachineType MachineFamily, BootInfoMode Mode, string? DisplayName, byte[] Payload, string SuggestedBinaryFileName, string SuggestedMetadataFileName, ushort? LoadAddress = null, ushort? ExecutionAddress = null);