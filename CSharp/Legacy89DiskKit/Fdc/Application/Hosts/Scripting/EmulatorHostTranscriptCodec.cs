using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using System.Text.Json;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public static class EmulatorHostTranscriptCodec
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string SerializeLines(IEnumerable<EmulatorHostTranscriptEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return string.Join(
            Environment.NewLine,
            entries.Select(entry => JsonSerializer.Serialize(entry, SerializerOptions)));
    }

    public static IReadOnlyList<EmulatorHostTranscriptEntry> DeserializeLines(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return payload
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonSerializer.Deserialize<EmulatorHostTranscriptEntry>(line, SerializerOptions)
                ?? throw new InvalidOperationException("The emulator host transcript entry could not be deserialized."))
            .ToArray();
    }
}
