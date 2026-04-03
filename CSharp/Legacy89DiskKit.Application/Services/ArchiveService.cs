using System.IO;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Infrastructure.DiskImage.Factory;

using Legacy89DiskKit.Application.CharacterEncoding;
using Legacy89DiskKit.Application.Native;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;

namespace Legacy89DiskKit.Application.Services;

public class ArchiveService
{
    private readonly DiskContainerFactory _containerFactory;
    private readonly FileSystemRegistry _fsRegistry;
    private readonly EncoderRegistry _encoderRegistry;

    public IEncoderRegistry EncoderRegistry => _encoderRegistry;

    public ArchiveService()
    {
        _containerFactory = new DiskContainerFactory();
        _fsRegistry = new FileSystemRegistry();
        _fsRegistry.Register(new HuBasicFileSystemProvider());
        _fsRegistry.Register(new N88BasicFileSystemProvider());
        _fsRegistry.Register(new MsxDosFileSystemProvider());

        _encoderRegistry = new EncoderRegistry();
        _encoderRegistry.Register("X1", new X1CharacterEncoder());
        _encoderRegistry.Register("SJIS", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("Shift-JIS", new ShiftJisCharacterEncoder());
    }

    public void CloneBootable(string srcPath, string destPath, string[] filesToCopy)
    {
        using var srcDisk = new DiskService(new ManagedNativeBridgeBackend(), _fsRegistry);
        var srcContainer = srcDisk.OpenDisk(srcPath, true);
        var srcFs = srcDisk.FileSystem;
        if (srcFs == null) throw new Exception("Source file system not detected.");

        // Create Target with same geometry
        int spt = srcContainer.GetAllSectors().Count(s => s.Cylinder == 0 && s.Head == 0);
        ushort sectorSize = (ushort)srcContainer.ReadSector(0, 0, 1).Length;
        
        using var destContainer = _containerFactory.Create(destPath, srcContainer.DiskType, "BOOT_DISK", spt, sectorSize);
        
        // Clone Track 0 (IPL)
        var bootData = srcFs.ReadBootArea();
        for (int s = 1; s <= spt; s++)
        {
            try {
                var data = srcContainer.ReadSector(0, 0, s);
                if (destContainer.SectorExists(0, 0, s))
                    destContainer.WriteSector(0, 0, s, data);
            } catch {}
        }
        destContainer.Save();

        // Open target to get FS
        using var destDisk = new DiskService(new ManagedNativeBridgeBackend(), _fsRegistry);
        destDisk.OpenDisk(destPath, false);
        var destFs = destDisk.FileSystem;
        if (destFs == null) throw new Exception("Target FS detection failed.");

        // Patch Hu-BASIC Start Sector if needed
        if (srcFs.GetFileSystemInfo().FileSystemName == "Hu-BASIC")
        {
            var destInfo = destFs.GetFileSystemInfo();
            int targetSector = (int)destInfo.ReservedSectors;
            bootData[0x1E] = (byte)(targetSector & 0xFF);
            bootData[0x1F] = (byte)((targetSector >> 8) & 0xFF);
        }

        destFs.Format();

        // Transfer files
        var targetFiles = filesToCopy;
        if (filesToCopy.Length == 1 && filesToCopy[0].Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            targetFiles = srcFs.GetFiles().Select(f => f.FullName).ToArray();
        }

        var transferService = new FileTransferService(new X1CharacterEncoder());
        var normalizationService = new FileNameNormalizationService(_encoderRegistry);
        var cloneService = new DiskCloneService(transferService, normalizationService);
        cloneService.TransferFiles(srcFs, destFs, targetFiles);

        destFs.WriteBootArea(bootData);
        destDisk.CloseDisk();
    }
    public void InjectFile(string diskPath, string hostFilePath, string? targetFileName = null, string? encodingOverride = null)
    {
        using var diskService = new DiskService(new ManagedNativeBridgeBackend(), _fsRegistry);
        diskService.OpenDisk(diskPath, false); // Open for writing
        
        var fs = diskService.FileSystem;
        if (fs == null) throw new Exception("Unsupported file system on target disk.");

        var fsInfo = fs.GetFileSystemInfo();
        string platformId = fsInfo.PlatformId;
        string encodingId = encodingOverride ?? fsInfo.DefaultEncodingId;

        var existingNames = new HashSet<string>(fs.GetFiles().Select(f => f.FullName.ToUpperInvariant()));
        
        string sourceName = targetFileName ?? Path.GetFileName(hostFilePath);
        
        var normalizationService = new FileNameNormalizationService(_encoderRegistry);
        
        string normalizedName = normalizationService.Normalize(sourceName, encodingId, fsInfo.MaxBaseNameLength, fsInfo.MaxExtensionLength, existingNames);
        
        byte[] data = File.ReadAllBytes(hostFilePath);
        
        // Simple heuristic for ASCII vs Binary
        bool isAscii = IsLikelyAscii(data);
        var attributes = fs.CreateDefaultAttributes(isAscii);
        
        Console.WriteLine($"Injecting '{sourceName}' as '{normalizedName}' (Encoding: {encodingId}, { (isAscii ? "ASCII" : "BIN") })...");
        fs.WriteFile(normalizedName, data, attributes);
        diskService.Session?.Save();
    }

    private bool IsLikelyAscii(byte[] data)
    {
        if (data.Length == 0) return true;
        // Check first 1024 bytes for nulls or high bits (crude check)
        int count = Math.Min(data.Length, 1024);
        int nonPrintable = 0;
        for (int i = 0; i < count; i++)
        {
            if (data[i] == 0) return false;
            if (data[i] < 32 && data[i] != 9 && data[i] != 10 && data[i] != 13 && data[i] != 0x1A) nonPrintable++;
        }
        return (double)nonPrintable / count < 0.1;
    }
}
