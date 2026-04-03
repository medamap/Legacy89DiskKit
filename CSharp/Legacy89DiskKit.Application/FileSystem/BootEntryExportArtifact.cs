using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.Application.FileSystem;

public sealed record BootEntryExportArtifact(
    MachineType MachineFamily,
    BootInfoMode Mode,
    string? DisplayName,
    byte[] Payload,
    string SuggestedBinaryFileName,
    string SuggestedMetadataFileName,
    ushort? LoadAddress = null,
    ushort? ExecutionAddress = null
);
