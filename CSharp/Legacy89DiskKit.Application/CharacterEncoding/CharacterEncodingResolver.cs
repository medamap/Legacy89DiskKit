using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Application.CharacterEncoding;

public sealed class CharacterEncodingResolver
{
    private readonly IEncoderRegistry _registry;

    public CharacterEncodingResolver(IEncoderRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public ICharacterEncoder ResolveEncoder(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        foreach (var candidate in GetCandidates(fsInfo, encodingOverride))
        {
            var encoder = _registry.GetEncoder(candidate);
            if (encoder != null)
            {
                return encoder;
            }
        }

        throw new InvalidOperationException($"No character encoder is registered for filesystem '{fsInfo.FileSystemName}'.");
    }

    public CharacterEncodingProfile ResolveProfile(DiskFileSystemInfo fsInfo, string? encodingOverride = null)
    {
        var encoder = ResolveEncoder(fsInfo, encodingOverride);
        return new CharacterEncodingProfile(
            EncodingId: encoder.EncodingId,
            DisplayName: encoder.EncodingId,
            MachineType: encoder.SupportedMachine);
    }

    private static IEnumerable<string> GetCandidates(DiskFileSystemInfo fsInfo, string? encodingOverride)
    {
        if (!string.IsNullOrWhiteSpace(encodingOverride))
        {
            yield return encodingOverride;
        }

        if (!string.IsNullOrWhiteSpace(fsInfo.DefaultEncodingId))
        {
            yield return fsInfo.DefaultEncodingId;
        }

        if (!string.IsNullOrWhiteSpace(fsInfo.PlatformId))
        {
            yield return fsInfo.PlatformId;
        }
    }
}
