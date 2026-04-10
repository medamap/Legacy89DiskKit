using System.Text.Json;

namespace Legacy89DiskKit.Tests;

internal static class HostProofTranscriptCodec
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string SerializeLines(IEnumerable<HostProofTranscriptEntry> entries)
    {
        return string.Join(
            Environment.NewLine,
            entries.Select(entry => JsonSerializer.Serialize(entry, SerializerOptions)));
    }

    public static IReadOnlyList<HostProofTranscriptEntry> DeserializeLines(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return payload
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonSerializer.Deserialize<HostProofTranscriptEntry>(line, SerializerOptions)
                ?? throw new InvalidOperationException("The host proof transcript entry could not be deserialized."))
            .ToArray();
    }
}
