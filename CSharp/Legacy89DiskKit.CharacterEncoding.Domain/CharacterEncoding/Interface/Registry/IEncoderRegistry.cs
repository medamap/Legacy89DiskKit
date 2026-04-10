using Legacy89DiskKit.Domain.CharacterEncoding.Interface;

namespace Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;

public interface IEncoderRegistry
{
    void Register(string platformId, ICharacterEncoder encoder);
    ICharacterEncoder? GetEncoder(string platformId);
}
