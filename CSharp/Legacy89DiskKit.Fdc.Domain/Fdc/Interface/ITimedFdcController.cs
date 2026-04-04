namespace Legacy89DiskKit.Domain.Fdc.Interface;

public interface ITimedFdcController
{
    TimeSpan? GetPendingAdvanceHint();

    void Advance(TimeSpan delta);
}
