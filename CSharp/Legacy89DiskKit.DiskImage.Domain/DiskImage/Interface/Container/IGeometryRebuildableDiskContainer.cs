namespace Legacy89DiskKit.DiskImage.Domain.Interface.Container;

public interface IGeometryRebuildableDiskContainer
{
    void RebuildGeometry(Func<int, int, (int sectors, ushort size, byte density)?> perTrackGeometry);
}
