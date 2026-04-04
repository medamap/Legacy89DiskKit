namespace Legacy89DiskKit.Domain.DiskImage.Interface.Container;

public interface IGeometryRebuildableDiskContainer
{
    void RebuildGeometry(Func<int, int, (int sectors, ushort size, byte density)?> perTrackGeometry);
}
