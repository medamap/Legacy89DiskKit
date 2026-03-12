using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.Domain.CharacterEncoding.Interface;

public interface ICharacterEncoder
{
    byte[] EncodeText(string text);
    string DecodeText(byte[] data);
    string DecodeText(byte[] data, string newline);
    MachineType SupportedMachine { get; }
}
