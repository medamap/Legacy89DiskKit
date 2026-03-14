namespace Legacy89DiskKit.Application.Fdc.Hosts;

public sealed record XmilWebFdcState(
    bool DiskInserted,
    bool DriveReady,
    bool Busy,
    bool Irq,
    bool Drq,
    int SelectedDrive,
    int SelectedSide);
