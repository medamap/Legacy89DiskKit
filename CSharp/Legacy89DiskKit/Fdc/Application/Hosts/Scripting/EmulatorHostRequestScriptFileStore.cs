using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public static class EmulatorHostRequestScriptFileStore
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

        var payload = EmulatorHostRequestScriptCodec.SerializeLines(requests);
        await File.WriteAllTextAsync(filePath, payload, cancellationToken);
    }

    public static async Task<IReadOnlyList<EmulatorHostRequest>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var payload = await File.ReadAllTextAsync(filePath, cancellationToken);
        return EmulatorHostRequestScriptCodec.DeserializeLines(payload);
    }
}
