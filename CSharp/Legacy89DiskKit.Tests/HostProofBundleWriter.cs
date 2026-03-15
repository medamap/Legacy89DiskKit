namespace Legacy89DiskKit.Tests;

internal static class HostProofBundleWriter
{
    public static async Task WriteAsync(
        string outputDirectory,
        string baseName,
        HostProofReport report,
        IReadOnlyList<HostProofTranscriptEntry> transcript,
        IReadOnlyList<Legacy89DiskKit.Application.Fdc.Hosts.Protocol.EmulatorHostRequest>? requestScript = null,
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
            HostProofReportMarkdownRenderer.Render(report),
            cancellationToken);

        await HostProofTranscriptFileStore.SaveAsync(transcriptPath, transcript, cancellationToken);

        if (requestScript is not null)
        {
            await HostProofRequestScriptFileStore.SaveAsync(requestPath, requestScript, cancellationToken);
        }

        var manifest = new HostProofBundleManifest(
            BaseName: baseName,
            ReportFileName: Path.GetFileName(markdownPath),
            TranscriptFileName: Path.GetFileName(transcriptPath),
            RequestScriptFileName: requestScript is null ? null : Path.GetFileName(requestPath),
            OpenMode: report.OpenMode,
            ExchangeMode: report.ExchangeMode);

        await File.WriteAllTextAsync(
            manifestPath,
            HostProofBundleManifestCodec.Serialize(manifest),
            cancellationToken);
    }
}
