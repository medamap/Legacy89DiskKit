namespace Legacy89DiskKit.Domain.FileSystem.Model;

public enum ContentKind
{
    Binary,
    Text,
}

public record TransferFileEnvelope(
    string FileName,
    byte[] Payload,
    ContentKind ContentKind,
    string? SourceFileSystemId,
    ushort? LoadAddress,
    ushort? ExecutionAddress,
    uint? Timestamp,
    string? EncodingId,
    IReadOnlyDictionary<string, string>? Metadata
);
