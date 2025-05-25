using System;
using System.IO;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.Test;

public class BasicTest
{
    public static void RunTests()
    {
        Console.WriteLine("Running basic tests...");
        
        try
        {
            TestCreateDiskImage();
            TestFormatDisk();
            Console.WriteLine("All tests passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
        }
    }
    
    private static void TestCreateDiskImage()
    {
        Console.WriteLine("Testing disk image creation...");
        
        var service = new DiskImageService();
        var testFile = "test.d88";
        
        if (File.Exists(testFile))
            File.Delete(testFile);
            
        using var container = service.CreateNewDiskImage(testFile, DiskType.TwoD, "TEST DISK");
        
        if (!File.Exists(testFile))
            throw new Exception("Disk image file was not created");
            
        Console.WriteLine("✓ Disk image creation test passed");
        
        File.Delete(testFile);
    }
    
    private static void TestFormatDisk()
    {
        Console.WriteLine("Testing disk formatting...");
        
        var diskService = new DiskImageService();
        var fileService = new FileSystemService();
        var testFile = "test_format.d88";
        
        if (File.Exists(testFile))
            File.Delete(testFile);
            
        using var container = diskService.CreateNewDiskImage(testFile, DiskType.TwoD, "FORMAT TEST");
        fileService.FormatDisk(container);
        
        var fileSystem = fileService.OpenFileSystem(container);
        
        if (!fileSystem.IsFormatted)
            throw new Exception("File system was not formatted correctly");
            
        Console.WriteLine("✓ Disk formatting test passed");
        
        File.Delete(testFile);
    }
}

class Program
{
    static void Main(string[] args)
    {
        BasicTest.RunTests();
    }
}