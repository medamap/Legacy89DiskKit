namespace Legacy89DiskKit.Fdc.Application.Hosts;

public sealed record XmilWebFdcState(
    bool DiskInserted,
    bool DriveReady,
    bool Busy,
    bool Irq,
    bool Drq,
    int SelectedDrive,
    int SelectedSide);
