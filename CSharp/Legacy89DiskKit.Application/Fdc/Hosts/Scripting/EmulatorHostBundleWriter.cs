using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public static class EmulatorHostBundleWriter
{
    public static async Task WriteAsync(
        string outputDirectory,
        string baseName,
        EmulatorHostProofReport report,
        IReadOnlyList<EmulatorHostTranscriptEntry> transcript,
        IReadOnlyList<EmulatorHostRequest>? requestScript = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(transcript);

        Directory.CreateDirectory(outputDirectory);

        var markdownPath = Path.Combine(outputDirectory, $"{baseName}.md");
        var transcriptPath = Path.Combine(outputDirectory, $"{baseName}.jsonl");
        var requestPath = Path.Combine(outputDirectory, $"{baseName}.requests.jsonl");
        var manifestPath = Path.Combine(outputDirectory, $"{baseName}.manifest.json");

        await File.WriteAllTextAsync(
            markdownPath,
            EmulatorHostProofReportMarkdownRenderer.Render(report),
            cancellationToken);

        await EmulatorHostTranscriptFileStore.SaveAsync(transcriptPath, transcript, cancellationToken);

        if (requestScript is not null)
        {
            await EmulatorHostRequestScriptFileStore.SaveAsync(requestPath, requestScript, cancellationToken);
        }

        var manifest = new EmulatorHostBundleManifest(
            BaseName: baseName,
            ReportFileName: Path.GetFileName(markdownPath),
            TranscriptFileName: Path.GetFileName(transcriptPath),
            RequestScriptFileName: requestScript is null ? null : Path.GetFileName(requestPath),
            OpenMode: report.OpenMode,
            ExchangeMode: report.ExchangeMode);

        await File.WriteAllTextAsync(
            manifestPath,
            EmulatorHostBundleManifestCodec.Serialize(manifest),
            cancellationToken);
    }
}
