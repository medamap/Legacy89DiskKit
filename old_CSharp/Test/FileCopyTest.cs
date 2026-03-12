using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Legacy89DiskKit.DependencyInjection;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.DiskOperation.Domain.Interface;
using Legacy89DiskKit.DiskOperation.Domain.Model;

namespace Test;

public static class FileCopyTest
{
    public static async Task RunTests()
    {
        var services = new ServiceCollection();
        services.AddLegacy89DiskKit();
        var serviceProvider = services.BuildServiceProvider();

        var diskContainerFactory = serviceProvider.GetRequiredService<IDiskContainerFactory>();
        var fileSystemFactory = serviceProvider.GetRequiredService<IFileSystemFactory>();
        var fileCopyService = serviceProvider.GetRequiredService<IFileCopyService>();

        Console.WriteLine("=== File Copy Test ===");
        Console.WriteLine();

        await TestSameFileSystemCopy(diskContainerFactory, fileSystemFactory, fileCopyService);
        await TestCrossFileSystemCopy(diskContainerFactory, fileSystemFactory, fileCopyService);
        await TestFileNameConversion(diskContainerFactory, fileSystemFactory, fileCopyService);
    }

    private static async Task TestSameFileSystemCopy(
        IDiskContainerFactory diskContainerFactory,
        IFileSystemFactory fileSystemFactory,
        IFileCopyService fileCopyService)
    {
        Console.WriteLine("Test 1: Same filesystem copy (Hu-BASIC to Hu-BASIC)");

        try
        {
            // Create test disk images
            var sourcePath = "TestOutput/source_copy_test.d88";
            var destPath = "TestOutput/dest_copy_test.d88";
            Directory.CreateDirectory("TestOutput");

            // Create and format source disk
            using (var container = diskContainerFactory.CreateNewDiskImage(sourcePath, DiskType.TwoD, "SOURCE"))
            {
                var fs = fileSystemFactory.CreateFileSystem(container, FileSystemType.HuBasic);
                fs.Format();
                
                // Write test file
                var testData = "This is a test file for copying."u8.ToArray();
                fs.WriteFile("TEST.TXT", testData, isText: true);
            }

            // Create and format destination disk
            using (var container = diskContainerFactory.CreateNewDiskImage(destPath, DiskType.TwoD, "DEST"))
            {
                var fs = fileSystemFactory.CreateFileSystem(container, FileSystemType.HuBasic);
                fs.Format();
            }

            // Copy file
            var result = await fileCopyService.CopyFileAsync(
                sourcePath,
                destPath,
                "TEST.TXT",
                new FileCopyOptions { ConflictResolution = ConflictResolution.Overwrite });

            if (result.Success)
            {
                Console.WriteLine($"✓ Copy successful: {result.SourceFileName} -> {result.DestinationFileName}");
                Console.WriteLine($"  Bytes copied: {result.BytesCopied}");
                Console.WriteLine($"  Duration: {result.Duration.TotalMilliseconds}ms");
            }
            else
            {
                Console.WriteLine($"✗ Copy failed: {result.ErrorMessage}");
            }

            // Verify
            using (var container = diskContainerFactory.OpenDiskImage(destPath, true))
            {
                var fs = fileSystemFactory.OpenFileSystemReadOnly(container);
                var files = fs.GetFiles();
                Console.WriteLine($"  Files in destination: {string.Join(", ", files.Select(f => f.FileName))}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestCrossFileSystemCopy(
        IDiskContainerFactory diskContainerFactory,
        IFileSystemFactory fileSystemFactory,
        IFileCopyService fileCopyService)
    {
        Console.WriteLine("Test 2: Cross-filesystem copy (Hu-BASIC to FAT12)");

        try
        {
            var sourcePath = "TestOutput/hubasic_source.d88";
            var destPath = "TestOutput/fat12_dest.d88";

            // Reuse or create source disk from previous test
            if (!File.Exists(sourcePath))
            {
                using (var container = diskContainerFactory.CreateNewDiskImage(sourcePath, DiskType.TwoD, "HUBASIC"))
                {
                    var fs = fileSystemFactory.CreateFileSystem(container, FileSystemType.HuBasic);
                    fs.Format();
                    fs.WriteFile("CROSS.TXT", "Cross filesystem test"u8.ToArray(), isText: true);
                }
            }

            // Create FAT12 destination
            using (var container = diskContainerFactory.CreateNewDiskImage(destPath, DiskType.TwoDD, "FAT12DSK"))
            {
                var fs = fileSystemFactory.CreateFileSystem(container, FileSystemType.Fat12);
                fs.Format();
            }

            // Copy file
            var result = await fileCopyService.CopyFileAsync(
                sourcePath,
                destPath,
                "CROSS.TXT");

            Console.WriteLine($"  Result: {(result.Success ? "Success" : "Failed")}");
            if (result.Success)
            {
                Console.WriteLine($"  Conversion: {result.ConversionType}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestFileNameConversion(
        IDiskContainerFactory diskContainerFactory,
        IFileSystemFactory fileSystemFactory,
        IFileCopyService fileCopyService)
    {
        Console.WriteLine("Test 3: Long filename conversion");

        try
        {
            var sourcePath = "TestOutput/longname_source.d88";
            var destPath = "TestOutput/longname_dest.d88";

            // Create source with long filename
            using (var container = diskContainerFactory.CreateNewDiskImage(sourcePath, DiskType.TwoD, "LONGNAME"))
            {
                var fs = fileSystemFactory.CreateFileSystem(container, FileSystemType.HuBasic);
                fs.Format();
                fs.WriteFile("VERYLONGFILENAME.TXT", "Test data"u8.ToArray(), isText: true);
            }

            // Create destination
            using (var container = diskContainerFactory.CreateNewDiskImage(destPath, DiskType.TwoD, "DEST"))
            {
                var fs = fileSystemFactory.CreateFileSystem(container, FileSystemType.HuBasic);
                fs.Format();
            }

            // This should fail because filename is too long
            var result = await fileCopyService.CopyFileAsync(
                sourcePath,
                destPath,
                "VERYLONGFILENAME.TXT");

            Console.WriteLine($"  Result: {(result.Success ? "Success" : "Failed")}");
            if (result.Success)
            {
                Console.WriteLine($"  Original: {result.SourceFileName}");
                Console.WriteLine($"  Converted: {result.DestinationFileName}");
                Console.WriteLine($"  Conversion type: {result.ConversionType}");
            }
            else
            {
                Console.WriteLine($"  Error: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test failed: {ex.Message}");
        }

        Console.WriteLine();
    }
}