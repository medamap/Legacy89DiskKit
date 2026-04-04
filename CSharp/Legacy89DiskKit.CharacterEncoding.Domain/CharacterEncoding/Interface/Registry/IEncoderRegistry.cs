using Legacy89DiskKit.CharacterEncoding.Domain.Interface;

namespace Legacy89DiskKit.CharacterEncoding.Domain.Interface.Registry;

public interface IEncoderRegistry
{
    void Register(string platformId, ICharacterEncoder encoder);
    ICharacterEncoder? GetEncoder(string platformId);
}
