using Legacy89DiskKit.DiskImage.Domain.Model;

namespace Legacy89DiskKit.Native.Domain;

public interface INativeBridgeBackend
{
    string BackendKind { get; }

    string BackendImplementation { get; }

    string BackendTarget { get; }

    INativeDiskSession OpenDisk(string path, bool readOnly);

    INativeDiskSession OpenDisk(byte[] imageData, string imageFormat, bool readOnly);

    INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName);
}
