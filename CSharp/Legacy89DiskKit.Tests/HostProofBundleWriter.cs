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

        await File.WriteAllTextAsync(
            markdownPath,
            HostProofReportMarkdownRenderer.Render(report),
            cancellationToken);

        await HostProofTranscriptFileStore.SaveAsync(transcriptPath, transcript, cancellationToken);

        if (requestScript is not null)
        {
            await HostProofRequestScriptFileStore.SaveAsync(requestPath, requestScript, cancellationToken);
        }
    }
}
