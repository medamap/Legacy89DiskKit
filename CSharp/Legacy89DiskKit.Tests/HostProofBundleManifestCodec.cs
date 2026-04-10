using System.Text.Json;

namespace Legacy89DiskKit.Tests;

internal static class HostProofBundleManifestCodec
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(HostProofBundleManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return JsonSerializer.Serialize(manifest, SerializerOptions);
    }

    public static HostProofBundleManifest Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        return JsonSerializer.Deserialize<HostProofBundleManifest>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("The host proof bundle manifest could not be deserialized.");
    }
}
