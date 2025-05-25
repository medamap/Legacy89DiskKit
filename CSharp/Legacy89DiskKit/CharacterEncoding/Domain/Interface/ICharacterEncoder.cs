using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.CharacterEncoding.Domain.Interface;

public interface ICharacterEncoder
{
    byte[] EncodeText(string text);
    string DecodeText(byte[] data);
    MachineType SupportedMachine { get; }
}