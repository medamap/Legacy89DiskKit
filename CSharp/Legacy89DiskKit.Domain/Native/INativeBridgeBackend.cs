using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Domain.Native;

public interface INativeBridgeBackend
{
    string BackendKind { get; }

    string BackendImplementation { get; }

    string BackendTarget { get; }

    INativeDiskSession OpenDisk(string path, bool readOnly);

    INativeDiskSession OpenDisk(byte[] imageData, string imageFormat, bool readOnly);

    INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName);
}
