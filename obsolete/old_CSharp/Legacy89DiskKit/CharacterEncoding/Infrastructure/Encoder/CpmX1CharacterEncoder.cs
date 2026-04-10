using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

/// <summary>
/// Character encoder for CP/M on X1 systems
/// </summary>
public class CpmX1CharacterEncoder : ICharacterEncoder
{
    private readonly X1CharacterEncoder _baseEncoder = new();
    
    /// <inheritdoc/>
    public MachineType SupportedMachine => MachineType.CpmX1;
    
    /// <inheritdoc/>
    public string DecodeText(byte[] data)
    {
        // Use the same logic as X1
        return _baseEncoder.DecodeText(data);
    }
    
    /// <inheritdoc/>
    public byte[] EncodeText(string text)
    {
        // Use the same logic as X1
        return _baseEncoder.EncodeText(text);
    }
}