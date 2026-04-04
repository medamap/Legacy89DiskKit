using System;
using System.Text;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.CharacterEncoding.Application;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace VerificationTool;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0) Console.WriteLine($"DEBUG: arg0={args[0]}");
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  VerificationTool <image_path>                      - Verify disk image");
            Console.WriteLine("  VerificationTool create-boot <src> <dest> <files>  - Create bootable disk");
            return;
        }

        if (args[0] == "debug-test")
        {
            Console.WriteLine("DEBUG-TEST OK");
            return;
        }

        if (args[0] == "create-boot")
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: VerificationTool create-boot <src> <dest> <file1,file2...>");
                return;
            }
            CreateBootDisk(args[1], args[2], args[3].Split(','));
            return;
        }
        else if (args[0] == "dump-files")
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: VerificationTool dump-files <image_path>");
                return;
            }
            DumpFiles(args[1]);
            return;
        }
        else if (args[0] == "dump-sector-range")
        {
            if (args.Length < 6)
            {
                Console.WriteLine("Usage: VerificationTool dump-sector-range <path> <cyl> <head> <start_sec> <count>");
                return;
            }
            DumpSectorRange(args[1], int.Parse(args[2]), int.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5]));
            return;
        }
        else if (args[0] == "dump-geometry")
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: VerificationTool dump-geometry <image_path>");
                return;
            }
            DumpGeometry(args[1]);
            return;
        }

        VerifyDisk(args[0]);
    }

    static void CreateBootDisk(string srcPath, string destPath, string[] filesToCopy)
    {
        Console.WriteLine($"==================================================");
        Console.WriteLine($"[CREATE BOOT] Source: {srcPath}");
        Console.WriteLine($"[CREATE BOOT] Target: {destPath}");
        Console.WriteLine($"==================================================");

        try
        {
            var fsRegistry = SetupFileSystemRegistry();
            var containerFactory = new Legacy89DiskKit.Infrastructure.DiskImage.Factory.DiskContainerFactory();
            var diskService = new DiskService(containerFactory, fsRegistry);

            // 1. Open Source
            var srcContainer = diskService.OpenDisk(srcPath, true);
            var srcFs = diskService.FileSystem;
            if (srcFs == null) throw new Exception("Source file system not detected.");

            // 2. Create Target (Blank) with exact source geometry
            Console.WriteLine($"[STEP 1] Creating blank disk: {destPath}");
            int spt = srcContainer.GetAllSectors().Count(s => s.Cylinder == 0 && s.Head == 0);
            ushort sectorSize = (ushort)srcContainer.ReadSector(0, 0, 1).Length;
            var destContainer = containerFactory.Create(destPath, srcContainer.DiskType, "BOOT_DISK", spt, sectorSize);
            
            // 3. Manual Boot Area Copy (to trigger detection)
            Console.WriteLine($"[STEP 2] Cloning Boot Area (IPL) and Track 0...");
            var bootData = srcFs.ReadBootArea();
            
            // X1 often requires the entire Track 0 to be identical for boot
            for (int s = 1; s <= spt; s++) // Start from sector 1 to include IPL
            {
                try {
                    var extra = srcContainer.ReadSector(0, 0, s);
                    if (destContainer.SectorExists(0, 0, s))
                        destContainer.WriteSector(0, 0, s, extra);
                } catch {}
            }
            destContainer.Save();

            // 4. Re-open Target with FS detection
            Console.WriteLine($"[STEP 3] Initializing Target File System...");
            var destService = new DiskService(containerFactory, fsRegistry);
            destService.OpenDisk(destPath, false);
            var destFs = destService.FileSystem;
            if (destFs == null) throw new Exception("Target file system detection failed after IPL copy.");
            Console.WriteLine($"  Detected: {destFs.GetFileSystemInfo().FileSystemName}");

            // Recalculate Start Sector if it's Hu-BASIC
            if (srcFs.GetFileSystemInfo().FileSystemName == "Hu-BASIC")
            {
                var destInfo = destFs.GetFileSystemInfo();
                int targetSector = (int)destInfo.ReservedSectors;
                Console.WriteLine($"  [Hu-BASIC] Patching Start Sector: {bootData[0x1E]} -> {targetSector}");
                bootData[0x1E] = (byte)(targetSector & 0xFF);
                bootData[0x1F] = (byte)((targetSector >> 8) & 0xFF);
            }

            // 5. Format Target
            Console.WriteLine($"[STEP 4] Formatting Directory and FAT...");
            destFs.Format();

            // 6. Migrate Files
            var targetFiles = filesToCopy;
            if (filesToCopy.Length == 1 && filesToCopy[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                targetFiles = srcFs.GetFiles().Select(f => f.FullName).ToArray();
            }

            Console.WriteLine($"[STEP 5] Transferring System Files: {(filesToCopy.Length == 1 && filesToCopy[0].Equals("all", StringComparison.OrdinalIgnoreCase) ? "ALL FILES" : string.Join(", ", targetFiles))}");
            var transferService = new FileTransferService(new X1CharacterEncoder());
            var cloneService = new DiskCloneService(transferService);
            cloneService.TransferFiles(srcFs, destFs, targetFiles);

            // Cleanup & Save
            destFs.WriteBootArea(bootData); // Write boot area again just in case format touched it (unlikely)
            destService.CloseDisk();
            
            Console.WriteLine($"\n[RESULT] Boot disk created successfully: {destPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[FATAL ERROR] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static FileSystemRegistry SetupFileSystemRegistry()
    {
        var fsRegistry = new FileSystemRegistry();
        fsRegistry.Register(new HuBasicFileSystemProvider());
        fsRegistry.Register(new N88BasicFileSystemProvider());
        fsRegistry.Register(new MsxDosFileSystemProvider());
        return fsRegistry;
    }

    static void VerifyDisk(string path)
    {
        Console.WriteLine($"==================================================");
        Console.WriteLine($"[TEST] Verifying: {path}");
        Console.WriteLine($"==================================================");

        try
        {
            // Setup Registry with all providers
            var fsRegistry = SetupFileSystemRegistry();

            // Setup Encoder Registry
            var encoderRegistry = new EncoderRegistry();
            encoderRegistry.Register("X1", new X1CharacterEncoder());
            encoderRegistry.Register("PC88", new Pc8801CharacterEncoder());
            encoderRegistry.Register("MSX", new Msx1CharacterEncoder());

            using var diskService = new DiskService(null, fsRegistry);
            var container = diskService.OpenDisk(path, true);

            var fsInfo = diskService.FileSystem.GetFileSystemInfo();
            Console.WriteLine($"[DETECTION]");
            Console.WriteLine($"  Container Type: {container.GetType().Name}");
            Console.WriteLine($"  File System: {fsInfo.FileSystemName}");
            Console.WriteLine($"  Platform: {fsInfo.PlatformId}");
            
            var encoder = encoderRegistry.GetEncoder(fsInfo.PlatformId);
            if (encoder == null)
            {
                Console.WriteLine($"  [WARNING] No encoder found for {fsInfo.PlatformId}, using ASCII.");
            }

            Console.WriteLine($"\n[BOOT AREA]");
            try
            {
                var bootArea = diskService.FileSystem.ReadBootArea();
                Console.WriteLine($"  Size: {bootArea.Length} bytes");
                string hex = BitConverter.ToString(bootArea, 0, Math.Min(32, bootArea.Length));
                Console.WriteLine($"  Hex (32B): {hex}");
                string decodedBoot = encoder?.DecodeText(bootArea.Take(32).ToArray()) ?? Encoding.ASCII.GetString(bootArea, 0, Math.Min(32, bootArea.Length));
                Console.WriteLine($"  Decoded (32B): {decodedBoot}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERROR] Failed to read boot area: {ex.Message}");
            }

            Console.WriteLine($"\n[FILES]");
            var files = diskService.FileSystem.GetFiles().ToList();
            Console.WriteLine($"  Count: {files.Count}");
            Console.WriteLine($"  {"Filename",-24} | {"Size",-8} | {"Attributes",-12} | {"Type"} | {"SC"}");
            Console.WriteLine($"  {new string('-', 24)}-+-{new string('-', 8)}-+-{new string('-', 12)}-+-{new string('-', 4)}-+-{new string('-', 4)}");

            foreach (var file in files.Take(20)) // Limit to 20 for brief summary
            {
                var attrs = file.Attributes.StandardAttributes;
                string attrStr = $"R:{(attrs.HasFlag(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.ReadOnly) ? 'Y' : 'N')} " +
                                 $"H:{(attrs.HasFlag(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.Hidden) ? 'Y' : 'N')} " +
                                 $"S:{(attrs.HasFlag(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.System) ? 'Y' : 'N')} " +
                                 $"D:{(attrs.HasFlag(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.Directory) ? 'Y' : 'N')}";
                
                // Filenames might contain Katakana, encoder should handle it
                Console.WriteLine($"  {file.FullName,-24} | {file.Size,8} | {attrStr,-12} | {(file.Attributes.IsAscii ? "ASC" : "BIN")} | SC:{file.StartCluster}");
            }

            if (files.Count > 20)
            {
                Console.WriteLine($"  ... (and {files.Count - 20} more)");
            }

            Console.WriteLine($"\n[CONTENT VERIFICATION]");
            var firstFile = files.FirstOrDefault(f => f.Size > 0);
            if (firstFile != null)
            {
                Console.WriteLine($"  Reading first non-empty file: {firstFile.FullName}");
                var content = diskService.FileSystem.ReadFile(firstFile.FullName);
                Console.WriteLine($"  Actual Read Size: {content.Length} bytes");
                
                if (firstFile.Attributes.IsAscii && encoder != null)
                {
                    string text = encoder.DecodeText(content.Take(64).ToArray());
                    Console.WriteLine($"  Text snippet (64B): {text.Replace("\n", "\\n").Replace("\r", "\\r")}");
                }
                else
                {
                    string hexContent = BitConverter.ToString(content, 0, Math.Min(16, content.Length));
                    Console.WriteLine($"  Head (16B): {hexContent}");
                }
            }

            Console.WriteLine($"\n[RESULT] Verification completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[FATAL ERROR] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void DumpSectorRange(string path, int cyl, int head, int startSec, int count)
    {
        try
        {
            var containerFactory = new Legacy89DiskKit.Infrastructure.DiskImage.Factory.DiskContainerFactory();
            using var container = containerFactory.Open(path, true);
            Console.WriteLine($"[SECTOR RANGE] {path} C:{cyl} H:{head} S:{startSec}-{startSec+count-1}");
            for (int s = startSec; s < startSec + count; s++)
            {
                try {
                    var data = container.ReadSector(cyl, head, s);
                    Console.WriteLine($"S:{s:D2} | {BitConverter.ToString(data)}");
                } catch {
                    Console.WriteLine($"S:{s:D2} | ERROR");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void DumpRawDir(string path)
    {
        try
        {
            Console.WriteLine($"Opening disk: {path}");
            var fsRegistry = SetupFileSystemRegistry();
            var containerFactory = new Legacy89DiskKit.Infrastructure.DiskImage.Factory.DiskContainerFactory();
            using var diskService = new DiskService(containerFactory, fsRegistry);
            diskService.OpenDisk(path, true);
            var fs = diskService.FileSystem;
            if (fs == null) { Console.WriteLine("FS Null - Detection Failed"); return; }
            Console.WriteLine($"FS Detected: {fs.GetType().Name}");

            var type = fs.GetType();
            var configField = type.GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var config = configField?.GetValue(fs) as Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models.HuBasicConfiguration;
            
            if (config == null) { Console.WriteLine("Config Null - Field Not Found"); return; }
            Console.WriteLine($"Config: Track={config.DirectoryTrack}, Sector={config.DirectorySector}");

            // Re-open container to read raw sectors
            var container = containerFactory.Open(path, true);
            var sectorData = container.ReadSector(config.DirectoryTrack / 2, config.DirectoryTrack % 2, config.DirectorySector);
            
            if (sectorData == null) { Console.WriteLine("Sector Data Null"); return; }
            Console.WriteLine($"[RAW DIR] {path} Track:{config.DirectoryTrack} Sector:{config.DirectorySector} DataLen:{sectorData.Length}");
            for (int i = 0; i < 10; i++)
            {
                if (i * 32 + 32 > sectorData.Length) break;
                var entry = sectorData.Skip(i * 32).Take(32).ToArray();
                Console.WriteLine($"Entry {i:D2}: {BitConverter.ToString(entry)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void DumpFiles(string path)
    {
        var fsRegistry = SetupFileSystemRegistry();
        var containerFactory = new Legacy89DiskKit.Infrastructure.DiskImage.Factory.DiskContainerFactory();
        using var diskService = new DiskService(containerFactory, fsRegistry);
        diskService.OpenDisk(path, true);
        var fs = diskService.FileSystem;
        if (fs == null) return;

        var files = fs.GetFiles().ToList();
        Console.WriteLine($"[FILES] {path} Count: {files.Count}");
        Console.WriteLine($"{"Name",-20} | {"Attr",-6} | {"Size",-8} | {"Load",-6} | {"Exec",-6} | {"SC",-4}");
        Console.WriteLine(new string('-', 60));
        foreach (var file in files)
        {
            var att = file.Attributes;
            string attrStr = $"0x{att.RawAttributes:X2}";
            Console.WriteLine($"{file.FullName,-20} | {attrStr,-6} | {file.Size,8} | 0x{file.LoadAddress:X4} | 0x{file.ExecutionAddress:X4} | {file.StartCluster,4}");
        }
    }

    static void DumpGeometry(string path)
    {
        var containerFactory = new Legacy89DiskKit.Infrastructure.DiskImage.Factory.DiskContainerFactory();
        using var container = containerFactory.Open(path, true);
        var sectors = container.GetAllSectors().ToList();
        var first = sectors.FirstOrDefault();
        if (first != null)
        {
            var spt = sectors.Count(s => s.Cylinder == 0 && s.Head == 0);
            Console.WriteLine($"[GEOMETRY] {path}");
            Console.WriteLine($"  Sector Size: {first.Size}");
            Console.WriteLine($"  Sectors/Track: {spt}");
            Console.WriteLine($"  Total Sectors: {sectors.Count}");
            Console.WriteLine($"  Total Tracks: {sectors.Count / (spt > 0 ? spt : 1)}");
        }
    }
}
