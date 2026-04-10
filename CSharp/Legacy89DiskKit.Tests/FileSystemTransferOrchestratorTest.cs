using Legacy89DiskKit.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class FileSystemTransferOrchestratorTest
{
    private XDosFileSystem CreateFormattedXDos(string name)
    {
        var (_, fs) = TestDiskFixtureFactory.CreateOpenFormattedXDos($"{name}.D88", DiskType.TwoDD);
        return fs;
    }

    [Fact]
    public void Transfer_InstanceRegistration_ResolvesCorrectAdapterPerInstance()
    {
        var srcFs = CreateFormattedXDos("ORCH_INST_SRC");
        var dstFs = CreateFormattedXDos("ORCH_INST_DST");

        byte[] data = new byte[256]; data[0] = 0xCC;
        srcFs.WriteFile("INST.BIN", data, srcFs.CreateDefaultAttributes(false));

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.Transfer(srcFs, dstFs, "INST.BIN", "INST.BIN");

        Assert.True(dstFs.FileExists("INST.BIN"));
        Assert.True(data.SequenceEqual(dstFs.ReadFile("INST.BIN")));
    }

    [Fact]
    public void Transfer_NoAdapterRegistered_ThrowsInvalidOperationException()
    {
        var srcFs = CreateFormattedXDos("ORCH_NOADAPT_SRC");
        var dstFs = CreateFormattedXDos("ORCH_NOADAPT_DST");

        srcFs.WriteFile("ANY.BIN", new byte[64], srcFs.CreateDefaultAttributes(false));

        var orchestrator = new FileSystemTransferOrchestrator();

        Assert.Throws<InvalidOperationException>(()
            => orchestrator.Transfer(srcFs, dstFs, "ANY.BIN", "ANY.BIN"));
    }

    [Fact]
    public void TransferAll_InstanceRegistration_CopiesAllEntries()
    {
        var srcFs = CreateFormattedXDos("ORCH_ALL_SRC");
        var dstFs = CreateFormattedXDos("ORCH_ALL_DST");

        byte[] d1 = new byte[100]; d1[0] = 0x01;
        byte[] d2 = new byte[200]; d2[0] = 0x02;
        byte[] d3 = new byte[300]; d3[0] = 0x03;

        srcFs.WriteFile("F1.BIN", d1, srcFs.CreateDefaultAttributes(false));
        srcFs.WriteFile("F2.BIN", d2, srcFs.CreateDefaultAttributes(false));
        srcFs.WriteFile("F3.BIN", d3, srcFs.CreateDefaultAttributes(false));

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.TransferAll(srcFs, dstFs);

        Assert.True(dstFs.FileExists("F1.BIN"));
        Assert.True(dstFs.FileExists("F2.BIN"));
        Assert.True(dstFs.FileExists("F3.BIN"));

        Assert.True(d1.SequenceEqual(dstFs.ReadFile("F1.BIN")));
        Assert.True(d2.SequenceEqual(dstFs.ReadFile("F2.BIN")));
        Assert.True(d3.SequenceEqual(dstFs.ReadFile("F3.BIN")));
    }

    [Fact]
    public void Transfer_InstanceRegistration_PreservesRawType()
    {
        var srcFs = CreateFormattedXDos("ORCH_TYPE_SRC");
        var dstFs = CreateFormattedXDos("ORCH_TYPE_DST");

        byte[] data = new byte[256];
        srcFs.WriteFileInternal("PROG.CMD", data, srcFs.CreateDefaultAttributes(false),
            loadAddress: 0xE000, executionAddress: 0xE100,
            forcedRawType: (ushort)XDosFileType.Cmd);

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.Transfer(srcFs, dstFs, "PROG.CMD", "PROG.CMD");

        var entry = dstFs.GetFilesWithMetadata().First(e => e.FileName == "PROG.CMD");
        Assert.Equal((ushort)XDosFileType.Cmd, entry.RawFileType);
        Assert.Equal((ushort)0xE000, entry.StartAddress);
        Assert.Equal((ushort)0xE100, entry.ExecAddressOrSizeHigh);
    }

    [Fact]
    public void Transfer_AscExport_OmitsExecutionAddressAndAdapterIdMatches()
    {
        var srcFs = CreateFormattedXDos("ORCH_ASC_SRC");
        var dstFs = CreateFormattedXDos("ORCH_ASC_DST");

        byte[] ascData = System.Text.Encoding.ASCII.GetBytes("HELLO XDOS\r\n");
        srcFs.WriteFile("TEXT.TXT", ascData, srcFs.CreateDefaultAttributes(true));

        var srcAdapter = new XDosTransferAdapter(srcFs);
        Assert.Equal("X-DOS", srcAdapter.FileSystemId);

        var srcEntry = srcFs.GetFiles().First(e => e.FileName == "TEXT.TXT");
        var envelope = srcAdapter.Export(srcEntry);
        Assert.Null(envelope.ExecutionAddress);

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, srcAdapter);
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.Transfer(srcFs, dstFs, "TEXT.TXT", "TEXT.TXT");

        var dstEntry = dstFs.GetFilesWithMetadata().First(e => e.FileName == "TEXT.TXT");
        Assert.Equal((ushort)XDosFileType.Asc, dstEntry.RawFileType);
    }

    [Fact]
    public void TypeLevel_Resolve_WorksWhenFileSystemIdDiffersFromDisplayName()
    {
        var srcFs = CreateFormattedXDos("ORCH_MISMATCH_SRC");
        var dstFs = CreateFormattedXDos("ORCH_MISMATCH_DST");

        byte[] data = new byte[64]; data[0] = 0xBB;
        srcFs.WriteFile("MISMATCH.BIN", data, srcFs.CreateDefaultAttributes(false));

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(new MismatchedIdAdapter(dstFs));

        orchestrator.Transfer(srcFs, dstFs, "MISMATCH.BIN", "MISMATCH.BIN");

        Assert.True(dstFs.FileExists("MISMATCH.BIN"));
        Assert.True(data.SequenceEqual(dstFs.ReadFile("MISMATCH.BIN")));
    }

    [Fact]
    public void TypeLevel_InstanceRegistration_TakesPrecedenceOverTypeLevel()
    {
        var srcFs = CreateFormattedXDos("ORCH_PREC_SRC");
        var dstFs = CreateFormattedXDos("ORCH_PREC_DST");

        byte[] data = new byte[64]; data[0] = 0xCC;
        srcFs.WriteFile("PREC.BIN", data, srcFs.CreateDefaultAttributes(false));

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(new ThrowingAdapter());
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(dstFs, new XDosTransferAdapter(dstFs));

        orchestrator.Transfer(srcFs, dstFs, "PREC.BIN", "PREC.BIN");

        Assert.True(dstFs.FileExists("PREC.BIN"));
        Assert.True(data.SequenceEqual(dstFs.ReadFile("PREC.BIN")));
    }

    [Fact]
    public void TypeLevel_Resolve_DoesNotCallGetFileSystemInfo()
    {
        var srcFs = CreateFormattedXDos("ORCH_NOFS_SRC");
        var dstFs = CreateFormattedXDos("ORCH_NOFS_DST");

        byte[] data = new byte[32]; data[0] = 0xEE;
        srcFs.WriteFile("NOFS.BIN", data, srcFs.CreateDefaultAttributes(false));

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(new MismatchedIdAdapter(dstFs));

        var ex = Record.Exception(() => orchestrator.Transfer(srcFs, dstFs, "NOFS.BIN", "NOFS.BIN"));
        Assert.Null(ex);
        Assert.True(dstFs.FileExists("NOFS.BIN"));
    }

    [Fact]
    public void Register_TypeLevel_ResolvesWhenNoInstanceRegistered()
    {
        var srcFs = CreateFormattedXDos("ORCH_TYPELV_SRC");
        var dstFs = CreateFormattedXDos("ORCH_TYPELV_DST");

        byte[] data = new byte[128]; data[0] = 0xDD;
        srcFs.WriteFile("TYPE_RES.BIN", data, srcFs.CreateDefaultAttributes(false));

        var orchestrator = new FileSystemTransferOrchestrator();
        orchestrator.Register(srcFs, new XDosTransferAdapter(srcFs));
        orchestrator.Register(new XDosTransferAdapter(dstFs));

        orchestrator.Transfer(srcFs, dstFs, "TYPE_RES.BIN", "TYPE_RES.BIN");

        Assert.True(dstFs.FileExists("TYPE_RES.BIN"));
        Assert.True(data.SequenceEqual(dstFs.ReadFile("TYPE_RES.BIN")));
    }

    private sealed class MismatchedIdAdapter : IFileSystemTransferAdapter
    {
        private readonly XDosTransferAdapter _inner;
        public MismatchedIdAdapter(XDosFileSystem fs) => _inner = new XDosTransferAdapter(fs);
        public string FileSystemId => "NOT-X-DOS";
        public bool Supports(IFileSystem fs) => fs is XDosFileSystem;
        public TransferFileEnvelope Export(FileEntry entry) => _inner.Export(entry);
        public void Import(TransferFileEnvelope envelope, string destFileName) => _inner.Import(envelope, destFileName);
    }

    private sealed class ThrowingAdapter : IFileSystemTransferAdapter
    {
        public string FileSystemId => "THROWING";
        public bool Supports(IFileSystem fs) => fs is XDosFileSystem;
        public TransferFileEnvelope Export(FileEntry entry) => throw new InvalidOperationException("Type-level adapter used instead of instance.");
        public void Import(TransferFileEnvelope envelope, string destFileName) => throw new InvalidOperationException("Type-level adapter used instead of instance.");
    }
}
