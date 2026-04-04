using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem;

public class HuBasicTransferAdapterTest
{
    [Fact]
    public void ExportImport_PreservesFileNameWithSpace()
    {
        // Setup
        string path1 = Path.Combine(Path.GetTempPath(), "test1.d88");
        string path2 = Path.Combine(Path.GetTempPath(), "test2.d88");
        
        try
        {
            using var disk1 = D88DiskContainer.CreateNew(path1, DiskType.TwoD, "TEST1");
            using var disk2 = D88DiskContainer.CreateNew(path2, DiskType.TwoD, "TEST2");
            
            using var fs1 = new HuBasicFileSystem(disk1);
            using var fs2 = new HuBasicFileSystem(disk2);
            
            fs1.Format();
            fs2.Format();
            
            string fileName = "Start up.Bas";
            byte[] data = { 0x01, 0x02, 0x03 };
            var attrs = fs1.CreateDefaultAttributes(false);
            
            fs1.WriteFile(fileName, data, attrs);
            
            var adapter1 = new HuBasicTransferAdapter(fs1);
            var adapter2 = new HuBasicTransferAdapter(fs2);
            
            var entry = fs1.GetFiles().First(f => f.FullName == fileName);
            
            // Export
            var envelope = adapter1.Export(entry);
            
            // Import
            adapter2.Import(envelope, fileName);
            
            // Assert
            var copiedEntries = fs2.GetFiles().ToList();
            Assert.Single(copiedEntries);
            Assert.Equal(fileName, copiedEntries[0].FullName);
            Assert.Equal("Start up", copiedEntries[0].FileName);
            Assert.Equal("Bas", copiedEntries[0].Extension);
            
            byte[] copiedData = fs2.ReadFile(fileName);
            Assert.Equal(data, copiedData);
        }
        finally
        {
            if (File.Exists(path1)) File.Delete(path1);
            if (File.Exists(path2)) File.Delete(path2);
        }
    }

    [Fact]
    public void ExportImport_PreservesRawMetadata()
    {
        // Setup
        string path1 = Path.Combine(Path.GetTempPath(), "test_meta1.d88");
        string path2 = Path.Combine(Path.GetTempPath(), "test_meta2.d88");
        
        try
        {
            using var disk1 = D88DiskContainer.CreateNew(path1, DiskType.TwoD, "TEST1");
            using var disk2 = D88DiskContainer.CreateNew(path2, DiskType.TwoD, "TEST2");
            
            using var fs1 = new HuBasicFileSystem(disk1);
            using var fs2 = new HuBasicFileSystem(disk2);
            
            fs1.Format();
            fs2.Format();
            
            string fileName = "SECRET.BIN";
            byte[] data = { 0xDE, 0xAD, 0xBE, 0xEF };
            
            // Create a file with specific mode/password manually if needed, 
            // but for now let's use standard WriteFile and see if it preserves it.
            var attrs = fs1.CreateDefaultAttributes(false);
            fs1.WriteFile(fileName, data, attrs, loadAddress: 0x1234, executionAddress: 0x5678);
            
            var adapter1 = new HuBasicTransferAdapter(fs1);
            var adapter2 = new HuBasicTransferAdapter(fs2);
            
            var entry = fs1.GetFiles().First(f => f.FullName == fileName);
            
            // Export
            var envelope = adapter1.Export(entry);
            
            // Import
            adapter2.Import(envelope, fileName);
            
            // Assert
            var copiedEntry = fs2.GetFiles().First(f => f.FullName == fileName);
            Assert.Equal((ushort)0x1234, copiedEntry.LoadAddress);
            Assert.Equal((ushort)0x5678, copiedEntry.ExecutionAddress);
            Assert.Equal(entry.Attributes.RawAttributes, copiedEntry.Attributes.RawAttributes);
        }
        finally
        {
            if (File.Exists(path1)) File.Delete(path1);
            if (File.Exists(path2)) File.Delete(path2);
        }
    }
}
