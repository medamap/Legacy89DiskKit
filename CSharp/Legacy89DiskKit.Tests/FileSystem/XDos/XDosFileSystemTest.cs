using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.XDos;
using Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.XDos.Reader;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.FileSystem.Domain.Model.XDos;
using FileAttributes = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;
using Xunit;
using System.Text;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosFileSystemTest
{
    private static string GetTempPath(string fileName)
    {
        return TestDiskFixtureFactory.CreateTempDiskPath(fileName);
    }

    private string CreateSyntheticXDosDisk(string fileName, int fileCount, DiskType diskType = DiskType.TwoD)
    {
        var path = TestDiskFixtureFactory.CreateFormattedXDosDisk(fileName, diskType);

        using var service = CreateDiskService();
        var container = service.OpenDisk(path, readOnly: false);
        var fs = new XDosFileSystem(container);

        for (var i = 0; i < fileCount; i++)
        {
            var data = Enumerable.Repeat((byte)(0x20 + i), 256 + (i * 16)).ToArray();
            var name = $"SYN{i:D2}.BIN";
            fs.WriteFile(name, data, fs.CreateDefaultAttributes(false), (ushort)(0x8000 + i), (ushort)(0x8000 + i));
        }

        container.Save();
        return path;
    }

    private static string CreateSyntheticHuBasicDisk(string fileName)
    {
        return TestDiskFixtureFactory.CreateFormattedHuBasicDisk(fileName, writeSampleFile: true);
    }

    [Fact]
    public void Provider_CanHandle_XDosDisk_ReturnsTrue_DEBUG()
    {
        var xdosSysPath = CreateSyntheticXDosDisk("Provider_CanHandle_XDosDisk_ReturnsTrue_DEBUG.d88", 5);
        using var diskService = CreateDiskService();
        diskService.OpenDisk(xdosSysPath, true);
        var container = diskService.Session as IDiskContainer;
        Assert.NotNull(container);

        var provider = new XDosFileSystemProvider();
        Assert.True(provider.CanHandle(container));
    }

    [Fact]
    public void Provider_CanHandle_NonXDosDisk_ReturnsFalse()
    {
        var huBasicPath = CreateSyntheticHuBasicDisk("Provider_CanHandle_NonXDosDisk_ReturnsFalse.d88");
        using var diskService = CreateDiskService();
        diskService.OpenDisk(huBasicPath, true);
        var container = diskService.Session as IDiskContainer;
        Assert.NotNull(container);

        var provider = new XDosFileSystemProvider();
        Assert.False(provider.CanHandle(container));
    }

    [Fact]
    public void GetFileSystemInfo_ReturnsValidInfo()
    {
        var xdosSysPath = CreateSyntheticXDosDisk("GetFileSystemInfo_ReturnsValidInfo.d88", 2);
        using var diskService = CreateDiskService();
        diskService.OpenDisk(xdosSysPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var info = fs.GetFileSystemInfo();
        Assert.Equal("X-DOS", info.FileSystemName);
        Assert.Equal("X1", info.PlatformId);
    }

    [Fact]
    public void GetFiles_XDosSys_ReturnsAtLeast5Entries()
    {
        var xdosSysPath = CreateSyntheticXDosDisk("GetFiles_XDosSys_ReturnsAtLeast5Entries.d88", 5);
        using var diskService = CreateDiskService();
        diskService.OpenDisk(xdosSysPath, true);
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
        var xdosUtilPath = CreateSyntheticXDosDisk("GetFiles_XDosUtil_ReturnsAtLeast10Entries.d88", 10);
        using var diskService = CreateDiskService();
        diskService.OpenDisk(xdosUtilPath, true);
        var fs = diskService.FileSystem;
        Assert.NotNull(fs);

        var files = fs.GetFiles().ToList();
        Assert.True(files.Count >= 10);
    }

    [Fact]
    public void FileExists_ReturnsExpectedResult()
    {
        var xdosSysPath = CreateSyntheticXDosDisk("FileExists_ReturnsExpectedResult.d88", 3);
        using var diskService = CreateDiskService();
        diskService.OpenDisk(xdosSysPath, true);
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
        var xdosSysPath = CreateSyntheticXDosDisk("Registry_AutoDetects_XDos.d88", 3);
        var registry = CreateFileSystemRegistry();
        using var diskService = new DiskService(fsRegistry: registry);
        diskService.OpenDisk(xdosSysPath, true);

        Assert.NotNull(diskService.FileSystem);
        Assert.IsType<XDosFileSystem>(diskService.FileSystem);
    }

    [Fact]
    public void ExplicitResolver_CanCreate_XDos()
    {
        var xdosSysPath = CreateSyntheticXDosDisk("ExplicitResolver_CanCreate_XDos.d88", 1);
        var resolver = new ExplicitFileSystemResolver();
        using var diskService = CreateDiskService();
        var container = diskService.OpenDisk(xdosSysPath, true);

        var fs = resolver.Create("xdos", container);
        Assert.NotNull(fs);
        Assert.IsType<XDosFileSystem>(fs);
    }

    [Fact]
    public void WriteFile_NewDisk2DD_WriteAndRead_RoundTrip()
    {
        var outputPath = GetTempPath("XDOS_NEW2DD.D88");
        if (File.Exists(outputPath)) File.Delete(outputPath);

        D88DiskContainer.CreateNew(outputPath, DiskType.TwoDD, "XD_NEW2DD",
            (c, h) => XDosMediaGeometry.FromDiskType(DiskType.TwoDD).GetTrackGeometry(c, h));

        using var destService = CreateDiskService();
        var destContainer = destService.OpenDisk(outputPath, false);
        var destFs = new XDosFileSystem(destContainer);
        destFs.Format();

        byte[] testData = Enumerable.Range(0, 1024).Select(i => (byte)(i & 0xFF)).ToArray();
        destFs.WriteFile("TEST.BIN", testData, destFs.CreateDefaultAttributes(false), 0x8000, 0x8000);
        destContainer.Save();

        using var verifyService = CreateDiskService();
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
        var xdosSysPath = CreateSyntheticXDosDisk("WriteFile_NewDisk2DD_CrossCopy_SRC.d88", 4);
        using var srcService = CreateDiskService();
        var srcContainer = srcService.OpenDisk(xdosSysPath, true);
        var srcFs = (srcService.FileSystem as XDosFileSystem)!;
        var srcBoot = srcFs.ReadBootArea();

        var outputPath = GetTempPath("XDOS_XCOPY2DD.D88");
        if (File.Exists(outputPath)) File.Delete(outputPath);

        D88DiskContainer.CreateNew(outputPath, DiskType.TwoDD, "XD_XCPY",
            (c, h) => XDosMediaGeometry.FromDiskType(DiskType.TwoDD).GetTrackGeometry(c, h));

        using var destService = CreateDiskService();
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

        using var verifyService = CreateDiskService();
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_FMT.D88");
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
    public void Format_DiskServiceCreate_TwoD_RebuildsToXDosGeometry()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_FMT_CLI_PATH.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (File.Exists(path)) File.Delete(path);

        var container = svc.CreateDisk(path, DiskType.TwoD);
        var fs = new XDosFileSystem(container);

        Assert.Equal(256, container.ReadSector(0, 1, 1).Length);
        Assert.True(container.SectorExists(0, 1, 16));
        Assert.True(container.SectorExists(1, 0, 11));

        fs.Format();

        Assert.Equal(256, container.ReadSector(0, 0, 1).Length);
        Assert.Equal(512, container.ReadSector(0, 1, 1).Length);
        Assert.Equal(512, container.ReadSector(1, 0, 10).Length);
        Assert.False(container.SectorExists(0, 1, 11));
        Assert.False(container.SectorExists(1, 0, 11));
    }

    [Fact]
    public void WriteFile_FamPointerTrack_AtLeastTwo()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_ALLOC.D88");
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_FULL.D88");
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_2HD.D88");
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_FAMW.D88");
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_RW.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        var rnd = new Random(42);
        byte[] data = new byte[2048];
        rnd.NextBytes(data);

        fs.WriteFile("RAND.BIN", data, fs.CreateDefaultAttributes(false));
        container.Save();

        using var svc2 = CreateDiskService();
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_TYPE_BIN.D88");
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_TYPE_ASC.D88");
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_TYPE_ATTR.D88");
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
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_TYPE_EXPL.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFileInternal("CMD_F.CMD", new byte[100], fs.CreateDefaultAttributes(false),
            forcedRawType: (ushort)XDosFileType.Cmd);

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "CMD_F.CMD");
        Assert.Equal((ushort)XDosFileType.Cmd, entry.RawFileType);
    }

    [Fact]
    public void WriteFile_CommitOrder_FatAndFamBothPresentAfterWrite()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_ORDER_FAT.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("ORDER.BIN", new byte[512], fs.CreateDefaultAttributes(false));

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "ORDER.BIN");

        var famC = entry.FamPointer.Track / 2;
        var famH = entry.FamPointer.Track % 2;
        var famSectorData = container.ReadSector(famC, famH, entry.FamPointer.Sector);
        Assert.NotEqual(0x00, famSectorData[0]);

        var fatSector = container.ReadSector(0, 1, 1);
        int famOffset = 0xA8 + entry.FamPointer.Track * 2;
        ushort famTrackWord = (ushort)((fatSector[famOffset] << 8) | fatSector[famOffset + 1]);
        bool famSectorBitUsed = ((famTrackWord >> (16 - entry.FamPointer.Sector)) & 1) == 0;
        Assert.True(famSectorBitUsed, "FAT must mark the FAM sector as used after write completes");
    }

    [Fact]
    public void WriteFile_DirectoryEntry_FamPointerBytesMatch()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_ORDER_DIR.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("DIRCHK.BIN", new byte[256], fs.CreateDefaultAttributes(false));

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "DIRCHK.BIN");

        var dirSector = container.ReadSector(0, 1, 2);
        int offset = 0;
        while (offset + 32 <= dirSector.Length)
        {
            ushort rawType = (ushort)((dirSector[offset] << 8) | dirSector[offset + 1]);
            if (rawType != 0x0000 && rawType != 0xFFFF) break;
            offset += 32;
        }

        Assert.Equal(entry.FamPointer.Track,  dirSector[offset + 0x1D]);
        Assert.Equal(entry.FamPointer.Sector, dirSector[offset + 0x1E]);
        Assert.Equal(entry.FamPointer.Record, dirSector[offset + 0x1F]);
    }

    [Fact]
    public void WriteFile_TwoConsecutiveFiles_NoDataOverwrite()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_ORDER_NOOVR.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        byte[] data1 = Enumerable.Repeat((byte)0xAA, 512).ToArray();
        byte[] data2 = Enumerable.Repeat((byte)0x55, 512).ToArray();

        fs.WriteFile("FIRST.BIN",  data1, fs.CreateDefaultAttributes(false));
        fs.WriteFile("SECOND.BIN", data2, fs.CreateDefaultAttributes(false));

        var read1 = fs.ReadFile("FIRST.BIN");
        var read2 = fs.ReadFile("SECOND.BIN");

        Assert.True(data1.SequenceEqual(read1), "FIRST.BIN data was overwritten");
        Assert.True(data2.SequenceEqual(read2), "SECOND.BIN data was overwritten");
    }

    [Fact]
    public void WriteFile_CommitOrder_DataRoundTripAfterSave()
    {
        var path = GetTempPath("XDOS_ORDER_RT.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        {
            using var svc = CreateDiskService();
            var container = svc.CreateDisk(path, DiskType.TwoDD);
            var fs = new XDosFileSystem(container);
            fs.Format();

            byte[] data = new byte[1536];
            for (int i = 0; i < data.Length; i++) data[i] = (byte)(i % 251);
            fs.WriteFile("RT_TEST.BIN", data, fs.CreateDefaultAttributes(false));
            container.Save();
        }

        {
            using var svc2 = CreateDiskService();
            svc2.OpenDisk(path, true);
            var fs2 = (svc2.FileSystem as XDosFileSystem)!;

            Assert.True(fs2.FileExists("RT_TEST.BIN"));
            var readBack = fs2.ReadFile("RT_TEST.BIN");
            Assert.Equal(1536, readBack.Length);
            for (int i = 0; i < readBack.Length; i++)
                Assert.Equal((byte)(i % 251), readBack[i]);
        }
    }

    [Fact]
    public void Format_D88_BeforeFormat_Generic2D_Is16x256()
    {
        var container = D88DiskContainer.CreateNewInMemory("GEOM_PRE", DiskType.TwoD);
        Assert.Equal(256, container.ReadSector(0, 0, 1).Length);
        Assert.Equal(256, container.ReadSector(1, 0, 1).Length);
        Assert.True(container.SectorExists(0, 0, 16));
        Assert.False(container.SectorExists(0, 0, 17));
        Assert.True(container.SectorExists(1, 0, 16));
        Assert.False(container.SectorExists(1, 0, 17));
    }

    [Fact]
    public void Format_D88_AfterXDosFormat_Track0Is16x256_OtherTracksAre10x512()
    {
        var container = D88DiskContainer.CreateNewInMemory("GEOM_AFT", DiskType.TwoD);
        new XDosFileSystem(container).Format();

        Assert.Equal(256, container.ReadSector(0, 0, 1).Length);
        Assert.True(container.SectorExists(0, 0, 16));
        Assert.False(container.SectorExists(0, 0, 17));

        Assert.Equal(512, container.ReadSector(1, 0, 1).Length);
        Assert.True(container.SectorExists(1, 0, 10));
        Assert.False(container.SectorExists(1, 0, 11));

        Assert.Equal(512, container.ReadSector(39, 0, 1).Length);
        Assert.True(container.SectorExists(39, 1, 10));
        Assert.False(container.SectorExists(39, 1, 11));
    }

    [Fact]
    public void Format_D88_ChangesImageBytes_AfterGeometryRebuild()
    {
        var container = D88DiskContainer.CreateNewInMemory("GEOM_CHG", DiskType.TwoD);
        var before = container.ToImageData();

        new XDosFileSystem(container).Format();

        var after = container.ToImageData();
        Assert.NotEqual(before.Length, after.Length);
    }

    [Fact]
    public void Format_D88_RepeatedFormat_PreservesXDosGeometry()
    {
        var container = D88DiskContainer.CreateNewInMemory("GEOM_RPT", DiskType.TwoD);
        var fs = new XDosFileSystem(container);
        fs.Format();
        fs.Format();

        Assert.Equal(256, container.ReadSector(0, 0, 1).Length);
        Assert.Equal(512, container.ReadSector(1, 0, 1).Length);
        Assert.True(container.SectorExists(1, 0, 10));
        Assert.False(container.SectorExists(1, 0, 11));
    }

    [Fact]
    public void Format_D88_GeometryParityWithExpectedTwoDProfile()
    {
        var container = D88DiskContainer.CreateNewInMemory("GEOM_PAR", DiskType.TwoD);
        new XDosFileSystem(container).Format();

        Assert.Equal(256, container.ReadSector(0, 0, 1).Length);
        Assert.Equal(512, container.ReadSector(1, 0, 1).Length);

        Assert.True(container.SectorExists(0, 0, 16));
        Assert.False(container.SectorExists(0, 0, 17));

        Assert.True(container.SectorExists(1, 0, 10));
        Assert.False(container.SectorExists(1, 0, 11));
    }

    [Fact]
    public void Format_RawContainer_DoesNotChangeContainerSize()
    {
        var rawContainer = RawDiskContainer.CreateNewInMemory(DiskType.TwoD);
        int sizeBefore = rawContainer.ToImageData().Length;
        var fs = new XDosFileSystem(rawContainer);
        try { fs.Format(); } catch { }
        Assert.Equal(sizeBefore, rawContainer.ToImageData().Length);
    }

    [Fact]
    public void Format_D88_2HD_Track0Is16x256_DataTracksAre16x512_Sector17Missing()
    {
        var container = D88DiskContainer.CreateNewInMemory("GEOM_2HD", DiskType.TwoHD);
        new XDosFileSystem(container).Format();

        Assert.Equal(256, container.ReadSector(0, 0, 1).Length);
        Assert.True(container.SectorExists(0, 0, 16));
        Assert.False(container.SectorExists(0, 0, 17));

        Assert.Equal(512, container.ReadSector(0, 1, 1).Length);
        Assert.True(container.SectorExists(0, 1, 16));
        Assert.False(container.SectorExists(0, 1, 17));
    }

    [Fact]
    public void ToFileEntry_XDosAsc_ShouldBeIsAsciiTrue()
    {
        var path = GetTempPath("XDOS_ISASCII_PROJECTION.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (File.Exists(path)) File.Delete(path);

        using var svc = CreateDiskService();
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("TEXT.TXT", new byte[10], fs.CreateDefaultAttributes(true));
        fs.WriteFile("BIN.BIN", new byte[10], fs.CreateDefaultAttributes(false));

        var files = fs.GetFiles().ToList();
        var textFile = files.First(f => f.FileName == "TEXT.TXT");
        var binFile = files.First(f => f.FileName == "BIN.BIN");

        Assert.True(textFile.Attributes.IsAscii);
        Assert.False(binFile.Attributes.IsAscii);
    }

    [Fact]
    public void ToFileEntry_ShouldPreserveRawAttributes()
    {
        var path = GetTempPath("XDOS_ATTR_PROJECTION_PRESERVE.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (File.Exists(path)) File.Delete(path);

        using var svc = CreateDiskService();
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        var attr = new ExtendedFileAttributes(FileAttributes.None, 0x5A, true, "X-DOS");
        fs.WriteFile("ATTR.TXT", new byte[10], attr);

        var file = fs.GetFiles().First(f => f.FileName == "ATTR.TXT");
        Assert.Equal(0x5A, file.Attributes.RawAttributes);
        Assert.True(file.Attributes.IsAscii);
    }

    [Fact]
    public void ToFileEntry_EndAddress_IsProjectedCorrectlyByFileType()
    {
        var outputPath = GetTempPath("XDOS_PROJECTION_TEST.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        if (File.Exists(outputPath)) File.Delete(outputPath);

        using var destService = CreateDiskService();
        var destContainer = destService.CreateDisk(outputPath, DiskType.TwoD);
        var destFs = new XDosFileSystem(destContainer);
        destFs.Format();

        destFs.WriteFile("TEST.BIN", new byte[100], destFs.CreateDefaultAttributes(false), 0x8000, 0x8000);
        destFs.WriteFile("TEST.TXT", new byte[100], destFs.CreateDefaultAttributes(true));

        var testFiles = destFs.GetFiles().ToList();
        var binEntry = testFiles.First(f => f.FileName == "TEST.BIN");
        var ascEntry = testFiles.First(f => f.FileName == "TEST.TXT");

        Assert.NotNull(binEntry.EndAddress);
        Assert.Equal((ushort)0x8064, binEntry.EndAddress);
        Assert.NotNull(binEntry.ExecutionAddress);
        Assert.Equal((ushort)0x8000, binEntry.ExecutionAddress);
        Assert.Null(ascEntry.EndAddress);
        Assert.Null(ascEntry.ExecutionAddress);
    }

    [Fact]
    public void GetFiles_ProjectsValidTimestamp()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_TIMESTAMP_VALID.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("TIME.BIN", new byte[10], fs.CreateDefaultAttributes(false));

        var dirSector = container.ReadSector(0, 1, 2);
        int offset = -1;
        for (int i = 0; i + 32 <= dirSector.Length; i += 32)
        {
            if (Encoding.Latin1.GetString(dirSector, i + 2, 16).Trim() == "TIME.BIN")
            {
                offset = i;
                break;
            }
        }
        Assert.True(offset >= 0, "Could not find TIME.BIN entry in directory");

        dirSector[offset + 0x18] = 0x00;
        dirSector[offset + 0x19] = 0x26;
        dirSector[offset + 0x1A] = 0x03;
        dirSector[offset + 0x1B] = 0x28;
        container.WriteSector(0, 1, 2, dirSector);

        var fs2 = new XDosFileSystem(container);
        var file = fs2.GetFiles().First(f => f.FileName.Trim() == "TIME.BIN");

        Assert.NotNull(file.LastModifiedAt);
        Assert.Equal(2026, file.LastModifiedAt.Value.Year);
        Assert.Equal(3, file.LastModifiedAt.Value.Month);
        Assert.Equal(28, file.LastModifiedAt.Value.Day);
    }

    [Fact]
    public void GetFiles_ZeroTimestamp_ProjectsNull()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_TIMESTAMP_ZERO.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("ZERO.BIN", new byte[10], fs.CreateDefaultAttributes(false));

        var file = fs.GetFiles().First(f => f.FileName.Trim() == "ZERO.BIN");
        Assert.Null(file.LastModifiedAt);
    }

    [Fact]
    public void GetFiles_InvalidTimestamp_ProjectsNull()
    {
        using var svc = CreateDiskService();
        var path = GetTempPath("XDOS_TIMESTAMP_INVALID.D88");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var container = svc.CreateDisk(path, DiskType.TwoDD);
        var fs = new XDosFileSystem(container);
        fs.Format();

        fs.WriteFile("INVALID.BIN", new byte[10], fs.CreateDefaultAttributes(false));

        var dirSector = container.ReadSector(0, 1, 2);
        int offset = -1;
        for (int i = 0; i + 32 <= dirSector.Length; i += 32)
        {
            if (Encoding.Latin1.GetString(dirSector, i + 2, 16).Trim() == "INVALID.BIN")
            {
                offset = i;
                break;
            }
        }
        Assert.True(offset >= 0, "Could not find INVALID.BIN entry in directory");

        dirSector[offset + 0x18] = 0x00;
        dirSector[offset + 0x19] = 0x26;
        dirSector[offset + 0x1A] = 0x13;
        dirSector[offset + 0x1B] = 0x01;
        container.WriteSector(0, 1, 2, dirSector);

        var fs2 = new XDosFileSystem(container);
        var file = fs2.GetFiles().First(f => f.FileName.Trim() == "INVALID.BIN");

        Assert.Null(file.LastModifiedAt);
    }

    private static DiskService CreateDiskService()
    {
        return new DiskService(fsRegistry: CreateFileSystemRegistry());
    }

    private static IFileSystemRegistry CreateFileSystemRegistry()
    {
        var registry = new FileSystemRegistry();
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider.XDosFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider.HuBasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Cpm.Provider.CpmFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Provider.N88BasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Msx.Provider.MsxDosFileSystemProvider());
        return registry;
    }
}
