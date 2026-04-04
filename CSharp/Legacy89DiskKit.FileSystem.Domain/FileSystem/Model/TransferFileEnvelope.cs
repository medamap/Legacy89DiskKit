namespace Legacy89DiskKit.FileSystem.Domain.Model;

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
    DateTimeOffset? Timestamp,
    string? EncodingId,
    IReadOnlyDictionary<string, string>? Metadata
);
