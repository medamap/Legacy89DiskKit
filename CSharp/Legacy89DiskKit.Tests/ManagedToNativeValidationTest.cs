using Xunit;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.DiskImage.Model;
using System.Runtime.InteropServices;

namespace Legacy89DiskKit.Tests;

public class ManagedToNativeValidationTest
{
    static ManagedToNativeValidationTest()
    {
        // 1. Setup C++ library path for interop globally for the test class
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

    [Fact]
    public void FullWorkflow_ValidationMode_ShouldMatch()
    {
        // 2. Setup Validation Backend
        var managed = new ManagedNativeBridgeBackend();
        var native = new CppLibraryNativeBridgeBackend();
        var validation = new ValidationNativeBridgeBackend(managed, native);

        string tempDisk = Path.Combine(Path.GetTempPath(), $"validation-{Guid.NewGuid():N}.d88");

        try
        {
            // 3. Exercise Workflows directly through the Validation backend
            using (var diskService = new DiskService(validation, validation.GetDefaultRegistry()))
            {
                // Create
                diskService.CreateDisk(tempDisk, DiskType.TwoD, "VALTEST");
                
                // Open and Format (auto-detects based on default provider logic)
                diskService.OpenDisk(tempDisk, readOnly: false);
                var fs = diskService.FileSystem!;
                fs.Format();

                // Info Flow
                var info = fs.GetFileSystemInfo();
                Assert.NotNull(info);

                // Write File (Inject)
                byte[] data = { 0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x11, 0x22, 0x33 };
                var attr = fs.CreateDefaultAttributes(isAscii: false);
                fs.WriteFile("TEST.BIN", data, attr);

                // List Files
                var files = fs.GetFiles().ToList();
                Assert.Contains(files, f => f.FullName == "TEST.BIN");

                // Read back (Extract)
                byte[] readData = fs.ReadFile("TEST.BIN");
                Assert.Equal(data, readData.Take(data.Length).ToArray());

                // Rename Flow
                // Use a short name to avoid N88-BASIC's 6.3 truncation logic issues in exact matching
                fs.RenameFile("TEST.BIN", "NEW.BIN");
                files = fs.GetFiles().ToList();
                Assert.DoesNotContain(files, f => f.FullName == "TEST.BIN");
                Assert.Contains(files, f => f.FullName == "NEW.BIN");

                // Delete Flow
                fs.DeleteFile("NEW.BIN");
                files = fs.GetFiles().ToList();
                Assert.DoesNotContain(files, f => f.FullName == "NEW.BIN");

                // Boot Area (Clone base) Flow
                if (fs.Capabilities.HasFlag(Legacy89DiskKit.Domain.FileSystem.Model.FileSystemCapabilities.SupportsBootArea))
                {
                    byte[] bootData = new byte[256];
                    bootData[0] = 0x01;
                    bootData[1] = 0xAA;
                    bootData[255] = 0x55;
                    fs.WriteBootArea(bootData);
                    
                    byte[] readBootData = fs.ReadBootArea();
                    Assert.Equal(0x01, readBootData[0]);
                    Assert.Equal(0xAA, readBootData[1]);
                    Assert.Equal(0x55, readBootData[255]);
                }
            }
        }
        finally
        {
            if (File.Exists(tempDisk)) File.Delete(tempDisk);
        }
    }
}
