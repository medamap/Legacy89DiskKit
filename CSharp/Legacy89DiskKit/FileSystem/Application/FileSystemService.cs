using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;

namespace Legacy89DiskKit.FileSystem.Application;

public class FileSystemService
{
    public IFileSystem OpenFileSystem(IDiskContainer diskContainer)
    {
        return new HuBasicFileSystem(diskContainer);
    }

    public void FormatDisk(IDiskContainer diskContainer)
    {
        var fileSystem = OpenFileSystem(diskContainer);
        fileSystem.Format();
        diskContainer.Save();
    }

    public void ImportTextFile(IFileSystem fileSystem, string hostFilePath, string diskFileName)
    {
        if (string.IsNullOrWhiteSpace(hostFilePath))
            throw new ArgumentException("Host file path cannot be null or empty", nameof(hostFilePath));
            
        if (string.IsNullOrWhiteSpace(diskFileName))
            throw new ArgumentException("Disk file name cannot be null or empty", nameof(diskFileName));
            
        if (!File.Exists(hostFilePath))
            throw new FileNotFoundException($"Host file not found: {hostFilePath}");
        
        // ファイルサイズ事前チェック
        var hostFileInfo = new FileInfo(hostFilePath);
        if (hostFileInfo.Length > 65535)
            throw new ArgumentException($"Host file too large: {hostFileInfo.Length:N0} bytes (max: 65,535)");
        
        // ディスク容量チェック
        var fsInfo = fileSystem.GetFileSystemInfo();
        var estimatedSizeNeeded = (int)hostFileInfo.Length + 100; // 余裕を持たせる
        var availableSpace = fsInfo.FreeClusters * fsInfo.ClusterSize;
        
        if (estimatedSizeNeeded > availableSpace)
            throw new InvalidOperationException($"Insufficient disk space: need ~{estimatedSizeNeeded:N0} bytes, available {availableSpace:N0} bytes");
        
        var hostText = File.ReadAllText(hostFilePath);
        var x1Data = X1Converter.WinToX1(hostText);
        var x1Text = X1Converter.ConvertLineEndings(hostText, false);
        var finalData = X1Converter.WinToX1(x1Text);
        
        // Add ASCII terminator
        var dataWithTerminator = new List<byte>(finalData);
        dataWithTerminator.AddRange(new byte[] { 0x0D, 0x1A });
        
        // 最終サイズチェック
        if (dataWithTerminator.Count > 65535)
            throw new ArgumentException($"Converted file too large: {dataWithTerminator.Count:N0} bytes (max: 65,535)");
        
        var attributes = new HuBasicFileAttributes(
            IsDirectory: false,
            IsReadOnly: false,
            IsVerify: false,
            IsHidden: false,
            IsBinary: false,
            IsBasic: false,
            IsAscii: true
        );
        
        fileSystem.WriteFile(diskFileName, dataWithTerminator.ToArray(), isText: true);
        fileSystem.DiskContainer.Save();
    }

    public void ExportTextFile(IFileSystem fileSystem, string diskFileName, string hostFilePath)
    {
        ExportTextFile(fileSystem, diskFileName, hostFilePath, false);
    }

    public void ExportTextFile(IFileSystem fileSystem, string diskFileName, string hostFilePath, bool allowPartialRead)
    {
        if (string.IsNullOrWhiteSpace(diskFileName))
            throw new ArgumentException("Disk file name cannot be null or empty", nameof(diskFileName));
            
        if (string.IsNullOrWhiteSpace(hostFilePath))
            throw new ArgumentException("Host file path cannot be null or empty", nameof(hostFilePath));

        byte[] fileData;
        if (fileSystem is HuBasicFileSystem huBasicFileSystem)
        {
            fileData = huBasicFileSystem.ReadFile(diskFileName, allowPartialRead);
        }
        else
        {
            fileData = fileSystem.ReadFile(diskFileName);
        }
        
        var winText = X1Converter.X1ToWin(fileData);
        var hostText = X1Converter.ConvertLineEndings(winText, true);
        
        File.WriteAllText(hostFilePath, hostText);
    }

    public void ImportBinaryFile(IFileSystem fileSystem, string hostFilePath, string diskFileName, 
        ushort loadAddress = 0, ushort executeAddress = 0)
    {
        if (string.IsNullOrWhiteSpace(hostFilePath))
            throw new ArgumentException("Host file path cannot be null or empty", nameof(hostFilePath));
            
        if (string.IsNullOrWhiteSpace(diskFileName))
            throw new ArgumentException("Disk file name cannot be null or empty", nameof(diskFileName));
            
        if (!File.Exists(hostFilePath))
            throw new FileNotFoundException($"Host file not found: {hostFilePath}");
        
        // ファイルサイズ事前チェック
        var hostFileInfo = new FileInfo(hostFilePath);
        if (hostFileInfo.Length > 65535)
            throw new ArgumentException($"Host file too large: {hostFileInfo.Length:N0} bytes (max: 65,535)");
        
        // ディスク容量チェック
        var fsInfo = fileSystem.GetFileSystemInfo();
        var sizeNeeded = (int)hostFileInfo.Length;
        var availableSpace = fsInfo.FreeClusters * fsInfo.ClusterSize;
        
        if (sizeNeeded > availableSpace)
            throw new InvalidOperationException($"Insufficient disk space: need {sizeNeeded:N0} bytes, available {availableSpace:N0} bytes");
        
        var binaryData = File.ReadAllBytes(hostFilePath);
        
        var attributes = new HuBasicFileAttributes(
            IsDirectory: false,
            IsReadOnly: false,
            IsVerify: false,
            IsHidden: false,
            IsBinary: true,
            IsBasic: false,
            IsAscii: false
        );
        
        fileSystem.WriteFile(diskFileName, binaryData, isText: false, loadAddress: loadAddress, execAddress: executeAddress);
        fileSystem.DiskContainer.Save();
    }

    public void ExportBinaryFile(IFileSystem fileSystem, string diskFileName, string hostFilePath)
    {
        ExportBinaryFile(fileSystem, diskFileName, hostFilePath, false);
    }

    public void ExportBinaryFile(IFileSystem fileSystem, string diskFileName, string hostFilePath, bool allowPartialRead)
    {
        if (string.IsNullOrWhiteSpace(diskFileName))
            throw new ArgumentException("Disk file name cannot be null or empty", nameof(diskFileName));
            
        if (string.IsNullOrWhiteSpace(hostFilePath))
            throw new ArgumentException("Host file path cannot be null or empty", nameof(hostFilePath));

        byte[] fileData;
        if (fileSystem is HuBasicFileSystem huBasicFileSystem)
        {
            fileData = huBasicFileSystem.ReadFile(diskFileName, allowPartialRead);
        }
        else
        {
            fileData = fileSystem.ReadFile(diskFileName);
        }
        
        File.WriteAllBytes(hostFilePath, fileData);
    }

    public void ImportBootSector(IFileSystem fileSystem, string hostFilePath, string label = "BOOT")
    {
        var bootData = File.ReadAllBytes(hostFilePath);
        
        var bootSector = new BootSector(
            IsBootable: true,
            Label: label,
            Extension: "Sys",
            Size: bootData.Length,
            LoadAddress: 0x0000,
            ExecuteAddress: 0x0000,
            ModifiedDate: DateTime.Now,
            StartSector: 0x0001
        );
        
        fileSystem.WriteBootSector(bootSector);
        fileSystem.DiskContainer.Save();
    }

    public void ExportBootSector(IFileSystem fileSystem, string hostFilePath)
    {
        var bootSector = fileSystem.GetBootSector();
        
        // For now, just export the boot sector structure as text
        var bootInfo = $"Boot Sector Information:\n" +
                      $"Bootable: {bootSector.IsBootable}\n" +
                      $"Label: {bootSector.Label}\n" +
                      $"Extension: {bootSector.Extension}\n" +
                      $"Size: {bootSector.Size}\n" +
                      $"Load Address: 0x{bootSector.LoadAddress:X4}\n" +
                      $"Execute Address: 0x{bootSector.ExecuteAddress:X4}\n" +
                      $"Modified Date: {bootSector.ModifiedDate}\n" +
                      $"Start Sector: {bootSector.StartSector}\n";
        
        File.WriteAllText(hostFilePath, bootInfo);
    }

    public void DeleteFile(IFileSystem fileSystem, string diskFileName)
    {
        fileSystem.DeleteFile(diskFileName);
        fileSystem.DiskContainer.Save();
    }

    public void SetFileProtection(IFileSystem fileSystem, string diskFileName, bool isProtected)
    {
        // This would require modification of the file system implementation
        // to support changing file attributes
        throw new NotImplementedException("SetFileProtection not yet implemented");
    }

    public IEnumerable<FileEntry> ListFiles(IFileSystem fileSystem)
    {
        return fileSystem.GetFiles();
    }

    public HuBasicFileSystemInfo GetFileSystemInfo(IFileSystem fileSystem)
    {
        return fileSystem.GetFileSystemInfo();
    }
}