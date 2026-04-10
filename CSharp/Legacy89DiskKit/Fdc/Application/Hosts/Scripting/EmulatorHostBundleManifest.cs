using System.Text.Json;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Scripting;

public sealed record EmulatorHostBundleManifest(
    string BaseName,
    string ReportFileName,
    string TranscriptFileName,
    string? RequestScriptFileName,
    string OpenMode,
    string ExchangeMode);

public static class EmulatorHostBundleManifestCodec
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(EmulatorHostBundleManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return JsonSerializer.Serialize(manifest, SerializerOptions);
    }

    public static EmulatorHostBundleManifest Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        return JsonSerializer.Deserialize<EmulatorHostBundleManifest>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("The emulator host bundle manifest could not be deserialized.");
    }
}
