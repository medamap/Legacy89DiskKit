using Xunit;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.Domain.DiskImage.Model;
using System.Runtime.InteropServices;

namespace Legacy89DiskKit.Tests;

public class ManagedToNativeValidationTest
{
    [Fact]
    public void FullWorkflow_ValidationMode_ShouldMatch()
    {
        // 1. Setup C++ library path for interop (Must be first)
        string libPath = "/tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp/";
        string libName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Legacy89DiskKitCpp.dll" :
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "libLegacy89DiskKitCpp.dylib" : "libLegacy89DiskKitCpp.so";
        
        string fullLibPath = Path.Combine(libPath, libName);
        if (!File.Exists(fullLibPath))
        {
            fullLibPath = libName; 
        }

        // Setup the resolver before any P/Invoke class is used
        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryImports).Assembly, (name, assembly, path) => {
            if (name == "Legacy89DiskKitCpp") 
            {
                return NativeLibrary.Load(fullLibPath);
            }
            return IntPtr.Zero;
        });

        // 2. Setup Validation Backend
        var managed = new ManagedNativeBridgeBackend();
        var native = new CppLibraryNativeBridgeBackend();
        var validation = new ValidationNativeBridgeBackend(managed, native);

        string tempDisk = Path.Combine(Path.GetTempPath(), $"validation-{Guid.NewGuid():N}.d88");

        try
        {
            // 3. Exercise Workflows directly through the Validation backend
            // This ensures ValidationDiskSession and ValidationFileSystem are used.
            
            // Create
            using (var session = validation.CreateDisk(tempDisk, DiskType.TwoD, "VALTEST"))
            {
                var fs = session.FileSystem!;
                fs.Format();

                // Write File (Validated inside ValidationFileSystem)
                byte[] data = { 0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x11, 0x22, 0x33 };
                var attr = fs.CreateDefaultAttributes(isAscii: false);
                fs.WriteFile("TEST.BIN", data, attr);

                // List Files (Validated inside ValidationFileSystem)
                var files = fs.GetFiles().ToList();
                Assert.Contains(files, f => f.FullName == "TEST.BIN");

                // Read back and compare (SequenceEqual check happens inside ValidationFileSystem)
                byte[] readData = fs.ReadFile("TEST.BIN");
                
                // Assert.Equal on byte arrays does a deep comparison in xUnit
                Assert.Equal(data, readData.Take(data.Length).ToArray());
            }
        }
        finally
        {
            // Clean up main temp disk
            if (File.Exists(tempDisk)) File.Delete(tempDisk);
            
            // Note: ValidationDiskSession handles cleanup of the .target disk image.
        }
    }
}
