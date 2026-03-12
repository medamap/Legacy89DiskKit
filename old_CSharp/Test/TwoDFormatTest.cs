using Legacy89DiskKit.DiskImage.Domain.Exception;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.DiskImage.Infrastructure.Factory;

namespace Legacy89DiskKit.Test;

public class TwoDFormatTest
{
    private static readonly string TestFilesDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Documents", "emulator", "x1turboROM", "DISK");

    private static readonly string TestFile2D = Path.Combine(TestFilesDirectory, "FT1_265.2D");

    public static void RunTests()
    {
        Console.WriteLine("Running 2D format tests...");
        
        try
        {
            Test2DFileExists();
            TestOpen2DContainer();
            Test2DContainerProperties();
            Test2DReadSector();
            Test2DSectorExists();
            Test2DGetAllSectors();
            TestCreate2DDisk();
            Test2DWriteAndRead();
            Test2DInvalidAddress();
            
            Console.WriteLine("All 2D format tests passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"2D format test failed: {ex.Message}");
            throw;
        }
    }

    private static void Test2DFileExists()
    {
        if (!File.Exists(TestFile2D))
        {
            throw new FileNotFoundException($"Test file not found: {TestFile2D}");
        }
        Console.WriteLine("✓ 2D test file exists");
    }

    private static void TestOpen2DContainer()
    {
        var factory = new DiskContainerFactory();
        using var container = factory.OpenDiskImage(TestFile2D, readOnly: true);

        if (container == null)
            throw new Exception("Container is null");
        if (!(container is TwoDDiskContainer))
            throw new Exception("Container is not TwoDDiskContainer");
        if (container.DiskType != DiskType.TwoD)
            throw new Exception("DiskType is not TwoD");
        if (!container.IsReadOnly)
            throw new Exception("Container should be read-only");
        if (container.FilePath != TestFile2D)
            throw new Exception("FilePath mismatch");
            
        Console.WriteLine("✓ 2D container opens correctly");
    }

    private static void Test2DContainerProperties()
    {
        using var container = new TwoDDiskContainer(TestFile2D, readOnly: true);

        if (container.DiskType != DiskType.TwoD)
            throw new Exception("DiskType is not TwoD");
        if (!container.IsReadOnly)
            throw new Exception("Container should be read-only");
        if (container.FilePath != TestFile2D)
            throw new Exception("FilePath mismatch");
            
        Console.WriteLine("✓ 2D container properties are correct");
    }

    private static void Test2DReadSector()
    {
        using var container = new TwoDDiskContainer(TestFile2D, readOnly: true);

        var sectorData = container.ReadSector(0, 0, 1);

        if (sectorData == null)
            throw new Exception("Sector data is null");
        if (sectorData.Length != 256)
            throw new Exception($"Expected 256 bytes, got {sectorData.Length}");
        if (sectorData[0] != 0x01)
            throw new Exception("First byte should be 0x01");
        if (sectorData[1] != (byte)'K')
            throw new Exception("Second byte should be 'K'");
            
        Console.WriteLine("✓ 2D sector reading works correctly");
    }

    private static void Test2DSectorExists()
    {
        using var container = new TwoDDiskContainer(TestFile2D, readOnly: true);

        if (!container.SectorExists(0, 0, 1))
            throw new Exception("Sector (0,0,1) should exist");
        if (!container.SectorExists(39, 1, 16))
            throw new Exception("Sector (39,1,16) should exist");
        if (container.SectorExists(40, 0, 1))
            throw new Exception("Sector (40,0,1) should not exist");
        if (container.SectorExists(0, 2, 1))
            throw new Exception("Sector (0,2,1) should not exist");
            
        Console.WriteLine("✓ 2D sector existence checks work correctly");
    }

    private static void Test2DGetAllSectors()
    {
        using var container = new TwoDDiskContainer(TestFile2D, readOnly: true);

        var sectors = container.GetAllSectors().ToList();

        if (sectors.Count != 1280) // 40 * 2 * 16 = 1280
            throw new Exception($"Expected 1280 sectors, got {sectors.Count}");
        
        var firstSector = sectors.First();
        if (firstSector.Cylinder != 0 || firstSector.Head != 0 || firstSector.Sector != 1)
            throw new Exception("First sector address is incorrect");
        if (firstSector.Size != 256)
            throw new Exception("First sector size is incorrect");

        var lastSector = sectors.Last();
        if (lastSector.Cylinder != 39 || lastSector.Head != 1 || lastSector.Sector != 16)
            throw new Exception("Last sector address is incorrect");
            
        Console.WriteLine("✓ 2D GetAllSectors works correctly");
    }

    private static void TestCreate2DDisk()
    {
        var tempFile = Path.GetTempFileName();
        var temp2DFile = Path.ChangeExtension(tempFile, ".2d");
        
        try
        {
            var factory = new DiskContainerFactory();
            using var container = factory.CreateNewDiskImage(temp2DFile, DiskType.TwoD, "TEST DISK");

            if (container == null)
                throw new Exception("Container is null");
            if (!(container is TwoDDiskContainer))
                throw new Exception("Container is not TwoDDiskContainer");
            if (container.DiskType != DiskType.TwoD)
                throw new Exception("DiskType is not TwoD");
            if (container.IsReadOnly)
                throw new Exception("Container should not be read-only");
            
            if (!File.Exists(temp2DFile))
                throw new Exception("2D file was not created");
            var fileInfo = new FileInfo(temp2DFile);
            if (fileInfo.Length != 327680)
                throw new Exception($"Expected 327680 bytes, got {fileInfo.Length}");
                
            Console.WriteLine("✓ 2D disk creation works correctly");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            if (File.Exists(temp2DFile))
                File.Delete(temp2DFile);
        }
    }

    private static void Test2DWriteAndRead()
    {
        var tempFile = Path.GetTempFileName();
        var temp2DFile = Path.ChangeExtension(tempFile, ".2d");
        
        try
        {
            var factory = new DiskContainerFactory();
            using (var container = factory.CreateNewDiskImage(temp2DFile, DiskType.TwoD, "TEST DISK"))
            {
                var testData = new byte[256];
                for (int i = 0; i < 256; i++)
                {
                    testData[i] = (byte)(i % 256);
                }

                container.WriteSector(0, 0, 1, testData);
                container.Save();
            }

            using (var container = factory.OpenDiskImage(temp2DFile, readOnly: true))
            {
                var readData = container.ReadSector(0, 0, 1);
                
                if (readData.Length != 256)
                    throw new Exception($"Expected 256 bytes, got {readData.Length}");
                for (int i = 0; i < 256; i++)
                {
                    if (readData[i] != (byte)(i % 256))
                        throw new Exception($"Data mismatch at position {i}");
                }
            }
            
            Console.WriteLine("✓ 2D write and read works correctly");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            if (File.Exists(temp2DFile))
                File.Delete(temp2DFile);
        }
    }

    private static void Test2DInvalidAddress()
    {
        using var container = new TwoDDiskContainer(TestFile2D, readOnly: true);

        try
        {
            container.ReadSector(-1, 0, 1);
            throw new Exception("Should have thrown ArgumentOutOfRangeException for cylinder -1");
        }
        catch (ArgumentOutOfRangeException) { }

        try
        {
            container.ReadSector(40, 0, 1);
            throw new Exception("Should have thrown ArgumentOutOfRangeException for cylinder 40");
        }
        catch (ArgumentOutOfRangeException) { }

        try
        {
            container.ReadSector(0, 2, 1);
            throw new Exception("Should have thrown ArgumentOutOfRangeException for head 2");
        }
        catch (ArgumentOutOfRangeException) { }

        try
        {
            container.ReadSector(0, 0, 0);
            throw new Exception("Should have thrown ArgumentOutOfRangeException for sector 0");
        }
        catch (ArgumentOutOfRangeException) { }

        try
        {
            container.ReadSector(0, 0, 17);
            throw new Exception("Should have thrown ArgumentOutOfRangeException for sector 17");
        }
        catch (ArgumentOutOfRangeException) { }
        
        Console.WriteLine("✓ 2D invalid address handling works correctly");
    }
}