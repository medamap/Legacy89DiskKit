using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal static class CliHostProofRunner
{
    public static async Task<IReadOnlyList<HostProofTranscriptEntry>> RunObservableAsync(
        CliHostProcessSession process,
        IEnumerable<EmulatorHostRequest> requests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentNullException.ThrowIfNull(requests);

        var transcript = new List<HostProofTranscriptEntry>();
        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await process.SendExchangeAsync(request, transcript);
        }

        return transcript;
    }

    public static async Task<IReadOnlyList<EmulatorHostResponse>> RunPlainAsync(
        CliHostProcessSession process,
        IEnumerable<EmulatorHostRequest> requests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentNullException.ThrowIfNull(requests);

        var responses = new List<EmulatorHostResponse>();
        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            responses.Add(await process.SendResponseAsync(request));
        }

        return responses;
    }
}
