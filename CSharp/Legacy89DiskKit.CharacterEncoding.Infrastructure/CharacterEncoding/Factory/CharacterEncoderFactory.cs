using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Factory;

public class CharacterEncoderFactory
{
    public ICharacterEncoder GetEncoder(MachineType machineType)
    {
        return machineType switch
        {
            MachineType.X1 => new X1CharacterEncoder(),
            _ => throw new NotSupportedException($"Encoder for machine type {machineType} is not implemented yet.")
        };
    }
}
