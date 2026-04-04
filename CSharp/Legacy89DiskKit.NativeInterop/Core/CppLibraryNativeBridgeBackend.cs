using System.Runtime.InteropServices;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.Native.Domain;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class CppLibraryNativeBridgeBackend : INativeBridgeBackend
{
    static CppLibraryNativeBridgeBackend()
    {
        // Setup default resolver if not already set
        string libPath = "/tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp/";
        string libName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Legacy89DiskKitCpp.dll" :
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "libLegacy89DiskKitCpp.dylib" : "libLegacy89DiskKitCpp.so";
        
        string fullLibPath = Path.Combine(libPath, libName);

        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryImports).Assembly, (name, assembly, path) => {
            if (name == "Legacy89DiskKitCpp") 
            {
                if (File.Exists(fullLibPath)) return NativeLibrary.Load(fullLibPath);
                return NativeLibrary.Load(libName);
            }
            return IntPtr.Zero;
        });
    }

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

    public INativeDiskSession OpenDisk(byte[] imageData, string imageFormat, bool readOnly)
    {
        int handle = NativeLibraryImports.OpenDiskFromBuffer(imageData, imageData.Length, readOnly ? 1 : 0);
        if (handle < 0)
        {
            throw new Exception($"Failed to open disk from buffer (Error code: {handle})");
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
