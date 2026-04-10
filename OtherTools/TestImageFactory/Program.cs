using System.Text.Json;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Registry;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;
using Legacy89DiskKit.FileSystem.Infrastructure.XDos;

static IEncoderRegistry CreateEncoderRegistry()
{
    var registry = new Legacy89DiskKit.CharacterEncoding.Application.EncoderRegistry();
    registry.Register("X1", new Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder.X1CharacterEncoder());
    registry.Register("SJIS", new Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder.ShiftJisCharacterEncoder());
    registry.Register("MSX", new Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder.ShiftJisCharacterEncoder());
    registry.Register("PC88", new Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder.ShiftJisCharacterEncoder());
    return registry;
}

static Legacy89DiskKit.CharacterEncoding.Domain.Interface.ICharacterEncoder CreateEncoder(Legacy89DiskKit.FileSystem.Domain.Model.DiskFileSystemInfo fsInfo)
{
    return new Legacy89DiskKit.CharacterEncoding.Application.CharacterEncodingResolver(CreateEncoderRegistry()).ResolveEncoder(fsInfo, null);
}

static Legacy89DiskKit.FileSystem.Domain.Interface.Registry.IFileSystemRegistry CreateFileSystemRegistry()
{
    var registry = new Legacy89DiskKit.FileSystem.Application.FileSystemRegistry();
    registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider.XDosFileSystemProvider());
    registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider.HuBasicFileSystemProvider());
    registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Cpm.Provider.CpmFileSystemProvider());
    registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Provider.N88BasicFileSystemProvider());
    registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Msx.Provider.MsxDosFileSystemProvider());
    return registry;
}

static Legacy89DiskKit.FileSystem.Application.FileTransferService CreateFileTransferService(Legacy89DiskKit.FileSystem.Domain.Model.DiskFileSystemInfo fsInfo)
{
    return new Legacy89DiskKit.FileSystem.Application.FileTransferService(CreateEncoder(fsInfo));
}

var manifestPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(AppContext.BaseDirectory, "manifest.local.json");

if (!File.Exists(manifestPath))
{
    throw new FileNotFoundException($"Manifest not found: {manifestPath}");
}

var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(manifestPath), JsonOptions())
    ?? throw new InvalidOperationException("Failed to parse manifest.");

if (manifest.CleanupOutputRoot && Directory.Exists(manifest.OutputRoot))
{
    foreach (var file in Directory.EnumerateFiles(manifest.OutputRoot))
    {
        File.Delete(file);
    }
}

Directory.CreateDirectory(manifest.OutputRoot);

var summaries = new List<GenerationResult>();

foreach (var item in manifest.Items)
{
    var targetPath = Path.Combine(manifest.OutputRoot, item.Name);
    ExecuteItem(manifest.OutputRoot, targetPath, item);
    summaries.Add(new GenerationResult(item.Name, item.Operation, item.BootRuntimeGuarantee));
    Console.WriteLine($"[OK] {item.Name} ({item.Operation})");
}

var summaryPath = Path.Combine(manifest.OutputRoot, "_generation_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(summaries, JsonOptions()));

static void ExecuteItem(string outputRoot, string targetPath, ManifestItem item)
{
    switch (item.Operation)
    {
        case "create-blank":
            CreateBlank(targetPath, ParseDiskType(item.DiskType));
            return;
        case "format":
            FormatFileSystem(targetPath, ParseDiskType(item.DiskType), item.FileSystem, item.PhysicalProfile);
            return;
        case "copy-image":
            File.Copy(Required(item.SourceImage), targetPath, true);
            return;
        case "boot-copy":
            BootCopy(Required(item.SourceImage), targetPath, item.SourceFileSystem, item.DestinationFileSystem);
            return;
        case "logical-copy-x1":
            X1LogicalCopy(Required(item.SourceImage), targetPath, item.SourceFileSystem, item.DestinationFileSystem);
            return;
        case "boot-write-from-file":
            BootWriteFromFile(Required(item.SourceImage), Required(item.SourceFile), targetPath, item.SourceFileSystem, item.DestinationFileSystem);
            return;
        case "xdos-clone-all":
            XDosCloneAll(Required(item.SourceImage), targetPath, ParseDiskType(item.DiskType));
            return;
        case "cpm-minimal-format":
            CreateFormattedBlank(targetPath, ParseDiskType(item.DiskType), item.PhysicalProfile);
            CpmMinimalFormat(targetPath, ParseDiskType(item.DiskType), item.PhysicalProfile);
            return;
        case "cpm-seed-from-source":
            CpmSeedFromSource(Required(item.SourceImage), targetPath);
            return;
        default:
            throw new InvalidOperationException($"Unsupported operation: {item.Operation}");
    }
}

static void CreateBlank(string targetPath, DiskType diskType)
{
    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
    if (File.Exists(targetPath))
    {
        File.Delete(targetPath);
    }
    WriteUnformattedBlankD88(targetPath, diskType, Path.GetFileNameWithoutExtension(targetPath));
}

static void CreateFormattedBlank(string targetPath, DiskType diskType, string? physicalProfile)
{
    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
    if (File.Exists(targetPath))
    {
        File.Delete(targetPath);
    }

    var profile = ResolvePhysicalProfile(diskType, physicalProfile);
    if (profile is null)
    {
        using var diskService = new Legacy89DiskKit.DiskImage.Application.DiskService(fsRegistry: CreateFileSystemRegistry());
        using var container = diskService.CreateDisk(targetPath, diskType, Path.GetFileNameWithoutExtension(targetPath));
        container.Save();
        return;
    }

    using var created = D88DiskContainer.CreateNew(
        targetPath,
        diskType,
        Path.GetFileNameWithoutExtension(targetPath),
        (c, h) => c < profile.Cylinders && h < 2
            ? (profile.SectorsPerTrack, profile.SectorSize, profile.Density)
            : null);
    created.Save();
}

static void FormatFileSystem(string targetPath, DiskType diskType, string? fileSystem, string? physicalProfile)
{
    CreateFormattedBlank(targetPath, diskType, physicalProfile);

    using var diskService = new Legacy89DiskKit.DiskImage.Application.DiskService(fsRegistry: CreateFileSystemRegistry());
    using var container = diskService.OpenDisk(targetPath, false);

    switch ((fileSystem ?? string.Empty).ToLowerInvariant())
    {
        case "hu-basic":
            using (var fs = new HuBasicFileSystem(container))
            {
                fs.Format();
            }
            break;
        case "xdos":
            using (var fs = new XDosFileSystem(container))
            {
                fs.Format();
            }
            break;
        default:
            throw new InvalidOperationException($"Unsupported format filesystem: {fileSystem}");
    }

    container.Save();
}

static void BootCopy(string sourceImage, string destImage, string? sourceFileSystem, string? destinationFileSystem)
{
    using var sourceContainer = OpenContainer(sourceImage, true);
    using var destContainer = OpenContainer(destImage, false);
    using var sourceFs = CreateKnownFileSystem(sourceFileSystem, sourceContainer);
    using var destFs = CreateKnownFileSystem(destinationFileSystem, destContainer);

    var cloneService = new Legacy89DiskKit.FileSystem.Application.DiskCloneService(CreateFileTransferService(destFs.GetFileSystemInfo()), new FileNameNormalizationService(CreateEncoderRegistry()));
    cloneService.TransferBootArea(sourceFs, destFs);
    destContainer.Save();
}

static void X1LogicalCopy(string sourceImage, string destImage, string? sourceFileSystem, string? destinationFileSystem)
{
    using var sourceContainer = OpenContainer(sourceImage, true);
    using var destContainer = OpenContainer(destImage, false);
    using var sourceFs = CreateKnownFileSystem(sourceFileSystem, sourceContainer);
    using var destFs = CreateKnownFileSystem(destinationFileSystem, destContainer);

    var exportService = new Legacy89DiskKit.FileSystem.Application.BootEntryExportService();
    var importService = new Legacy89DiskKit.FileSystem.Application.BootEntryImportService();
    var cloneService = new Legacy89DiskKit.FileSystem.Application.DiskCloneService(CreateFileTransferService(sourceFs.GetFileSystemInfo()), new FileNameNormalizationService(CreateEncoderRegistry()));
    var sourceFiles = sourceFs.GetFiles().ToList();
    var srcAdapter = sourceFs is HuBasicFileSystem hSrc ? new HuBasicTransferAdapter(hSrc) : null;
    var dstAdapter = destFs is HuBasicFileSystem hDst ? new HuBasicTransferAdapter(hDst) : null;

    var entries = exportService.ExportEntries(sourceContainer, sourceFs);
    Console.WriteLine($"[X1LogicalCopy] exported {entries.Count} entries from {Path.GetFileName(sourceImage)}");

    cloneService.TransferFiles(sourceFs, destFs, sourceFiles.Select(file => file.FullName), srcAdapter, dstAdapter);

    foreach (var entry in entries)
    {
        ushort? startRecord = null;
        if (entry.Mode == BootInfoMode.FileBacked)
        {
            startRecord = ResolveCopiedHuBasicStartRecord(sourceContainer, sourceFs, destFs);
        }

        var metadata = new BootEntryImportMetadata(
            entry.MachineFamily,
            entry.Mode.ToString(),
            entry.DisplayName,
            entry.SuggestedBinaryFileName,
            entry.Payload.Length,
            entry.LoadAddress,
            entry.ExecutionAddress,
            startRecord);
        importService.ImportEntry(destContainer, destFs, metadata, entry.Payload);
    }

    Console.WriteLine($"[X1LogicalCopy] destination files after apply: {destFs.GetFiles().Count()}");
    destContainer.Save();
}

static ushort ResolveCopiedHuBasicStartRecord(IDiskContainer sourceContainer, IFileSystem sourceFs, IFileSystem destFs)
{
    var metadataService = new HuBasicMetadataService();
    var bootInfo = metadataService.GetBootRecordInfo(sourceFs)
        ?? throw new InvalidOperationException("Hu-BASIC boot record metadata was not found.");
    var sourceConfig = HuBasicConfiguration.GetDefault(sourceContainer.DiskType);
    var sectorsPerCluster = sourceConfig.ClusterSize / sourceConfig.SectorSize;

    var sourceBootFile = sourceFs.GetFiles().FirstOrDefault(file =>
        file.Size == bootInfo.Size &&
        (file.StartCluster * sectorsPerCluster) == bootInfo.StartRecord);

    if (sourceBootFile == null)
    {
        throw new InvalidOperationException("Failed to locate the Hu-BASIC boot payload among source filesystem entries.");
    }

    var destInfo = destFs.GetFileSystemInfo();
    var normalizationService = new FileNameNormalizationService(CreateEncoderRegistry());
    
    // If we are Hu-BASIC to Hu-BASIC, we expect the original name to be preserved.
    bool sIsHu = sourceFs.GetFileSystemInfo().FileSystemName == "Hu-BASIC";
    bool dIsHu = destFs.GetFileSystemInfo().FileSystemName == "Hu-BASIC";
    
    var expectedDestName = (sIsHu && dIsHu)
        ? sourceBootFile.FullName
        : normalizationService.Normalize(
            sourceBootFile.FullName,
            destInfo.DefaultEncodingId,
            destInfo.MaxBaseNameLength,
            destInfo.MaxExtensionLength,
            new HashSet<string>());

    var destBootFile = destFs.GetFiles().FirstOrDefault(file =>
        file.FullName.Equals(expectedDestName, StringComparison.OrdinalIgnoreCase));

    if (destBootFile == null)
    {
        throw new InvalidOperationException($"Failed to locate copied Hu-BASIC boot file '{expectedDestName}' in destination filesystem.");
    }

    return (ushort)(destBootFile.StartCluster * sectorsPerCluster);
}

static void BootWriteFromFile(string sourceImage, string sourceFile, string destImage, string? sourceFileSystem, string? destinationFileSystem)
{
    using var sourceContainer = OpenContainer(sourceImage, true);
    using var destContainer = OpenContainer(destImage, false);
    using var sourceFs = CreateKnownFileSystem(sourceFileSystem, sourceContainer);
    using var destFs = CreateKnownFileSystem(destinationFileSystem, destContainer);

    var bootBytes = sourceFs.ReadFile(sourceFile);
    destFs.WriteBootArea(bootBytes);
    destContainer.Save();
}


static void XDosCloneAll(string sourceImage, string destImage, DiskType diskType)
{
    FormatFileSystem(destImage, diskType, "xdos", null);

    using var sourceContainer = OpenContainer(sourceImage, true);
    using var destContainer = OpenContainer(destImage, false);
    using var sourceFs = new XDosFileSystem(sourceContainer);
    using var destFs = new XDosFileSystem(destContainer);

    var cloneService = new Legacy89DiskKit.FileSystem.Application.DiskCloneService(CreateFileTransferService(sourceFs.GetFileSystemInfo()), new FileNameNormalizationService(CreateEncoderRegistry()));
    cloneService.CloneXDosBootable(sourceFs, new XDosTransferAdapter(sourceFs), destFs, new XDosTransferAdapter(destFs));
    destContainer.Save();
}

static void CpmMinimalFormat(string targetPath, DiskType diskType, string? physicalProfile)
{
    using var container = D88DiskContainer.CreateNew(targetPath, diskType, Path.GetFileNameWithoutExtension(targetPath));
    var profile = ResolvePhysicalProfile(diskType, physicalProfile) ?? GetDefaultPhysicalProfile(diskType);
    container.RebuildGeometry((c, h) => (profile.Cylinders, GetHeads(), profile.SectorsPerTrack, profile.SectorSize, profile.Density, c, h) switch
    {
        (_, _, _, _, _, var cyl, var head) when cyl < profile.Cylinders && head < 2 => (profile.SectorsPerTrack, profile.SectorSize, profile.Density),
        _ => null
    });

    var sectorSize = profile.SectorSize;
    var blankSector = new byte[sectorSize];
    for (var cyl = 0; cyl < GetCylinders(diskType); cyl++)
    {
        for (var head = 0; head < 2; head++)
        {
            for (var sector = 1; sector <= GetSectorsPerTrack(diskType); sector++)
            {
                container.WriteSector(cyl, head, sector, blankSector);
            }
        }
    }

    var deletedSector = Enumerable.Repeat((byte)0xE5, sectorSize).ToArray();
    for (var sector = 1; sector <= 8; sector++)
    {
        container.WriteSector(4, 0, sector, deletedSector);
    }

    container.Save();
}

static void CpmSeedFromSource(string sourceImage, string destImage)
{
    using var sourceContainer = OpenContainer(sourceImage, true);
    using var destContainer = OpenContainer(destImage, false);

    foreach (var sectorInfo in sourceContainer.GetAllSectors())
    {
        if (!destContainer.SectorExists(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector))
        {
            continue;
        }

        var data = sourceContainer.ReadSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector);
        destContainer.WriteSector(sectorInfo.Cylinder, sectorInfo.Head, sectorInfo.Sector, data);
    }

    destContainer.Save();
}

static IDiskContainer OpenContainer(string path, bool readOnly)
{
    return new D88DiskContainer(path, readOnly);
}

static IFileSystem CreateKnownFileSystem(string? kind, IDiskContainer container)
{
    return (kind ?? string.Empty).ToLowerInvariant() switch
    {
        "hu-basic" => new HuBasicFileSystem(container),
        "xdos" => new XDosFileSystem(container),
        _ => throw new InvalidOperationException($"Unsupported filesystem for known operation: {kind}")
    };
}

static DiskType ParseDiskType(string? value) => (value ?? string.Empty).ToLowerInvariant() switch
{
    "2d" => DiskType.TwoD,
    "2dd" => DiskType.TwoDD,
    "2hd" => DiskType.TwoHD,
    _ => throw new InvalidOperationException($"Unsupported disk type: {value}")
};

static int GetCylinders(DiskType diskType) => diskType switch
{
    DiskType.TwoD => 40,
    DiskType.TwoDD => 80,
    DiskType.TwoHD => 77,
    _ => throw new InvalidOperationException($"Unsupported disk type: {diskType}")
};

static int GetHeads() => 2;

static int GetSectorsPerTrack(DiskType diskType) => diskType switch
{
    DiskType.TwoHD => 16,
    _ => 16
};

static ushort GetSectorSize(DiskType diskType) => diskType switch
{
    DiskType.TwoHD => 256,
    _ => 256
};

static byte GetDensity(DiskType diskType) => diskType switch
{
    DiskType.TwoHD => 0x01,
    _ => 0x00
};

static string Required(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException("Manifest value is required.");
    }

    return value;
}

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNameCaseInsensitive = true
};

static PhysicalProfile GetDefaultPhysicalProfile(DiskType diskType) => diskType switch
{
    DiskType.TwoD => new(40, 16, 256, 0x00),
    DiskType.TwoDD => new(80, 16, 256, 0x00),
    DiskType.TwoHD => new(77, 16, 256, 0x01),
    _ => throw new InvalidOperationException($"Unsupported disk type: {diskType}")
};

static PhysicalProfile? ResolvePhysicalProfile(DiskType diskType, string? physicalProfile)
{
    if (string.IsNullOrWhiteSpace(physicalProfile))
    {
        return null;
    }

    return physicalProfile.Trim().ToLowerInvariant() switch
    {
        "x1-hubasic-2hd" => new PhysicalProfile(77, 26, 256, 0x00),
        "x1-cpm-2hd" => new PhysicalProfile(77, 26, 256, 0x01),
        _ => throw new InvalidOperationException($"Unsupported physical profile: {physicalProfile}")
    };
}

static void WriteUnformattedBlankD88(string targetPath, DiskType diskType, string diskName)
{
    var buffer = new byte[0x2b0];
    var nameBytes = System.Text.Encoding.ASCII.GetBytes(diskName);
    Array.Copy(nameBytes, 0, buffer, 0, Math.Min(nameBytes.Length, 17));
    buffer[0x1a] = 0x00;
    buffer[0x1b] = (byte)diskType;
    BitConverter.GetBytes((uint)buffer.Length).CopyTo(buffer, 0x1c);
    File.WriteAllBytes(targetPath, buffer);
}

internal sealed record Manifest(
    string OutputRoot,
    bool CleanupOutputRoot,
    List<ManifestItem> Items);

internal sealed record ManifestItem(
    string Name,
    string Operation,
    string? DiskType,
    string? PhysicalProfile,
    string? FileSystem,
    string? SourceImage,
    string? SourceFile,
    string? SourceFileSystem,
    string? DestinationFileSystem,
    string? BootRuntimeGuarantee);

internal sealed record GenerationResult(
    string Name,
    string Operation,
    string? BootRuntimeGuarantee);

internal sealed record PhysicalProfile(
    int Cylinders,
    int SectorsPerTrack,
    ushort SectorSize,
    byte Density);
