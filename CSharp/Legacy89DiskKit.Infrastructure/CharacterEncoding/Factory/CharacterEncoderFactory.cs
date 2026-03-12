using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;

namespace Legacy89DiskKit.Infrastructure.CharacterEncoding.Factory;

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
