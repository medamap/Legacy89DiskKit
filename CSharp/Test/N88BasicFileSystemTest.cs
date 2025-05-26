using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Legacy89DiskKit.DependencyInjection;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;

namespace Legacy89DiskKit.Test;

public class N88BasicFileSystemTest
{
    private static IDiskContainerFactory _diskContainerFactory = null!;
    private static IFileSystemFactory _fileSystemFactory = null!;
    
    public static void RunTests()
    {
        Console.WriteLine("🧪 Running N88-BASIC FileSystem Tests...");
        
        // DI設定
        var host = CreateHostBuilder().Build();
        _diskContainerFactory = host.Services.GetRequiredService<IDiskContainerFactory>();
        _fileSystemFactory = host.Services.GetRequiredService<IFileSystemFactory>();
        
        try
        {
            TestN88BasicConfiguration();
            TestN88BasicFileEntry();
            TestN88BasicFileNameValidator();
            TestN88BasicFileSystemBasics();
            Console.WriteLine("✅ All N88-BASIC tests passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ N88-BASIC test failed: {ex.Message}");
            throw;
        }
    }
    
    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLegacy89DiskKit();
            });
    
    private static void TestN88BasicConfiguration()
    {
        Console.WriteLine("Testing N88BasicConfiguration...");
        
        // 2D設定テスト
        var config2D = N88BasicConfiguration.Create(DiskType.TwoD);
        if (config2D.SystemTrack != 18 || config2D.SystemHead != 1)
            throw new Exception("2D system track location incorrect");
        if (config2D.SectorsPerCluster != 8 || config2D.ClusterSize != 2048)
            throw new Exception("2D cluster size incorrect");
        
        // 2DD設定テスト
        var config2DD = N88BasicConfiguration.Create(DiskType.TwoDD);
        if (config2DD.SystemTrack != 40 || config2DD.SystemHead != 0)
            throw new Exception("2DD system track location incorrect");
        if (config2DD.SectorsPerCluster != 16 || config2DD.ClusterSize != 4096)
            throw new Exception("2DD cluster size incorrect");
        
        // FATマーカーテスト
        if (!config2D.IsFreeCluster(0xFF))
            throw new Exception("Free cluster marker test failed");
        if (!config2D.IsEofMarker(0xC0, 0))
            throw new Exception("EOF marker test failed");
        
        Console.WriteLine("✓ N88BasicConfiguration tests passed");
    }
    
    private static void TestN88BasicFileEntry()
    {
        Console.WriteLine("Testing N88BasicFileEntry...");
        
        // バイト配列からの作成テスト
        var entryBytes = new byte[16];
        Encoding.ASCII.GetBytes("HELLO").CopyTo(entryBytes, 0);
        entryBytes[5] = 0x20; // スペース埋め
        Encoding.ASCII.GetBytes("TXT").CopyTo(entryBytes, 6);
        entryBytes[9] = 0x80;  // トークン化BASIC
        entryBytes[10] = 5;    // 開始クラスタ
        
        var entry = N88BasicFileEntry.FromBytes(entryBytes);
        if (entry.FileName != "HELLO" || entry.Extension != "TXT")
            throw new Exception("File name parsing failed");
        if (!entry.IsTokenizedBasic || entry.StartCluster != 5)
            throw new Exception("Attribute parsing failed");
        
        // バイト配列への変換テスト
        var newEntry = new N88BasicFileEntry
        {
            FileName = "TEST",
            Extension = "BAS",
            Attributes = 0x81,
            StartCluster = 10,
            Status = N88BasicEntryStatus.Active
        };
        
        var bytes = newEntry.ToBytes();
        if (bytes.Length != 16)
            throw new Exception("ToBytes size incorrect");
        if (Encoding.ASCII.GetString(bytes, 0, 4) != "TEST")
            throw new Exception("ToBytes filename incorrect");
        if (bytes[9] != 0x81 || bytes[10] != 10)
            throw new Exception("ToBytes attributes incorrect");
        
        // 属性プロパティテスト
        newEntry.IsBinary = true;
        if ((newEntry.Attributes & 0x01) == 0)
            throw new Exception("Binary attribute setter failed");
        
        newEntry.IsWriteProtected = true;
        if ((newEntry.Attributes & 0x10) == 0)
            throw new Exception("Write protected attribute setter failed");
        
        Console.WriteLine("✓ N88BasicFileEntry tests passed");
    }
    
    private static void TestN88BasicFileNameValidator()
    {
        Console.WriteLine("Testing N88BasicFileNameValidator...");
        
        // 有効なファイル名テスト
        var result = N88BasicFileNameValidator.ValidateFileName("TEST.TXT");
        if (!result.IsValid || result.BaseName != "TEST" || result.Extension != "TXT")
            throw new Exception("Valid filename validation failed");
        
        // 無効なファイル名テスト（長すぎる）
        result = N88BasicFileNameValidator.ValidateFileName("TOOLONG");
        if (result.IsValid)
            throw new Exception("Invalid filename (too long) should be rejected");
        
        // 予約語テスト
        result = N88BasicFileNameValidator.ValidateFileName("CON");
        if (result.IsValid)
            throw new Exception("Reserved name should be rejected");
        
        // 正規化テスト
        var normalized = N88BasicFileNameValidator.NormalizeFileName("hello.txt");
        if (normalized != "HELLO.TXT")
            throw new Exception("Normalization failed");
        
        // 修正候補生成テスト
        var alternatives = N88BasicFileNameValidator.GenerateAlternatives("TOOLONG.TOOLONGEXT");
        if (!alternatives.Any(alt => alt.Contains("TOOLON")))
            throw new Exception("Alternative generation failed");
        
        Console.WriteLine("✓ N88BasicFileNameValidator tests passed");
    }
    
    private static void TestN88BasicFileSystemBasics()
    {
        Console.WriteLine("Testing N88BasicFileSystem basics...");
        
        var testFile = "test_n88basic.d88";
        
        try
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
            
            // 2D ディスクイメージ作成・フォーマットテスト
            using (var container = _diskContainerFactory.CreateNewDiskImage(testFile, DiskType.TwoD, "N88TEST"))
            {
                var fileSystem = new N88BasicFileSystem(container);
                
                // フォーマットテスト
                fileSystem.Format();
                
                if (!fileSystem.IsFormatted)
                    throw new Exception("Disk formatting failed");
                
                // ファイル一覧テスト（空）
                var files = fileSystem.GetFiles();
                if (files.Any())
                    throw new Exception("Formatted disk should be empty");
                
                // ファイルシステム情報テスト
                var info = fileSystem.GetFileSystemInfo();
                if (info.TotalClusters <= 0 || info.FreeClusters <= 0)
                    throw new Exception("FileSystem info incorrect");
                
                Console.WriteLine($"✓ N88-BASIC 2D disk: {info.FreeClusters}/{info.TotalClusters} clusters free");
                
                // 簡単なファイル書き込み・読み取りテスト
                var testData = Encoding.ASCII.GetBytes("Hello N88-BASIC!");
                fileSystem.WriteFile("TEST.TXT", testData, isText: true);
                
                // ファイル一覧再確認
                files = fileSystem.GetFiles();
                if (!files.Any(f => f.FileName == "TEST" && f.Extension == "TXT"))
                    throw new Exception("Written file not found in file list");
                
                // ファイル読み取り
                var readData = fileSystem.ReadFile("TEST.TXT");
                if (!readData.SequenceEqual(testData))
                    throw new Exception("Read data doesn't match written data");
                
                // ファイル削除
                fileSystem.DeleteFile("TEST.TXT");
                files = fileSystem.GetFiles();
                if (files.Any(f => f.FileName == "TEST" && f.Extension == "TXT"))
                    throw new Exception("Deleted file still exists");
            }
            
            // 2DD テスト
            if (File.Exists(testFile))
                File.Delete(testFile);
            
            using (var container = _diskContainerFactory.CreateNewDiskImage(testFile, DiskType.TwoDD, "N88TEST2DD"))
            {
                var fileSystem = new N88BasicFileSystem(container);
                fileSystem.Format();
                
                var info = fileSystem.GetFileSystemInfo();
                Console.WriteLine($"✓ N88-BASIC 2DD disk: {info.FreeClusters}/{info.TotalClusters} clusters free");
                
                if (info.TotalClusters <= 80) // 2DDは2Dより多くないといけない
                    throw new Exception("2DD should have more clusters than 2D");
            }
            
            Console.WriteLine("✓ N88BasicFileSystem basic tests passed");
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }
}