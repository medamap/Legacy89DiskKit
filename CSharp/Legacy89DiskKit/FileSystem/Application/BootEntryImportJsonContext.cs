using System.Text.Json.Serialization;
namespace Legacy89DiskKit.FileSystem.Application;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(BootEntryImportMetadata))]
public partial class BootEntryImportJsonContext : JsonSerializerContext
{
}
