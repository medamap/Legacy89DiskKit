using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Model.XDos;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Legacy89DiskKit.FileSystem.Infrastructure.XDos;
using FileAttributes = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;
using Xunit;

namespace Legacy89DiskKit.Tests.FileSystem.XDos;

public class XDosTransferAdapterTest
{
    private (IDiskContainer container, XDosFileSystem fs) CreateFormattedDisk(string name)
    {
        return TestDiskFixtureFactory.CreateOpenFormattedXDos($"{name}.D88", DiskType.TwoDD);
    }

    private static FileEntry GetFileEntry(XDosFileSystem fs, string fileName)
        => fs.GetFiles().First(e => e.FileName == fileName);

    [Fact]
    public void Export_BinaryFile_HasBinaryContentKindAndMetadata()
    {
        var (container, fs) = CreateFormattedDisk("TA_EXPORT_BIN");

        byte[] data = Enumerable.Repeat((byte)0xAB, 256).ToArray();
        fs.WriteFile("BIN_F.BIN", data, fs.CreateDefaultAttributes(false), 0x8000, 0x8000);

        var adapter = new XDosTransferAdapter(fs);
        var env = adapter.Export(GetFileEntry(fs, "BIN_F.BIN"));

        Assert.Equal(ContentKind.Binary, env.ContentKind);
        Assert.Equal("X-DOS", env.SourceFileSystemId);
        Assert.True(env.Metadata!.ContainsKey("xdos.fileType"));
        Assert.Equal("0100", env.Metadata["xdos.fileType"]);
        Assert.Equal("false", env.Metadata["xdos.isAscii"]);
        Assert.Equal((ushort)0x8000, env.LoadAddress);
        Assert.Equal((ushort)0x8000, env.ExecutionAddress);
        Assert.True(data.SequenceEqual(env.Payload));
    }

    [Fact]
    public void Export_AscFile_HasTextContentKindAndMetadata()
    {
        var (container, fs) = CreateFormattedDisk("TA_EXPORT_ASC");

        byte[] data = System.Text.Encoding.ASCII.GetBytes("HELLO WORLD\r\n");
        fs.WriteFile("TEXT_F.TXT", data, fs.CreateDefaultAttributes(true));

        var adapter = new XDosTransferAdapter(fs);
        var env = adapter.Export(GetFileEntry(fs, "TEXT_F.TXT"));

        Assert.Equal(ContentKind.Text, env.ContentKind);
        Assert.Equal("0400", env.Metadata!["xdos.fileType"]);
        Assert.Equal("true", env.Metadata["xdos.isAscii"]);
        Assert.Equal("shift_jis", env.EncodingId);
    }

    [Fact]
    public void Export_AscFile_DoesNotSetExecutionAddress()
    {
        var (container, fs) = CreateFormattedDisk("TA_ASC_NOEXEC");

        byte[] data = System.Text.Encoding.ASCII.GetBytes("LINE1\r\nLINE2\r\n");
        fs.WriteFile("ASC_E.TXT", data, fs.CreateDefaultAttributes(true));

        var adapter = new XDosTransferAdapter(fs);
        var env = adapter.Export(GetFileEntry(fs, "ASC_E.TXT"));

        Assert.Equal(ContentKind.Text, env.ContentKind);
        Assert.Null(env.ExecutionAddress);
    }

    [Fact]
    public void Export_LargeAscFile_SizeHighNotExportedAsExecutionAddress()
    {
        var (container, fs) = CreateFormattedDisk("TA_ASC_LARGE");

        byte[] data = new byte[600];
        System.Text.Encoding.ASCII.GetBytes("A").CopyTo(data, 0);
        fs.WriteFileInternal("LARGE.ASC", data, fs.CreateDefaultAttributes(true),
            executionAddress: 0x0001,
            forcedRawType: (ushort)XDosFileType.Asc);

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "LARGE.ASC");
        Assert.NotEqual(0, entry.ExecAddressOrSizeHigh);

        var adapter = new XDosTransferAdapter(fs);
        var env = adapter.Export(GetFileEntry(fs, "LARGE.ASC"));

        Assert.Equal(ContentKind.Text, env.ContentKind);
        Assert.Null(env.ExecutionAddress);
    }

    [Fact]
    public void Export_NonAscType_HasBinaryContentKind()
    {
        var (container, fs) = CreateFormattedDisk("TA_EXPORT_SYS");

        byte[] data = new byte[512];
        fs.WriteFileInternal("SYS_F.SYS", data, fs.CreateDefaultAttributes(false),
            forcedRawType: (ushort)XDosFileType.Sys);

        var adapter = new XDosTransferAdapter(fs);
        var env = adapter.Export(GetFileEntry(fs, "SYS_F.SYS"));

        Assert.Equal(ContentKind.Binary, env.ContentKind);
        Assert.Equal("0700", env.Metadata!["xdos.fileType"]);
        Assert.Null(env.EncodingId);
    }

    [Fact]
    public void Export_BinaryFile_PreservesExecutionAddress()
    {
        var (container, fs) = CreateFormattedDisk("TA_BIN_EXEC");

        byte[] data = new byte[256];
        fs.WriteFileInternal("EXEC.CMD", data, fs.CreateDefaultAttributes(false),
            loadAddress: 0xB000, executionAddress: 0xB100,
            forcedRawType: (ushort)XDosFileType.Cmd);

        var adapter = new XDosTransferAdapter(fs);
        var env = adapter.Export(GetFileEntry(fs, "EXEC.CMD"));

        Assert.Equal(ContentKind.Binary, env.ContentKind);
        Assert.Equal((ushort)0xB000, env.LoadAddress);
        Assert.Equal((ushort)0xB100, env.ExecutionAddress);
    }

    [Fact]
    public void Import_FromXDosSource_PreservesRawFileType()
    {
        var (container, fs) = CreateFormattedDisk("TA_IMPORT_XDOS");

        var envelope = new TransferFileEnvelope(
            FileName:          "CMD_F.CMD",
            Payload:           new byte[128],
            ContentKind:       ContentKind.Binary,
            SourceFileSystemId: "X-DOS",
            LoadAddress:       0xC000,
            ExecutionAddress:  0xC000,
            Timestamp:         null,
            EncodingId:        null,
            Metadata:          new Dictionary<string, string>
            {
                ["xdos.fileType"]      = "0300",
                ["xdos.rawAttributes"] = "00",
                ["xdos.isAscii"]       = "false",
            });

        var adapter = new XDosTransferAdapter(fs);
        adapter.Import(envelope, "CMD_F.CMD");

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "CMD_F.CMD");
        Assert.Equal((ushort)XDosFileType.Cmd, entry.RawFileType);
        Assert.Equal((ushort)0xC000, entry.StartAddress);
    }

    [Fact]
    public void Import_FromXDosSource_PreservesRawAttributes()
    {
        var (container, fs) = CreateFormattedDisk("TA_IMPORT_ATTR");

        var envelope = new TransferFileEnvelope(
            FileName:          "ATTR_F.BIN",
            Payload:           new byte[64],
            ContentKind:       ContentKind.Binary,
            SourceFileSystemId: "X-DOS",
            LoadAddress:       null,
            ExecutionAddress:  null,
            Timestamp:         null,
            EncodingId:        null,
            Metadata:          new Dictionary<string, string>
            {
                ["xdos.fileType"]      = "0100",
                ["xdos.rawAttributes"] = "80",
                ["xdos.isAscii"]       = "false",
            });

        var adapter = new XDosTransferAdapter(fs);
        adapter.Import(envelope, "ATTR_F.BIN");

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "ATTR_F.BIN");
        Assert.Equal((ushort)XDosFileType.Bin, entry.RawFileType);
        Assert.Equal(0x80, entry.Attribute);
    }

    [Fact]
    public void Import_FromNonXDosSource_BinaryUsesDefaultBinType()
    {
        var (container, fs) = CreateFormattedDisk("TA_IMPORT_NONDEF");

        var envelope = new TransferFileEnvelope(
            FileName:          "HOST_F.BIN",
            Payload:           new byte[256],
            ContentKind:       ContentKind.Binary,
            SourceFileSystemId: "HU-BASIC",
            LoadAddress:       null,
            ExecutionAddress:  null,
            Timestamp:         null,
            EncodingId:        null,
            Metadata:          null);

        var adapter = new XDosTransferAdapter(fs);
        adapter.Import(envelope, "HOST_F.BIN");

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "HOST_F.BIN");
        Assert.Equal((ushort)XDosFileType.Bin, entry.RawFileType);
    }

    [Fact]
    public void Import_FromNonXDosSource_TextUsesDefaultAscType()
    {
        var (container, fs) = CreateFormattedDisk("TA_IMPORT_NONTEXT");

        var envelope = new TransferFileEnvelope(
            FileName:          "HOST_T.TXT",
            Payload:           System.Text.Encoding.ASCII.GetBytes("HELLO\r\n"),
            ContentKind:       ContentKind.Text,
            SourceFileSystemId: "HU-BASIC",
            LoadAddress:       null,
            ExecutionAddress:  null,
            Timestamp:         null,
            EncodingId:        "shift_jis",
            Metadata:          null);

        var adapter = new XDosTransferAdapter(fs);
        adapter.Import(envelope, "HOST_T.TXT");

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "HOST_T.TXT");
        Assert.Equal((ushort)XDosFileType.Asc, entry.RawFileType);
    }

    [Fact]
    public void XDosToXDos_RoundTrip_PreservesAllMetadata()
    {
        var (srcContainer, srcFs) = CreateFormattedDisk("TA_RT_SRC");
        var (dstContainer, dstFs) = CreateFormattedDisk("TA_RT_DST");

        byte[] data = new byte[512];
        new Random(99).NextBytes(data);

        srcFs.WriteFileInternal("RT_F.CMD", data,
            new ExtendedFileAttributes(FileAttributes.None, 0x40, false, "X-DOS"),
            loadAddress: 0xA000, executionAddress: 0xA100,
            forcedRawType: (ushort)XDosFileType.Cmd);

        var srcAdapter = new XDosTransferAdapter(srcFs);
        var dstAdapter = new XDosTransferAdapter(dstFs);

        var envelope = srcAdapter.Export(GetFileEntry(srcFs, "RT_F.CMD"));
        dstAdapter.Import(envelope, "RT_F.CMD");

        var entry = dstFs.GetFilesWithMetadata().First(e => e.FileName == "RT_F.CMD");
        Assert.Equal((ushort)XDosFileType.Cmd, entry.RawFileType);
        Assert.Equal(0x40, entry.Attribute);
        Assert.Equal((ushort)0xA000, entry.StartAddress);
        Assert.Equal((ushort)0xA100, entry.ExecAddressOrSizeHigh);

        var readBack = dstFs.ReadFileRaw(entry.RawFileName);
        Assert.True(data.SequenceEqual(readBack));
    }

    [Fact]
    public void XDosToXDos_RoundTrip_PreservesTimestamp()
    {
        var (srcContainer, srcFs) = CreateFormattedDisk("TA_RT_TS_SRC");
        var (dstContainer, dstFs) = CreateFormattedDisk("TA_RT_TS_DST");

        byte[] data = new byte[128];
        uint expectedTs = 0x260328; // 2026-03-28 in X-DOS BCD (assumed)

        srcFs.WriteFileInternal("TS_F.BIN", data, srcFs.CreateDefaultAttributes(false),
            forcedTimestampRaw: expectedTs);

        var srcAdapter = new XDosTransferAdapter(srcFs);
        var dstAdapter = new XDosTransferAdapter(dstFs);

        var envelope = srcAdapter.Export(GetFileEntry(srcFs, "TS_F.BIN"));
        Assert.Equal(expectedTs.ToString("X6"), envelope.Metadata!["xdos.timestampRaw"]);

        dstAdapter.Import(envelope, "TS_F.BIN");

        var entry = dstFs.GetFilesWithMetadata().First(e => e.FileName == "TS_F.BIN");
        Assert.Equal(expectedTs, entry.TimestampRaw);
    }

    [Fact]
    public void XDosToXDos_TransferAll_PreservesRawNameWithSpaces()
    {
        var (srcContainer, srcFs) = CreateFormattedDisk("TA_RT_SPACE_SRC");
        var (dstContainer, dstFs) = CreateFormattedDisk("TA_RT_SPACE_DST");

        byte[] data = Enumerable.Repeat((byte)0x5A, 512).ToArray();
        byte[] rawName = System.Text.Encoding.Latin1.GetBytes("X-DOS System    ");

        srcFs.WriteFileInternal(
            "X-DOS System",
            data,
            new ExtendedFileAttributes(FileAttributes.None, 0x80, false, "X-DOS"),
            loadAddress: 0x4000,
            executionAddress: 0x4000,
            forcedRawName: rawName,
            forcedRawType: (ushort)XDosFileType.Sys);

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.TransferAll(srcFs, dstFs);

        var dstEntry = dstFs.GetFilesWithMetadata().Single();
        var srcSlot = srcFs.FindDirectorySlot(rawName, (ushort)XDosFileType.Sys);
        var dstSlot = dstFs.FindDirectorySlot(rawName, (ushort)XDosFileType.Sys);

        Assert.Equal("X-DOS System", dstEntry.FileName);
        Assert.True(rawName.SequenceEqual(dstEntry.RawFileName));
        Assert.Equal((ushort)XDosFileType.Sys, dstEntry.RawFileType);
        Assert.Equal(srcSlot, dstSlot);
    }

    [Fact]
    public void Import_Utf8Payload_ConvertsToShiftJis()
    {
        var (container, fs) = CreateFormattedDisk("TA_IMPORT_UTF8");

        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes("Hello World\r\n");

        var envelope = new TransferFileEnvelope(
            FileName:          "UTF8_F.TXT",
            Payload:           utf8Bytes,
            ContentKind:       ContentKind.Text,
            SourceFileSystemId: null,
            LoadAddress:       null,
            ExecutionAddress:  null,
            Timestamp:         null,
            EncodingId:        "utf-8",
            Metadata:          null);

        var adapter = new XDosTransferAdapter(fs);
        adapter.Import(envelope, "UTF8_F.TXT");

        var entry = fs.GetFilesWithMetadata().First(e => e.FileName == "UTF8_F.TXT");
        Assert.Equal((ushort)XDosFileType.Asc, entry.RawFileType);

        var readBack = fs.ReadFile("UTF8_F.TXT");
        string decoded = System.Text.Encoding.GetEncoding(932).GetString(readBack);
        Assert.Contains("Hello World", decoded);
    }

    [Fact]
    public void Export_FileNotFound_ThrowsFileNotFoundException()
    {
        var (container, fs) = CreateFormattedDisk("TA_NOTFOUND");
        var adapter = new XDosTransferAdapter(fs);

        var fakeEntry = new FileEntry("MISSING.BIN", "", 0, null, null,
            fs.CreateDefaultAttributes(false));

        Assert.Throws<FileNotFoundException>(() => adapter.Export(fakeEntry));
    }

    [Fact]
    public void Adapter_FileSystemId_ReturnsXDos()
    {
        var (container, fs) = CreateFormattedDisk("TA_FSID");
        var adapter = new XDosTransferAdapter(fs);
        Assert.Equal("X-DOS", adapter.FileSystemId);
    }

    [Fact]
    public void Orchestrator_TransferAll_CopiesAllFilesViaRegisteredAdapter()
    {
        var (srcContainer, srcFs) = CreateFormattedDisk("TA_ORCH_SRC");
        var (dstContainer, dstFs) = CreateFormattedDisk("TA_ORCH_DST");

        byte[] data1 = new byte[256]; data1[0] = 0x11;
        byte[] data2 = new byte[128]; data2[0] = 0x22;

        srcFs.WriteFile("FILE1.BIN", data1, srcFs.CreateDefaultAttributes(false), 0x8000, 0x8000);
        srcFs.WriteFile("FILE2.BIN", data2, srcFs.CreateDefaultAttributes(false), 0x9000, 0x9000);

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.TransferAll(srcFs, dstFs);

        Assert.True(dstFs.FileExists("FILE1.BIN"));
        Assert.True(dstFs.FileExists("FILE2.BIN"));

        Assert.True(data1.SequenceEqual(dstFs.ReadFile("FILE1.BIN")));
        Assert.True(data2.SequenceEqual(dstFs.ReadFile("FILE2.BIN")));
    }

    [Fact]
    public void Orchestrator_Transfer_CopiesSingleFileViaRegisteredAdapter()
    {
        var (srcContainer, srcFs) = CreateFormattedDisk("TA_ORCH1_SRC");
        var (dstContainer, dstFs) = CreateFormattedDisk("TA_ORCH1_DST");

        byte[] data = Enumerable.Repeat((byte)0x42, 512).ToArray();
        srcFs.WriteFileInternal("SRC.CMD", data, srcFs.CreateDefaultAttributes(false),
            loadAddress: 0xD000, executionAddress: 0xD100,
            forcedRawType: (ushort)XDosFileType.Cmd);

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.Transfer(srcFs, dstFs, "SRC.CMD", "DST.CMD");

        var dstEntry = dstFs.GetFilesWithMetadata().First(e => e.FileName == "DST.CMD");
        Assert.Equal((ushort)XDosFileType.Cmd, dstEntry.RawFileType);
        Assert.Equal((ushort)0xD000, dstEntry.StartAddress);
        Assert.Equal((ushort)0xD100, dstEntry.ExecAddressOrSizeHigh);
        Assert.True(data.SequenceEqual(dstFs.ReadFileRaw(dstEntry.RawFileName)));
    }

    [Fact]
    public void XDosToXDos_RoundTrip_PreservesTimestampRaw()
    {
        var (srcContainer, srcFs) = CreateFormattedDisk("TA_RT_TS_SRC");
        var (dstContainer, dstFs) = CreateFormattedDisk("TA_RT_TS_DST");

        byte[] data = new byte[128];
        uint originalTimestampRaw = 0x152CBAD5;

        srcFs.WriteFileInternal("TS_F.BIN", data, srcFs.CreateDefaultAttributes(false),
            forcedTimestampRaw: originalTimestampRaw);

        var srcAdapter = new XDosTransferAdapter(srcFs);
        var dstAdapter = new XDosTransferAdapter(dstFs);

        var envelope = srcAdapter.Export(GetFileEntry(srcFs, "TS_F.BIN"));
        dstAdapter.Import(envelope, "TS_F.BIN");

        var entry = dstFs.GetFilesWithMetadata().First(e => e.FileName == "TS_F.BIN");
        Assert.Equal(originalTimestampRaw, entry.TimestampRaw);
    }
}
