using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Legacy89DiskKit.DependencyInjection;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Factory;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Application;

namespace Legacy89DiskKit.Test;

public class ComprehensiveTestSuite
{
    private readonly IDiskContainerFactory _diskContainerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly CharacterEncodingService _characterEncodingService;
    private readonly List<TestResult> _results = new();
    private readonly string _testDirectory;

    public ComprehensiveTestSuite()
    {
        var host = CreateHostBuilder().Build();
        _diskContainerFactory = host.Services.GetRequiredService<IDiskContainerFactory>();
        _fileSystemFactory = host.Services.GetRequiredService<IFileSystemFactory>();
        _characterEncodingService = host.Services.GetRequiredService<CharacterEncodingService>();
        
        _testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        Directory.CreateDirectory(_testDirectory);
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLegacy89DiskKit();
            });

    public void RunAllTests()
    {
        Console.WriteLine("🔍 Starting Comprehensive Test Suite...");
        Console.WriteLine($"Test Directory: {_testDirectory}");
        Console.WriteLine();

        _results.Clear();

        // テストマトリックス定義
        var fileSystemTypes = new[] { FileSystemType.HuBasic, FileSystemType.Fat12, FileSystemType.N88Basic, FileSystemType.MsxDos, FileSystemType.Cpm, FileSystemType.Cdos };
        var diskTypes = new[] { DiskType.TwoD, DiskType.TwoDD, DiskType.TwoHD };
        var containerExtensions = new[] { ".d88", ".dsk", ".2d" };
        var machineTypes = new[] { MachineType.X1, MachineType.Pc8801, MachineType.Msx1 };
        var cpmMachineTypes = new[] { MachineType.CpmGeneric, MachineType.CpmPc8801, MachineType.CpmX1, MachineType.CpmMsxDos };

        var testOperations = new[]
        {
            "CreateDiskImage",
            "FormatDisk", 
            "ImportTextFile",
            "ExportTextFile",
            "ImportBinaryFile",
            "ExportBinaryFile",
            "ListFiles",
            "DeleteFile",
            "DiskInfo"
        };

        int totalTests = 0;
        int executedTests = 0;

        foreach (var fileSystemType in fileSystemTypes)
        {
            foreach (var diskType in diskTypes)
            {
                foreach (var containerExt in containerExtensions)
                {
                    foreach (var operation in testOperations)
                    {
                        totalTests++;
                        
                        // DSK作成は今回実装済み
                        // (スキップコードを削除)

                        if (operation.Contains("Text"))
                        {
                            // テキスト操作は各機種でテスト
                            var testMachineTypes = fileSystemType == FileSystemType.Cpm ? cpmMachineTypes : machineTypes;
                            foreach (var machineType in testMachineTypes)
                            {
                                totalTests++;
                                executedTests++;
                                RunSingleTest(fileSystemType, diskType, containerExt, operation, machineType);
                            }
                        }
                        else
                        {
                            executedTests++;
                            RunSingleTest(fileSystemType, diskType, containerExt, operation, null);
                        }
                    }
                }
            }
        }

        Console.WriteLine($"\n📊 Test Execution Summary:");
        Console.WriteLine($"Total Test Cases: {_results.Count}");
        Console.WriteLine($"Executed: {_results.Count(r => r.Status != TestStatus.Skipped)}");
        Console.WriteLine($"Skipped: {_results.Count(r => r.Status == TestStatus.Skipped)}");
        Console.WriteLine();

        GenerateDetailedReport();
        GenerateSummaryReport();
        CleanupTestFiles();
    }

    private void RunSingleTest(FileSystemType fileSystemType, DiskType diskType, string containerExt, string operation, MachineType? machineType)
    {
        // N88-BASICはTwoHDをサポートしていない
        if (fileSystemType == FileSystemType.N88Basic && diskType == DiskType.TwoHD)
        {
            var skipTestName = $"{fileSystemType}_{diskType}_{containerExt.Replace(".", "")}_{operation}" + 
                              (machineType.HasValue ? $"_{machineType}" : "");
            Console.WriteLine($"⏩ Skipping {skipTestName} (N88-BASIC doesn't support TwoHD)");
            
            _results.Add(new TestResult
            {
                FileSystemType = fileSystemType,
                DiskType = diskType,
                ContainerType = containerExt,
                Operation = operation,
                MachineType = machineType,
                Status = TestStatus.Skipped,
                Message = "N88-BASIC doesn't support TwoHD disk type"
            });
            return;
        }
        
        // MSX-DOSは720KB (TwoDD)が標準、他のディスクタイプもテスト対象に含める
        if (fileSystemType == FileSystemType.MsxDos && (diskType == DiskType.TwoD || diskType == DiskType.TwoHD))
        {
            var skipTestName = $"{fileSystemType}_{diskType}_{containerExt.Replace(".", "")}_{operation}" + 
                              (machineType.HasValue ? $"_{machineType}" : "");
            Console.WriteLine($"⏩ Skipping {skipTestName} (MSX-DOS primarily supports TwoDD/720KB)");
            
            _results.Add(new TestResult
            {
                FileSystemType = fileSystemType,
                DiskType = diskType,
                ContainerType = containerExt,
                Operation = operation,
                MachineType = machineType,
                Status = TestStatus.Skipped,
                Message = "MSX-DOS primarily supports TwoDD (720KB) disk type"
            });
            return;
        }
        
        // CDOSは2D/2HDをサポート、2DDは非対応
        if (fileSystemType == FileSystemType.Cdos && diskType == DiskType.TwoDD)
        {
            var skipTestName = $"{fileSystemType}_{diskType}_{containerExt.Replace(".", "")}_{operation}" + 
                              (machineType.HasValue ? $"_{machineType}" : "");
            Console.WriteLine($"⏩ Skipping {skipTestName} (CDOS doesn't support TwoDD)");
            
            _results.Add(new TestResult
            {
                FileSystemType = fileSystemType,
                DiskType = diskType,
                ContainerType = containerExt,
                Operation = operation,
                MachineType = machineType,
                Status = TestStatus.Skipped,
                Message = "CDOS doesn't support TwoDD disk type"
            });
            return;
        }
        
        var testName = $"{fileSystemType}_{diskType}_{containerExt.Replace(".", "")}_{operation}" + 
                       (machineType.HasValue ? $"_{machineType}" : "");
        
        var testFile = Path.Combine(_testDirectory, $"{testName}{containerExt}");
        
        var result = new TestResult
        {
            FileSystemType = fileSystemType,
            DiskType = diskType,
            ContainerType = containerExt,
            Operation = operation,
            MachineType = machineType,
            TestFile = testFile
        };

        try
        {
            Console.Write($"Testing {testName}... ");
            
            switch (operation)
            {
                case "CreateDiskImage":
                    TestCreateDiskImage(result);
                    break;
                case "FormatDisk":
                    TestFormatDisk(result);
                    break;
                case "ImportTextFile":
                    TestImportTextFile(result);
                    break;
                case "ExportTextFile":
                    TestExportTextFile(result);
                    break;
                case "ImportBinaryFile":
                    TestImportBinaryFile(result);
                    break;
                case "ExportBinaryFile":
                    TestExportBinaryFile(result);
                    break;
                case "ListFiles":
                    TestListFiles(result);
                    break;
                case "DeleteFile":
                    TestDeleteFile(result);
                    break;
                case "DiskInfo":
                    TestDiskInfo(result);
                    break;
                default:
                    result.Status = TestStatus.Skipped;
                    result.Message = "Unknown operation";
                    break;
            }
            
            if (result.Status == TestStatus.Unknown)
                result.Status = TestStatus.Success;
                
            Console.WriteLine($"✅ {result.Status}");
        }
        catch (Exception ex)
        {
            result.Status = TestStatus.Failed;
            result.Message = ex.Message;
            result.Exception = ex;
            Console.WriteLine($"❌ FAILED: {ex.Message}");
        }

        _results.Add(result);
    }

    private void TestCreateDiskImage(TestResult result)
    {
        if (File.Exists(result.TestFile))
            File.Delete(result.TestFile);

        using var container = _diskContainerFactory.CreateNewDiskImage(result.TestFile, result.DiskType, "TEST");
        
        if (!File.Exists(result.TestFile))
            throw new Exception("Disk image file was not created");
            
        result.Message = $"Created {new FileInfo(result.TestFile).Length} bytes";
    }

    private void TestFormatDisk(TestResult result)
    {
        EnsureDiskExists(result);
        
        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile);
        var fileSystem = _fileSystemFactory.CreateFileSystem(container, result.FileSystemType);
        fileSystem.Format();
        
        result.Message = "Disk formatted successfully";
    }

    private void TestImportTextFile(TestResult result)
    {
        EnsureFormattedDisk(result);
        
        var testText = "Hello World!\nこれはテストです。\n機種: " + result.MachineType;
        var hostFile = Path.ChangeExtension(result.TestFile, ".txt");
        File.WriteAllText(hostFile, testText, Encoding.UTF8);

        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile);
        var fileSystem = _fileSystemFactory.OpenFileSystem(container, result.FileSystemType);
        
        var machineData = _characterEncodingService.EncodeText(testText, result.MachineType!.Value);
        fileSystem.WriteFile("TEST.TXT", machineData, isText: true);
        
        result.Message = $"Imported {machineData.Length} bytes";
    }

    private void TestExportTextFile(TestResult result)
    {
        // Import first
        TestImportTextFile(result);
        
        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile, readOnly: true);
        var fileSystem = _fileSystemFactory.OpenFileSystem(container, result.FileSystemType);
        
        var machineData = fileSystem.ReadFile("TEST.TXT");
        var hostText = _characterEncodingService.DecodeText(machineData, result.MachineType!.Value);
        
        var exportFile = Path.ChangeExtension(result.TestFile, ".export.txt");
        File.WriteAllText(exportFile, hostText, Encoding.UTF8);
        
        result.Message = $"Exported {hostText.Length} characters";
    }

    private void TestImportBinaryFile(TestResult result)
    {
        EnsureFormattedDisk(result);
        
        var binaryData = Encoding.ASCII.GetBytes("BINARY_TEST_DATA_" + DateTime.Now.Ticks);
        var hostFile = Path.ChangeExtension(result.TestFile, ".bin");
        File.WriteAllBytes(hostFile, binaryData);

        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile);
        var fileSystem = _fileSystemFactory.OpenFileSystem(container, result.FileSystemType);
        fileSystem.WriteFile("TEST.COM", binaryData, isText: false, 0x8000, 0x8000);
        
        result.Message = $"Imported {binaryData.Length} bytes";
    }

    private void TestExportBinaryFile(TestResult result)
    {
        // Import first
        TestImportBinaryFile(result);
        
        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile, readOnly: true);
        var fileSystem = _fileSystemFactory.OpenFileSystem(container, result.FileSystemType);
        
        var binaryData = fileSystem.ReadFile("TEST.COM");
        var exportFile = Path.ChangeExtension(result.TestFile, ".export.bin");
        File.WriteAllBytes(exportFile, binaryData);
        
        result.Message = $"Exported {binaryData.Length} bytes";
    }

    private void TestListFiles(TestResult result)
    {
        EnsureFormattedDisk(result);
        
        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile, readOnly: true);
        var fileSystem = _fileSystemFactory.OpenFileSystem(container, result.FileSystemType);
        
        var files = fileSystem.GetFiles().ToList();
        result.Message = $"Listed {files.Count} files";
    }

    private void TestDeleteFile(TestResult result)
    {
        // Add a file first
        TestImportBinaryFile(result);
        
        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile);
        var fileSystem = _fileSystemFactory.OpenFileSystem(container, result.FileSystemType);
        
        fileSystem.DeleteFile("TEST.COM");
        
        var files = fileSystem.GetFiles().ToList();
        if (files.Any(f => f.FileName.Equals("TEST", StringComparison.OrdinalIgnoreCase)))
            throw new Exception("File was not deleted");
            
        result.Message = "File deleted successfully";
    }

    private void TestDiskInfo(TestResult result)
    {
        EnsureDiskExists(result);
        
        using var container = _diskContainerFactory.OpenDiskImage(result.TestFile, readOnly: true);
        
        result.Message = $"Type: {container.DiskType}, ReadOnly: {container.IsReadOnly}";
    }

    private void EnsureDiskExists(TestResult result)
    {
        if (!File.Exists(result.TestFile))
        {
            TestCreateDiskImage(result);
        }
    }

    private void EnsureFormattedDisk(TestResult result)
    {
        EnsureDiskExists(result);
        
        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(result.TestFile, readOnly: true);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, result.FileSystemType);
            var files = fileSystem.GetFiles().ToList(); // Test if formatted
        }
        catch
        {
            TestFormatDisk(result);
        }
    }

    private void GenerateDetailedReport()
    {
        var reportFile = Path.Combine(_testDirectory, "DetailedTestReport.txt");
        using var writer = new StreamWriter(reportFile);
        
        writer.WriteLine("COMPREHENSIVE TEST SUITE - DETAILED REPORT");
        writer.WriteLine("==========================================");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine();

        foreach (var group in _results.GroupBy(r => r.Status))
        {
            writer.WriteLine($"{group.Key.ToString().ToUpper()} TESTS ({group.Count()}):");
            writer.WriteLine(new string('-', 50));
            
            foreach (var result in group.OrderBy(r => r.GetTestName()))
            {
                writer.WriteLine($"  {result.GetTestName()}");
                writer.WriteLine($"    Status: {result.Status}");
                writer.WriteLine($"    Message: {result.Message ?? "N/A"}");
                if (result.Exception != null)
                {
                    writer.WriteLine($"    Exception: {result.Exception.GetType().Name}");
                    writer.WriteLine($"    Details: {result.Exception.Message}");
                }
                writer.WriteLine();
            }
        }
        
        Console.WriteLine($"📄 Detailed report saved: {reportFile}");
    }

    private void GenerateSummaryReport()
    {
        var reportFile = Path.Combine(_testDirectory, "TestSummaryMatrix.txt");
        using var writer = new StreamWriter(reportFile);
        
        writer.WriteLine("TEST MATRIX SUMMARY");
        writer.WriteLine("==================");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine();

        // Success rate by category
        writer.WriteLine("SUCCESS RATES BY CATEGORY:");
        writer.WriteLine(new string('-', 40));
        
        foreach (var fsGroup in _results.GroupBy(r => r.FileSystemType))
        {
            var total = fsGroup.Count();
            var success = fsGroup.Count(r => r.Status == TestStatus.Success);
            var successRate = total > 0 ? (success * 100.0 / total) : 0;
            writer.WriteLine($"  {fsGroup.Key}: {success}/{total} ({successRate:F1}%)");
        }
        
        writer.WriteLine();
        
        foreach (var diskGroup in _results.GroupBy(r => r.DiskType))
        {
            var total = diskGroup.Count();
            var success = diskGroup.Count(r => r.Status == TestStatus.Success);
            var successRate = total > 0 ? (success * 100.0 / total) : 0;
            writer.WriteLine($"  {diskGroup.Key}: {success}/{total} ({successRate:F1}%)");
        }
        
        writer.WriteLine();
        writer.WriteLine("OPERATION SUCCESS RATES:");
        writer.WriteLine(new string('-', 40));
        
        foreach (var opGroup in _results.GroupBy(r => r.Operation))
        {
            var total = opGroup.Count();
            var success = opGroup.Count(r => r.Status == TestStatus.Success);
            var successRate = total > 0 ? (success * 100.0 / total) : 0;
            writer.WriteLine($"  {opGroup.Key}: {success}/{total} ({successRate:F1}%)");
        }

        var overallTotal = _results.Count;
        var overallSuccess = _results.Count(r => r.Status == TestStatus.Success);
        var overallRate = overallTotal > 0 ? (overallSuccess * 100.0 / overallTotal) : 0;
        
        writer.WriteLine();
        writer.WriteLine($"OVERALL SUCCESS RATE: {overallSuccess}/{overallTotal} ({overallRate:F1}%)");
        
        Console.WriteLine($"📊 Summary report saved: {reportFile}");
        Console.WriteLine($"🎯 Overall Success Rate: {overallRate:F1}% ({overallSuccess}/{overallTotal})");
    }

    private void CleanupTestFiles()
    {
        try
        {
            var testFiles = Directory.GetFiles(_testDirectory, "*", SearchOption.AllDirectories)
                                    .Where(f => !f.EndsWith(".txt"))
                                    .ToList();
            
            foreach (var file in testFiles)
            {
                File.Delete(file);
            }
            
            Console.WriteLine($"🧹 Cleaned up {testFiles.Count} test files");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Cleanup warning: {ex.Message}");
        }
    }
}

public class TestResult
{
    public FileSystemType FileSystemType { get; set; }
    public DiskType DiskType { get; set; }
    public string ContainerType { get; set; } = "";
    public string Operation { get; set; } = "";
    public MachineType? MachineType { get; set; }
    public TestStatus Status { get; set; } = TestStatus.Unknown;
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public string TestFile { get; set; } = "";

    public string GetTestName()
    {
        var machineSuffix = MachineType.HasValue ? $"_{MachineType}" : "";
        return $"{FileSystemType}_{DiskType}_{ContainerType.Replace(".", "")}_{Operation}{machineSuffix}";
    }
}

public enum TestStatus
{
    Unknown,
    Success,
    Failed,
    Skipped
}