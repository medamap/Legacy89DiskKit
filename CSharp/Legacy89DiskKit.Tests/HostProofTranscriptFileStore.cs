namespace Legacy89DiskKit.Tests;

internal static class HostProofTranscriptFileStore
{
    public static async Task SaveAsync(string filePath, IEnumerable<HostProofTranscriptEntry> entries, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(entries);

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var payload = HostProofTranscriptCodec.SerializeLines(entries);
        await File.WriteAllTextAsync(filePath, payload, cancellationToken);
    }

    public static async Task<IReadOnlyList<HostProofTranscriptEntry>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var payload = await File.ReadAllTextAsync(filePath, cancellationToken);
        return HostProofTranscriptCodec.DeserializeLines(payload);
    }
}
