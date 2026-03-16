using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.NativeInterop.Core;

public interface INativeBridgeBackend
{
    string BackendKind { get; }

    string BackendImplementation { get; }

    string BackendTarget { get; }

    INativeDiskSession OpenDisk(string path, bool readOnly);

    INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName);
}
