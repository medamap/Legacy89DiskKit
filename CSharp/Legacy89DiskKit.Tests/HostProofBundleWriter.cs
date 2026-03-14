namespace Legacy89DiskKit.Tests;

internal static class HostProofBundleWriter
{
    public static async Task WriteAsync(
        string outputDirectory,
        string baseName,
        HostProofReport report,
        IReadOnlyList<HostProofTranscriptEntry> transcript,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(transcript);

        Directory.CreateDirectory(outputDirectory);

        var markdownPath = Path.Combine(outputDirectory, $"{baseName}.md");
        var transcriptPath = Path.Combine(outputDirectory, $"{baseName}.jsonl");

        await File.WriteAllTextAsync(
            markdownPath,
            HostProofReportMarkdownRenderer.Render(report),
            cancellationToken);

        await HostProofTranscriptFileStore.SaveAsync(transcriptPath, transcript, cancellationToken);
    }
}
