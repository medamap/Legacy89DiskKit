namespace Legacy89DiskKit.Domain.Fdc.Interface;

public interface ITimedFdcController
{
    void Advance(TimeSpan delta);
}
