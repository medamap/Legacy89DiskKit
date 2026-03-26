using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using FileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosFileSystemTest
{
    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private string XDosSysPath  => GetRepoPath("images/disk_org/x1/XDOS_SYS.D88");
    private string XDosUtilPath => GetRepoPath("images/disk_org/x1/XDOSUTIL.D88");
    private string HuBasicPath  => GetRepoPath("images/disk_org/x1/X1turboIIIDemo.d88");

    [Fact]
    public void Provider_CanHandle_XDosDisk_ReturnsTrue_DEBUG()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var container = diskService.Session as IDiskContainer;
        Assert.NotNull(container);

        var provider = new XDosFileSystemProvider();
        Assert.True(provider.CanHandle(container));
    }

    [Fact]
    public void Provider_CanHandle_NonXDosDisk_ReturnsFalse()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(HuBasicPath, true);
        var container = diskService.Session as IDiskContainer;
        Assert.NotNull(container);

        var provider = new XDosFileSystemProvider();
        Assert.False(provider.CanHandle(container));
    }

    [Fact]
    public void GetFileSystemInfo_ReturnsValidInfo()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var info = fs.GetFileSystemInfo();
        Assert.Equal("X-DOS", info.FileSystemName);
        Assert.Equal("X1", info.PlatformId);
    }

    [Fact]
    public void GetFiles_XDosSys_ReturnsAtLeast5Entries()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var files = fs.GetFiles().ToList();
        Assert.True(files.Count >= 5);
        Assert.All(files, f => Assert.False(string.IsNullOrEmpty(f.FileName)));
        Assert.Contains(files, f => f.LoadAddress > 0);
    }

    [Fact]
    public void GetFiles_XDosUtil_ReturnsAtLeast10Entries()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosUtilPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var files = fs.GetFiles().ToList();
        Assert.True(files.Count >= 10);
    }

    [Fact]
    public void FileExists_ReturnsExpectedResult()
    {
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        diskService.OpenDisk(XDosSysPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var files = fs.GetFiles().ToList();
        var firstFile = files.First().FileName;

        Assert.True(fs.FileExists(firstFile));
        Assert.False(fs.FileExists("DOESNOTEXIST"));
    }

    [Fact]
    public void Registry_AutoDetects_XDos()
    {
        var registry = Legacy89DiskKitApplication.CreateFileSystemRegistry();
        using var diskService = new DiskService(fsRegistry: registry);
        diskService.OpenDisk(XDosSysPath, true);

        Assert.NotNull(diskService.FileSystem);
        Assert.IsType<XDosFileSystem>(diskService.FileSystem);
    }

    [Fact]
    public void ExplicitResolver_CanCreate_XDos()
    {
        var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
        using var diskService = Legacy89DiskKitApplication.CreateDiskService();
        var container = diskService.OpenDisk(XDosSysPath, true);

        var fs = resolver.Create("xdos", container);
        Assert.NotNull(fs);
        Assert.IsType<XDosFileSystem>(fs);
    }

    [Fact]
    public void WriteFile_NewDisk2DD_WriteAndRead_RoundTrip()
    {
        var outputPath = GetRepoPath("images/test/XDOS_NEW2DD.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        if (File.Exists(outputPath)) File.Delete(outputPath);

        D88DiskContainer.CreateNew(outputPath, DiskType.TwoDD, "XD_NEW2DD",
            XDosFileSystem.XDosTrackGeometry);

        using var destService = Legacy89DiskKitApplication.CreateDiskService();
        var destContainer = destService.OpenDisk(outputPath, false);
        var destFs = new XDosFileSystem(destContainer);
        destFs.Format();

        byte[] testData = Enumerable.Range(0, 1024).Select(i => (byte)(i & 0xFF)).ToArray();
        destFs.WriteFile("TEST.BIN", testData, destFs.CreateDefaultAttributes(false), 0x8000, 0x8000);
        destContainer.Save();

        using var verifyService = Legacy89DiskKitApplication.CreateDiskService();
        verifyService.OpenDisk(outputPath, true);
        var verifyFs = new XDosFileSystem(verifyService.Session as IDiskContainer
            ?? throw new InvalidOperationException());

        Assert.True(verifyFs.FileExists("TEST.BIN"));
        var readBack = verifyFs.ReadFile("TEST.BIN");
        Assert.Equal(testData.Length, readBack.Length);
        Assert.True(testData.SequenceEqual(readBack));
    }

    [Fact]
    public void WriteFile_NewDisk2DD_CrossCopy()
    {
        using var srcService = Legacy89DiskKitApplication.CreateDiskService();
        var srcContainer = srcService.OpenDisk(XDosSysPath, true);
        var srcFs = (srcService.FileSystem as XDosFileSystem)!;
        var srcBoot = srcFs.ReadBootArea();

        var outputPath = GetRepoPath("images/test/XDOS_XCOPY2DD.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        if (File.Exists(outputPath)) File.Delete(outputPath);

        D88DiskContainer.CreateNew(outputPath, DiskType.TwoDD, "XD_XCPY",
            XDosFileSystem.XDosTrackGeometry);

        using var destService = Legacy89DiskKitApplication.CreateDiskService();
        var destContainer = destService.OpenDisk(outputPath, false);
        var destFs = new XDosFileSystem(destContainer);
        destFs.Format();

        var seen = new HashSet<string>();
        foreach (var e in srcFs.GetFilesWithMetadata().Where(e => !e.IsEmpty))
        {
            var key = $"{BitConverter.ToString(e.RawFileName)}:{e.RawFileType:X4}";
            if (!seen.Add(key)) continue;

            var data = srcFs.ReadFileRaw(e.RawFileName);
            destFs.WriteFileInternal(
                e.FileName, data,
                new ExtendedFileAttributes(FileAttributes.None, e.Attribute, false, "X-DOS"),
                e.StartAddress, e.ExecAddressOrSizeHigh,
                forcedRawName: e.RawFileName, forcedRawType: e.RawFileType);
        }

        destFs.WriteBootArea(srcBoot);
        destContainer.Save();

        Assert.Equal(256, destContainer.ReadSector(0, 0, 1).Length);
        Assert.Equal(512, destContainer.ReadSector(1, 0, 2).Length);

        using var verifyService = Legacy89DiskKitApplication.CreateDiskService();
        verifyService.OpenDisk(outputPath, true);
        var verifyFs = (verifyService.FileSystem as XDosFileSystem)!;

        foreach (var srcEntry in srcFs.GetFilesWithMetadata().Where(e => !e.IsEmpty))
        {
            Assert.True(verifyFs.FileExistsRaw(srcEntry.RawFileName),
                $"Missing: {srcEntry.FileName}");
            var srcData  = srcFs.ReadFileRaw(srcEntry.RawFileName);
            var destData = verifyFs.ReadFileRaw(srcEntry.RawFileName);
            Assert.True(srcData.SequenceEqual(destData),
                $"Parity failure: {srcEntry.FileName} (T{srcEntry.FamPointer.Track} S{srcEntry.FamPointer.Sector})");
        }
    }

    [Fact]
    public void Format_FatBitmap_TracksZeroAndOneAreUsed_Track2IsFree()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_FMT.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        var fatSector = container.ReadSector(0, 1, 1);
        Assert.Equal(0x01, fatSector[0x01]);

        int t0off = 0xA8;
        int t1off = 0xAA;
        Assert.Equal(0x00, fatSector[t0off]);
        Assert.Equal(0x00, fatSector[t0off + 1]);
        Assert.Equal(0x00, fatSector[t1off]);
        Assert.Equal(0x00, fatSector[t1off + 1]);

        int t2off = 0xAC;
        ushort t2word = (ushort)((fatSector[t2off] << 8) | fatSector[t2off + 1]);
        Assert.Equal((ushort)0xFFC0, t2word);
    }

    [Fact]
    public void WriteFile_FamPointerTrack_AtLeastTwo()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_ALLOC.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("TEST.BIN", new byte[100], fs.CreateDefaultAttributes(false));

        var files = fs.GetFilesWithMetadata();
        var entry = files.First(e => e.FileName == "TEST.BIN");
        Assert.True(entry.FamPointer.Track >= 2,
            $"Expected FAM track >= 2, but got {entry.FamPointer.Track}");
    }

    [Fact]
    public void WriteFile_ExceedPhysicalCapacity_ThrowsDiskFull()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_FULL.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        byte[] largeData = new byte[81 * 5120];
        var ex = Assert.Throws<IOException>(() =>
            fs.WriteFile("TOO_BIG.BIN", largeData, fs.CreateDefaultAttributes(false)));
        Assert.Equal("Disk full.", ex.Message);
    }

    [Fact]
    public void WriteFile_TwoHd_AllocatesRecordsIn16SectorTracks()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_2HD.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoHD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("2HD_TEST.BIN", new byte[6000], fs.CreateDefaultAttributes(false));

        var files = fs.GetFilesWithMetadata();
        var entry = files.First(e => e.FileName == "2HD_TEST.BIN");
        Assert.True(entry.FamPointer.Track >= 2);

        var famReader = new XDosFamReader(container);
        var famEntries = famReader.ReadFam(entry.FamPointer);
        Assert.NotEmpty(famEntries);

        var info = fs.GetFileSystemInfo();
        Assert.Equal(512, info.ClusterSize);
    }

    [Fact]
    public void XDosFamReader_ParsesFamTuples_Correctly()
    {
        var sector = new byte[512];
        sector[0] = 0x02; sector[1] = 0x02; sector[2] = 0x0F;
        sector[3] = 0x03; sector[4] = 0x01; sector[5] = 0x08;
        sector[6] = 0x00;

        var entries = XDosFamReader.ParseFam(sector);
        Assert.Equal(2, entries.Count);
        Assert.Equal(0x02, entries[0].Track);
        Assert.Equal(0x02, entries[0].Sector);
        Assert.Equal(0x0F, entries[0].RecordCount);
        Assert.Equal(0x03, entries[1].Track);
        Assert.Equal(0x01, entries[1].Sector);
        Assert.Equal(0x08, entries[1].RecordCount);
    }

    [Fact]
    public void XDosFamWriter_WritesTerminatorAtEnd()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_FAMW.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("FAM_T.BIN", new byte[512], fs.CreateDefaultAttributes(false));

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "FAM_T.BIN");
        int c = entry.FamPointer.Track / 2;
        int h = entry.FamPointer.Track % 2;
        var famSectorData = container.ReadSector(c, h, entry.FamPointer.Sector);

        int pos = 0;
        while (pos + 2 < famSectorData.Length && famSectorData[pos] != 0x00) pos += 3;
        Assert.Equal(0x00, famSectorData[pos]);
    }

    [Fact]
    public void WriteFile_ReadBack_DataIntact()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_RW.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        var rnd = new Random(42);
        byte[] data = new byte[2048];
        rnd.NextBytes(data);

        fs.WriteFile("RAND.BIN", data, fs.CreateDefaultAttributes(false));
        container.Save();

        using var svc2 = Legacy89DiskKitApplication.CreateDiskService();
        svc2.OpenDisk(path, true);
        var fs2 = (svc2.FileSystem as XDosFileSystem)!;

        Assert.True(fs2.FileExists("RAND.BIN"));
        var readBack = fs2.ReadFile("RAND.BIN");
        Assert.Equal(data.Length, readBack.Length);
        Assert.True(data.SequenceEqual(readBack));
    }

    [Fact]
    public void WriteFile_BinaryIntent_DefaultsToFileType0x0100()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_TYPE_BIN.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("BIN_DEF.BIN", new byte[100], fs.CreateDefaultAttributes(false));

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "BIN_DEF.BIN");
        Assert.Equal((ushort)XDosFileType.Bin, entry.RawFileType);
    }

    [Fact]
    public void WriteFile_TextIntent_DefaultsToFileType0x0400()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_TYPE_ASC.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("TXT_DEF.TXT", new byte[100], fs.CreateDefaultAttributes(true));

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "TXT_DEF.TXT");
        Assert.Equal((ushort)XDosFileType.Asc, entry.RawFileType);
    }

    [Fact]
    public void WriteFile_NonzeroRawAttributes_DoNotAlterFileType()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_TYPE_ATTR.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        var attrWithNonZeroRaw = new ExtendedFileAttributes(FileAttributes.None, 0x80, false, "X-DOS");
        fs.WriteFile("ATTR.BIN", new byte[100], attrWithNonZeroRaw);

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "ATTR.BIN");
        Assert.Equal((ushort)XDosFileType.Bin, entry.RawFileType);
        Assert.Equal(0x80, entry.Attribute);
    }

    [Fact]
    public void WriteFileInternal_ExplicitRawType_PreservedUnchanged()
    {
        using var svc = Legacy89DiskKitApplication.CreateDiskService();
        var path = GetRepoPath("images/test/XDOS_TYPE_EXPL.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFileInternal("CMD_F.CMD", new byte[100], fs.CreateDefaultAttributes(false),
            forcedRawType: (ushort)XDosFileType.Cmd);

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "CMD_F.CMD");
        Assert.Equal((ushort)XDosFileType.Cmd, entry.RawFileType);
    }
}
