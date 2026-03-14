namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public static class EmulatorHostTranscriptFileStore
{
    public static async Task SaveAsync(string filePath, IEnumerable<EmulatorHostTranscriptEntry> entries, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(entries);

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var payload = EmulatorHostTranscriptCodec.SerializeLines(entries);
        await File.WriteAllTextAsync(filePath, payload, cancellationToken);
    }

    public static async Task<IReadOnlyList<EmulatorHostTranscriptEntry>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var payload = await File.ReadAllTextAsync(filePath, cancellationToken);
        return EmulatorHostTranscriptCodec.DeserializeLines(payload);
    }
}
