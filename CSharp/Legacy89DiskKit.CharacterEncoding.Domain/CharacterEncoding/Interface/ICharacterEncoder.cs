using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.CharacterEncoding.Domain.Interface;

public interface ICharacterEncoder
{
    string EncodingId { get; }
    byte[] EncodeText(string text);
    string DecodeText(byte[] data);
    string DecodeText(byte[] data, string newline);
    MachineType SupportedMachine { get; }
}
