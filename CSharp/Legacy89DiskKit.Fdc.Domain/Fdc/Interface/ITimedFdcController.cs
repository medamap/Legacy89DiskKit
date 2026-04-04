namespace Legacy89DiskKit.Fdc.Domain.Interface;

public interface ITimedFdcController
{
    TimeSpan? GetPendingAdvanceHint();

    void Advance(TimeSpan delta);
}
