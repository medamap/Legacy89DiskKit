namespace Legacy89DiskKit.Domain.Fdc.Interface;

public interface IControllerFacingMedium
{
    string MediumKind { get; }

    bool IsReady { get; }

    bool IsWriteProtected { get; }

    void Reset();

    void SelectSide(int side);

    void SeekTrack(int track);

    byte ReadStatus();

    byte ReadTrackRegister();

    byte ReadSectorRegister();

    byte ReadDataRegister();

    void WriteCommand(byte value);

    void WriteTrackRegister(byte value);

    void WriteSectorRegister(byte value);

    void WriteDataRegister(byte value);

    bool IsIrqAsserted { get; }

    bool IsDrqAsserted { get; }
}
