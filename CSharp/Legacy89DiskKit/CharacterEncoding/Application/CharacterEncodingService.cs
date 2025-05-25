using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Factory;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Domain.Exception;

namespace Legacy89DiskKit.CharacterEncoding.Application;

public class CharacterEncodingService
{
    private readonly ICharacterEncoderFactory _encoderFactory;

    public CharacterEncodingService(ICharacterEncoderFactory encoderFactory)
    {
        _encoderFactory = encoderFactory ?? throw new ArgumentNullException(nameof(encoderFactory));
    }

    public string DecodeText(byte[] data, MachineType machineType)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        try
        {
            var encoder = _encoderFactory.GetEncoder(machineType);
            return encoder.DecodeText(data);
        }
        catch (Exception ex) when (!(ex is CharacterEncodingException))
        {
            throw new CharacterEncodingException($"Failed to decode text for machine type {machineType}", ex);
        }
    }

    public byte[] EncodeText(string text, MachineType machineType)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        try
        {
            var encoder = _encoderFactory.GetEncoder(machineType);
            return encoder.EncodeText(text);
        }
        catch (Exception ex) when (!(ex is CharacterEncodingException))
        {
            throw new CharacterEncodingException($"Failed to encode text for machine type {machineType}", ex);
        }
    }

    public bool IsMachineTypeSupported(MachineType machineType)
    {
        try
        {
            _encoderFactory.GetEncoder(machineType);
            return true;
        }
        catch (CharacterEncodingException)
        {
            return false;
        }
    }

    public IEnumerable<MachineType> GetSupportedMachineTypes()
    {
        var supportedTypes = new List<MachineType>();
        
        foreach (MachineType machineType in Enum.GetValues<MachineType>())
        {
            if (IsMachineTypeSupported(machineType))
            {
                supportedTypes.Add(machineType);
            }
        }

        return supportedTypes;
    }
}