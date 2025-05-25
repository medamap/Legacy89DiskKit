using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Factory;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Domain.Exception;
using Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Factory;

public class CharacterEncoderFactory : ICharacterEncoderFactory
{
    public ICharacterEncoder GetEncoder(MachineType machineType)
    {
        return machineType switch
        {
            MachineType.X1 => new X1CharacterEncoder(),
            MachineType.X1Turbo => new X1CharacterEncoder(), // X1Turbo uses same encoding as X1
            MachineType.Pc8801 => new Pc8801CharacterEncoder(),
            MachineType.Pc8801Mk2 => new Pc8801CharacterEncoder(), // Pc8801Mk2 uses same encoding as Pc8801
            MachineType.Msx1 => new Msx1CharacterEncoder(),
            MachineType.Msx2 => new Msx1CharacterEncoder(), // Msx2 uses same encoding as Msx1 for basic characters
            _ => throw new CharacterEncodingException($"Unsupported machine type: {machineType}")
        };
    }
}