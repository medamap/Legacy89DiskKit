namespace Legacy89DiskKit.Tests;

internal static class HostProofBundleReader
{
    public static async Task<HostProofBundle> ReadAsync(
        string outputDirectory,
        string baseName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);

        var manifestPath = Path.Combine(outputDirectory, $"{baseName}.manifest.json");
        var manifest = HostProofBundleManifestCodec.Deserialize(
            await File.ReadAllTextAsync(manifestPath, cancellationToken));

        var markdownPath = Path.Combine(outputDirectory, manifest.ReportFileName);
        var transcriptPath = Path.Combine(outputDirectory, manifest.TranscriptFileName);

        var markdown = await File.ReadAllTextAsync(markdownPath, cancellationToken);
        var transcript = await HostProofTranscriptFileStore.LoadAsync(transcriptPath, cancellationToken);

        IReadOnlyList<Legacy89DiskKit.Fdc.Application.Hosts.Protocol.EmulatorHostRequest> requestScript = [];
        if (!string.IsNullOrWhiteSpace(manifest.RequestScriptFileName))
        {
            var requestPath = Path.Combine(outputDirectory, manifest.RequestScriptFileName);
            requestScript = await HostProofRequestScriptFileStore.LoadAsync(requestPath, cancellationToken);
        }

        return new HostProofBundle(manifest, markdown, transcript, requestScript);
    }
}
