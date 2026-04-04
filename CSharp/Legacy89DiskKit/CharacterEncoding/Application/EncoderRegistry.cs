using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;

namespace Legacy89DiskKit.CharacterEncoding.Application;

public class EncoderRegistry : IEncoderRegistry
{
    private readonly Dictionary<string, ICharacterEncoder> _encoders = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string platformId, ICharacterEncoder encoder)
    {
        _encoders[platformId] = encoder;
    }

    public ICharacterEncoder? GetEncoder(string platformId)
    {
        if (_encoders.TryGetValue(platformId, out var encoder))
        {
            return encoder;
        }
        return null;
    }
}
