using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using System.Text;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

/// <summary>
/// Character encoder for CP/M on PC-8801 systems
/// </summary>
public class CpmPc8801CharacterEncoder : ICharacterEncoder
{
    private readonly Pc8801CharacterEncoder _baseEncoder = new();
    
    /// <inheritdoc/>
    public MachineType SupportedMachine => MachineType.CpmPc8801;
    
    /// <inheritdoc/>
    public string DecodeText(byte[] data)
    {
        // Use the same logic as PC-8801
        return _baseEncoder.DecodeText(data);
    }
    
    /// <inheritdoc/>
    public byte[] EncodeText(string text)
    {
        // Use the same logic as PC-8801
        return _baseEncoder.EncodeText(text);
    }
}