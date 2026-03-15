using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal static class HostProofRequestScriptFileStore
{
    public static async Task SaveAsync(string filePath, IEnumerable<EmulatorHostRequest> requests, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(requests);

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var payload = HostProofRequestScriptCodec.SerializeLines(requests);
        await File.WriteAllTextAsync(filePath, payload, cancellationToken);
    }

    public static async Task<IReadOnlyList<EmulatorHostRequest>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var payload = await File.ReadAllTextAsync(filePath, cancellationToken);
        return HostProofRequestScriptCodec.DeserializeLines(payload);
    }
}
