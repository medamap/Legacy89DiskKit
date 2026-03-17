using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class CppLibraryNativeBridgeBackend : INativeBridgeBackend
{
    public string BackendKind => "native-library-bridge";

    public string BackendImplementation => "libLegacy89DiskKitCpp";

    public string BackendTarget => "C++ Infrastructure/Application";

    public INativeDiskSession OpenDisk(string path, bool readOnly)
    {
        int handle = NativeLibraryImports.OpenDisk(path, readOnly ? 1 : 0);
        if (handle < 0)
        {
            throw new Exception($"Failed to open disk: {path} (Error code: {handle})");
        }
        return new LibraryNativeDiskSession(handle);
    }

    public INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName)
    {
        int handle = NativeLibraryImports.CreateDisk(path, (int)diskType, diskName);
        if (handle < 0)
        {
            throw new Exception($"Failed to create disk: {path} (Error code: {handle})");
        }
        return new LibraryNativeDiskSession(handle);
    }
}
