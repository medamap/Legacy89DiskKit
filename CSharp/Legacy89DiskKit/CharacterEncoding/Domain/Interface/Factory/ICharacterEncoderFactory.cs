using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.CharacterEncoding.Domain.Interface.Factory;

public interface ICharacterEncoderFactory
{
    ICharacterEncoder GetEncoder(MachineType machineType);
}