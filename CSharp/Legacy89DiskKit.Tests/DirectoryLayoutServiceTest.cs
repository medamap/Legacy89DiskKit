using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Interface.Layout;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Xunit;
using DomainFileAttributes = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;

namespace Legacy89DiskKit.Tests;

public class DirectoryLayoutServiceTest
{
    [Fact]
    public void InsertLabelBefore_AddsVirtualLabelBeforeTarget()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("A.TXT", 0),
            CreateFileItem("B.TXT", 1)
        });
        var service = new DirectoryLayoutService();

        var layout = service.InsertLabelBefore(fileSystem, "HEADER.DOC", "B.TXT");

        Assert.Equal(3, layout.Items.Count);
        Assert.Equal(DirectoryLayoutItemKind.VirtualLabel, layout.Items[1].Kind);
        Assert.Equal("HEADER.DOC", layout.Items[1].DisplayName);
        Assert.Equal("B.TXT", layout.Items[2].DisplayName);
    }

    [Fact]
    public void SortEntries_PreservesVirtualLabelPositions()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("B.TXT", 0),
            CreateLabelItem("SECTION", string.Empty, 1),
            CreateFileItem("A.TXT", 2)
        });
        var service = new DirectoryLayoutService();

        var layout = service.SortEntries(fileSystem, DirectorySortBy.Name);

        Assert.Equal("A.TXT", layout.Items[0].DisplayName);
        Assert.Equal("SECTION", layout.Items[1].DisplayName);
        Assert.Equal(DirectoryLayoutItemKind.VirtualLabel, layout.Items[1].Kind);
        Assert.Equal("B.TXT", layout.Items[2].DisplayName);
    }

    [Fact]
    public void MoveEntryBefore_ReordersEntries()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("A.TXT", 0),
            CreateFileItem("B.TXT", 1),
            CreateFileItem("C.TXT", 2)
        });
        var service = new DirectoryLayoutService();

        var layout = service.MoveEntryBefore(fileSystem, "C.TXT", "A.TXT");

        Assert.Equal(new[] { "C.TXT", "A.TXT", "B.TXT" }, layout.Items.Select(item => item.DisplayName).ToArray());
    }

    [Fact]
    public void ExportPlan_AndApplyPlan_RoundTripsLayout()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("A.TXT", 0),
            CreateFileItem("B.TXT", 1)
        });
        var service = new DirectoryLayoutService();

        var plan = service.ExportPlan(fileSystem);
        var result = service.ApplyPlan(fileSystem, plan);

        Assert.True(result.IsValid, string.Join(" | ", result.Messages.Select(message => $"{message.Severity}:{message.Message}")));
        Assert.Equal(new[] { "A.TXT", "B.TXT" }, fileSystem.ReadDirectoryLayout().Items.Select(item => item.DisplayName).ToArray());
    }

    [Fact]
    public void ApplyPlan_RenamesAndReordersEntries()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("A.TXT", 0),
            CreateFileItem("B.TXT", 1)
        });
        var service = new DirectoryLayoutService();
        var layout = fileSystem.ReadDirectoryLayout();
        var lines = new[]
        {
            $"{DirectoryLayoutService.CreateStableId(layout.Items[1].Id)} B2.TXT",
            $"{DirectoryLayoutService.CreateStableId(layout.Items[0].Id)} A.TXT"
        };

        var result = service.ApplyPlan(fileSystem, string.Join(Environment.NewLine, lines));

        Assert.True(result.IsValid, string.Join(" | ", result.Messages.Select(message => $"{message.Severity}:{message.Message}")));
        Assert.Equal(new[] { "B2.TXT", "A.TXT" }, fileSystem.ReadDirectoryLayout().Items.Select(item => item.DisplayName).ToArray());
    }

    [Fact]
    public void ValidatePlan_WarnsForOmittedEntries()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("A.TXT", 0),
            CreateFileItem("B.TXT", 1)
        });
        var service = new DirectoryLayoutService();
        var layout = fileSystem.ReadDirectoryLayout();
        var plan = $"{DirectoryLayoutService.CreateStableId(layout.Items[0].Id)} A.TXT";

        var result = service.ValidatePlan(fileSystem, plan);

        if (!result.IsValid)
        {
            throw new InvalidOperationException(string.Join(" | ", result.Messages.Select(message => $"{message.Severity}:{message.Message}")));
        }
        Assert.Equal(1, result.WarningCount);
        Assert.NotNull(result.ProposedLayout);
        Assert.Equal(new[] { "A.TXT", "B.TXT" }, result.ProposedLayout!.Items.Select(item => item.DisplayName).ToArray());
    }

    [Fact]
    public void ValidatePlan_CreatesSingleLabelEntryFromCommentLine()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("A.TXT", 0)
        });
        var service = new DirectoryLayoutService();
        var layout = fileSystem.ReadDirectoryLayout();
        var plan = string.Join(Environment.NewLine,
            "# ｺｺｶﾗｼﾀﾊｺﾝｶｲｼｭｳﾛｸｼﾀｱﾌﾟﾘｹｰｼｮﾝﾉｿｰｽｺｰﾄﾞｶﾞﾊｲｯﾃｲﾏｽ",
            $"{DirectoryLayoutService.CreateStableId(layout.Items[0].Id)} A.TXT");

        var result = service.ValidatePlan(fileSystem, plan);

        if (!result.IsValid)
        {
            throw new InvalidOperationException(string.Join(" | ", result.Messages.Select(message => $"{message.Severity}:{message.Message}")));
        }
        Assert.NotNull(result.ProposedLayout);
        Assert.Equal(2, result.ProposedLayout!.Items.Count);
        Assert.Equal(DirectoryLayoutItemKind.VirtualLabel, result.ProposedLayout.Items[0].Kind);
        Assert.Equal("ｺｺｶﾗｼﾀﾊｺﾝｶｲｼｭ", result.ProposedLayout.Items[0].VirtualLabel!.FileName);
        Assert.Equal(string.Empty, result.ProposedLayout.Items[0].VirtualLabel!.Extension);
    }

    [Fact]
    public void ExportPlan_UsesLabelNameAndExtension()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateLabelItem("-------------", "---", 0),
            CreateFileItem("A.TXT", 1)
        });
        var service = new DirectoryLayoutService();

        var plan = service.ExportPlan(fileSystem);

        Assert.Contains("# -------------.---", plan);
    }

    [Fact]
    public void ValidatePlan_ParsesOnlyFirstDotForLabel()
    {
        var fileSystem = new FakeLayoutFileSystem(new[]
        {
            CreateFileItem("A.TXT", 0)
        });
        var service = new DirectoryLayoutService();
        var layout = fileSystem.ReadDirectoryLayout();
        var plan = string.Join(Environment.NewLine,
            "# NAME.EXT.EXTRA",
            $"{DirectoryLayoutService.CreateStableId(layout.Items[0].Id)} A.TXT");

        var result = service.ValidatePlan(fileSystem, plan);

        if (!result.IsValid)
        {
            throw new InvalidOperationException(string.Join(" | ", result.Messages.Select(message => $"{message.Severity}:{message.Message}")));
        }

        var label = result.ProposedLayout!.Items[0].VirtualLabel!;
        Assert.Equal("NAME", label.FileName);
        Assert.Equal("EXT", label.Extension);
    }

    private static DirectoryLayoutItem CreateFileItem(string fullName, int order)
    {
        var parts = fullName.Split('.', 2);
        var entry = new FileEntry(parts[0], parts.Length > 1 ? parts[1] : string.Empty, 1, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.None, 0, false, ""), order + 1);
        return new DirectoryLayoutItem(fullName, order, DirectoryLayoutItemKind.FileEntry, fullName, entry, null);
    }

    private static DirectoryLayoutItem CreateLabelItem(string name, string extension, int order)
    {
        return new DirectoryLayoutItem(
            $"label:{order}",
            order,
            DirectoryLayoutItemKind.VirtualLabel,
            string.IsNullOrEmpty(extension) ? name : $"{name}.{extension}",
            null,
            new VirtualDirectoryLabelEntry(name, extension, 0x44, 0x01, 0, 0xFFFF, 0xFFFF, 0xFFFF, 0x7FFF)
        );
    }

    private sealed class FakeLayoutFileSystem : IFileSystem, IDirectoryLayoutProvider
    {
        private DirectoryEntryLayout _layout;

        public FakeLayoutFileSystem(IReadOnlyList<DirectoryLayoutItem> items)
        {
            _layout = new DirectoryEntryLayout("Hu-BASIC", items);
        }

        public DirectoryEntryLayout ReadDirectoryLayout() => _layout;

        public void ApplyDirectoryLayout(DirectoryEntryLayout layout) => _layout = layout;

        public DiskFileSystemInfo GetFileSystemInfo() => new("Hu-BASIC", 0, 0, 0, 0, "X1", "X1");
        public FileSystemCapabilities Capabilities => FileSystemCapabilities.None;
        public IEnumerable<FileEntry> GetFiles() => _layout.Items.Where(item => item.Entry != null).Select(item => item.Entry!);
        public bool FileExists(string fileName) => false;
        public byte[] ReadFile(string fileName) => Array.Empty<byte>();
        public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null) { }
        public void DeleteFile(string fileName) { }
        public void RenameFile(string oldName, string newName) { }
        public void CopyFile(string sourceName, string targetName) { }
        public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes) { }
        public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii) => new(DomainFileAttributes.None, 0, isAscii, "");
        public void Format() { }
        public byte[] ReadBootArea() => Array.Empty<byte>();
        public void WriteBootArea(byte[] data) { }
        public void Dispose() { }
    }
}
