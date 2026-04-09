using System.IO;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Infrastructure.DiskImage.Factory;
using Legacy89DiskKit.CharacterEncoding.Application;
using Legacy89DiskKit.Native.Application;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.Archive.Application;
public class ArchiveService
{
    private readonly DiskContainerFactory _containerFactory;
    private readonly FileSystemRegistry _fsRegistry;
    private readonly EncoderRegistry _encoderRegistry;
    private readonly DiskCloneService _diskCloneService;
    public IEncoderRegistry EncoderRegistry => _encoderRegistry;

    public ArchiveService()
    {
        _containerFactory = new DiskContainerFactory();
        _fsRegistry = new FileSystemRegistry();
        _fsRegistry.Register(new HuBasicFileSystemProvider());
        _fsRegistry.Register(new N88BasicFileSystemProvider());
        _fsRegistry.Register(new MsxDosFileSystemProvider());
        _fsRegistry.Register(new Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider.XDosFileSystemProvider());
        _encoderRegistry = new EncoderRegistry();
        _encoderRegistry.Register("X1", new X1CharacterEncoder());
        _encoderRegistry.Register("SJIS", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("ShiftJIS", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("Shift-JIS", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("Shift_JIS", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("sjis", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("shiftjis", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("shift-jis", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("shift_jis", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("MSX", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("PC88", new ShiftJisCharacterEncoder());
        var transferService = new FileTransferService(new Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder.X1CharacterEncoder()); // Default encoder for transfer
        var normalizationService = new FileNameNormalizationService(_encoderRegistry);
        _diskCloneService = new DiskCloneService(transferService, normalizationService);
    }

    public void CloneBootable(string srcPath, string destPath, string[] filesToCopy)
    {
        using var srcDisk = new DiskService(new ManagedNativeBridgeBackend(), _fsRegistry);
        var srcContainer = srcDisk.OpenDisk(srcPath, true);
        var srcFs = srcDisk.FileSystem;
        if (srcFs == null)
            throw new Exception("Source file system not detected.");

        // Create Target with same geometry and default name
        int spt = srcContainer.GetAllSectors().Count(s => s.Cylinder == 0 && s.Head == 0);
        ushort sectorSize = (ushort)srcContainer.ReadSector(0, 0, 1).Length;
        using var destContainer = _containerFactory.Create(destPath, srcContainer.DiskType, "BOOT_DISK", spt, sectorSize);

        using var destDisk = new DiskService(new ManagedNativeBridgeBackend(), _fsRegistry);
        destDisk.OpenDisk(destPath, false);
        var destFs = destDisk.FileSystem;
        if (destFs == null)
            throw new Exception("Target FS detection failed.");

        var srcAdapter = CreateTransferAdapter(srcFs);
        var destAdapter = CreateTransferAdapter(destFs);

        var srcFsInfo = srcFs.GetFileSystemInfo();
        if (srcFsInfo.FileSystemName == "X-DOS" && srcAdapter is Legacy89DiskKit.Infrastructure.FileSystem.XDos.XDosTransferAdapter xdosSrcAdapter && destAdapter is Legacy89DiskKit.Infrastructure.FileSystem.XDos.XDosTransferAdapter xdosDestAdapter)
        {
            _diskCloneService.CloneXDosBootable(srcFs, xdosSrcAdapter, destFs, xdosDestAdapter);
        }
        else if (srcFsInfo.FileSystemName == "Hu-BASIC" && srcAdapter is Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.HuBasicTransferAdapter huBasicSrcAdapter && destAdapter is Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.HuBasicTransferAdapter huBasicDestAdapter)
        {
            _diskCloneService.CloneHuBasicBootable(srcFs, huBasicSrcAdapter, destFs, huBasicDestAdapter);
        }
        else
        {
            // Fallback for other file systems or if adapters are not available
            // Manual Track 0 copy
            for (int s = 1; s <= spt; s++)
            {
                try
                {
                    var data = srcContainer.ReadSector(0, 0, s);
                    if (destContainer.SectorExists(0, 0, s))
                        destContainer.WriteSector(0, 0, s, data);
                }
                catch
                {
                    // Ignore errors during sector copy, aim for best effort
                }
            }
            destContainer.Save();

            // Format after raw copy
            destFs.Format();

            var targetFiles = filesToCopy;
            if (filesToCopy.Length == 1 && filesToCopy[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                targetFiles = srcFs.GetFiles().Select(f => f.FullName).ToArray();
            }
            _diskCloneService.TransferFiles(srcFs, destFs, targetFiles);
        }

        destFs.WriteBootArea(srcFs.ReadBootArea()); // Ensure boot area is written after formatting/file transfer
        destDisk.CloseDisk();
    }

    private IFileSystemTransferAdapter? CreateTransferAdapter(IFileSystem fs)
    {
        return fs switch
        {
            XDosFileSystem xdos => new XDosTransferAdapter(xdos),
            HuBasicFileSystem huBasic => new HuBasicTransferAdapter(huBasic),
            _ => null
        };
    }

    public void InjectFile(string diskPath, string hostFilePath, string? targetFileName = null, string? encodingOverride = null, TextTransferOptions? textOptions = null)
    {
        using var diskService = new DiskService(new ManagedNativeBridgeBackend(), _fsRegistry);
        diskService.OpenDisk(diskPath, false); // Open for writing
        var fs = diskService.FileSystem;
        if (fs == null)
            throw new Exception("Unsupported file system on target disk.");
        var fsInfo = fs.GetFileSystemInfo();
        string platformId = fsInfo.PlatformId;
        string encodingId = encodingOverride ?? fsInfo.DefaultEncodingId;
        var existingNames = new HashSet<string>(fs.GetFiles().Select(f => f.FullName.ToUpperInvariant()));
        string sourceName = targetFileName ?? Path.GetFileName(hostFilePath);
        var normalizationService = new FileNameNormalizationService(_encoderRegistry);
        string normalizedName = normalizationService.Normalize(sourceName, encodingId, fsInfo.MaxBaseNameLength, fsInfo.MaxExtensionLength, existingNames);
        byte[] data = File.ReadAllBytes(hostFilePath);
        bool isAscii = IsLikelyAscii(data);
        Console.WriteLine($"Injecting '{sourceName}' as '{normalizedName}' (Encoding: {encodingId}, {(isAscii ? "ASCII" : "BIN")})...");
        if (isAscii)
        {
            var encoder = _encoderRegistry.GetEncoder(encodingId) ?? throw new InvalidOperationException($"Unsupported encoding: {encodingId}");
            var transferService = new FileTransferService(encoder);
            transferService.ImportFile(fs, hostFilePath, normalizedName, true, textOptions);
        }
        else
        {
            var attributes = fs.CreateDefaultAttributes(false);
            fs.WriteFile(normalizedName, data, attributes);
        }

        diskService.Session?.Save();
    }

    private bool IsLikelyAscii(byte[] data)
    {
        if (data.Length == 0)
            return true;
        // Check first 1024 bytes for nulls or high bits (crude check)
        int count = Math.Min(data.Length, 1024);
        int nonPrintable = 0;
        for (int i = 0; i < count; i++)
        {
            if (data[i] == 0)
                return false;
            if (data[i] < 32 && data[i] != 9 && data[i] != 10 && data[i] != 13 && data[i] != 0x1A)
                nonPrintable++;
        }

        return (double)nonPrintable / count < 0.1;
    }
}
