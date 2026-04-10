using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public static class EmulatorHostBundleReader
{
    public static async Task<EmulatorHostBundle> ReadAsync(
        string outputDirectory,
        string baseName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);

        var manifestPath = Path.Combine(outputDirectory, $"{baseName}.manifest.json");
        var manifest = EmulatorHostBundleManifestCodec.Deserialize(
            await File.ReadAllTextAsync(manifestPath, cancellationToken));

        var markdownPath = Path.Combine(outputDirectory, manifest.ReportFileName);
        var transcriptPath = Path.Combine(outputDirectory, manifest.TranscriptFileName);

        var markdown = await File.ReadAllTextAsync(markdownPath, cancellationToken);
        var transcript = await EmulatorHostTranscriptFileStore.LoadAsync(transcriptPath, cancellationToken);

        IReadOnlyList<EmulatorHostRequest> requestScript = [];
        if (!string.IsNullOrWhiteSpace(manifest.RequestScriptFileName))
        {
            var requestPath = Path.Combine(outputDirectory, manifest.RequestScriptFileName);
            requestScript = await EmulatorHostRequestScriptFileStore.LoadAsync(requestPath, cancellationToken);
        }

        return new EmulatorHostBundle(manifest, markdown, transcript, requestScript);
    }
}
