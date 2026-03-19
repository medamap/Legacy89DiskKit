using System.Text;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;
using FileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos;

public class XDosFileSystem : IFileSystem
{
    private readonly IDiskContainer    _container;
    private readonly XDosFatReader     _fat;
    private readonly XDosFamReader     _fam;
    private readonly XDosDirParser     _dirParser;
    private readonly XDosClusterReader _clusterReader;
    private readonly XDosFatWriter     _fatWriter;
    private readonly XDosFamWriter     _famWriter;
    private readonly XDosDirWriter     _dirWriter;
    private IReadOnlyList<XDosDirectoryEntry>? _cachedDirectory;

    public XDosFileSystem(IDiskContainer container)
    {
        _container = container;
        _fat = new XDosFatReader(container);
        _fam = new XDosFamReader(container);
        _dirParser = new XDosDirParser();
        _clusterReader = new XDosClusterReader(container, _fam);
        _fatWriter = new XDosFatWriter(container);
        _famWriter = new XDosFamWriter(container);
        _dirWriter = new XDosDirWriter(container);
    }

    public FileSystemCapabilities Capabilities => FileSystemCapabilities.SupportsBootArea | FileSystemCapabilities.SupportsAttributes | FileSystemCapabilities.FixedFileNameLength;

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        int free = _fat.CountFreeClusters();
        int used = _fat.CountUsedClusters();
        return new DiskFileSystemInfo("X-DOS", (long)(free + used) * 5120, (long)free * 5120, 5120, 0, "X1", "X1");
    }

    public IEnumerable<FileEntry> GetFiles() => GetDirectory().Select(ToFileEntry);
    public IReadOnlyList<XDosDirectoryEntry> GetFilesWithMetadata() => GetDirectory();
    public bool FileExists(string fileName) => FileExistsRaw(NormalizeRawName(Encoding.ASCII.GetBytes(fileName)));
    public bool FileExistsRaw(byte[] rawName) => GetDirectory().Any(e => e.RawFileName.SequenceEqual(NormalizeRawName(rawName)));
    public bool FileExistsExact(byte[] rawName, byte rawType) => GetDirectory().Any(e => e.RawFileName.SequenceEqual(NormalizeRawName(rawName)) && e.RawFileType == rawType);
    public byte[] ReadFile(string fileName) => ReadFileRaw(Encoding.ASCII.GetBytes(fileName));
    public byte[] ReadFileRaw(byte[] rawName)
    {
        var normalized = NormalizeRawName(rawName);
        var entry = GetDirectory().FirstOrDefault(e => e.RawFileName.SequenceEqual(normalized)) ?? throw new FileNotFoundException();
        return _clusterReader.ReadFile(entry);
    }

    private static byte[] NormalizeRawName(byte[] rawName)
    {
        if (rawName.Length == 16) return rawName;
        var res = new byte[16]; Array.Fill(res, (byte)0x20); Array.Copy(rawName, 0, res, 0, Math.Min(rawName.Length, 16));
        return res;
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
        => WriteFileInternal(fileName, data, attributes, loadAddress, executionAddress);

    public void WriteFileInternal(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null, int? forcedStartTrack = null, byte[]? forcedRawName = null, byte? forcedRawType = null, int? forcedStartSectorR = null, IReadOnlyList<byte>? forcedClusterChain = null)
    {
        if (_container.IsReadOnly) throw new InvalidOperationException("Read-only.");
        _cachedDirectory = null;
        byte[] rawName = NormalizeRawName(forcedRawName ?? Encoding.ASCII.GetBytes(fileName));
        byte rawType = forcedRawType ?? (attributes.RawAttributes != 0 ? attributes.RawAttributes : (byte)XDosFileType.Binary);
        if (!forcedStartTrack.HasValue && FileExistsExact(rawName, rawType)) throw new IOException("Exists.");

        int startTrack;
        if (forcedStartTrack.HasValue) startTrack = forcedStartTrack.Value;
        else
        {
            var tracks = _fatWriter.AllocateClusters((data.Length + 511) / 512);
            startTrack = tracks[0];
            _famWriter.UpdateChain(tracks);
            _fatWriter.Commit(); _famWriter.Commit();
            Array.Copy(_fatWriter.Fat, _fat.Fat, _fat.Fat.Length);
            Array.Copy(_famWriter.Fam, _fam.Fam, _fam.Fam.Length);
        }

        int written = 0;
        int currentTrack = startTrack;
        int startSectorR = forcedStartSectorR ?? ((currentTrack == 1 || currentTrack == 2) ? 2 : 1);

        while (written < data.Length)
        {
            int c = currentTrack / 2, h = currentTrack % 2;
            int trackStartR = (currentTrack == 1 || currentTrack == 2) ? 2 : 1;
            int rStart = (currentTrack == startTrack) ? Math.Max(trackStartR, startSectorR) : trackStartR;
            int maxR = (c == 0 && h == 0) ? 16 : 10;
            
            for (int r = rStart; r <= maxR && written < data.Length; r++)
            {
                int sz = (c == 0 && h == 0) ? 256 : 512;
                byte[] buf = new byte[sz];
                int take = Math.Min(sz, data.Length - written);
                Array.Copy(data, written, buf, 0, take);
                _container.WriteSector(c, h, r, buf);
                written += take;
            }
            
            if (written < data.Length)
            {
                if (forcedClusterChain != null)
                {
                    int idx = forcedClusterChain.ToList().IndexOf((byte)currentTrack);
                    if (idx >= 0 && idx + 1 < forcedClusterChain.Count)
                        currentTrack = forcedClusterChain[idx + 1];
                    else
                        throw new IOException($"Forced cluster chain exhausted at cluster {currentTrack}.");
                }
                else if (forcedStartTrack.HasValue)
                    currentTrack++;
                else
                {
                    var ch = _fam.GetChain((byte)startTrack);
                    int idx = ch.ToList().IndexOf((byte)currentTrack);
                    if (idx >= 0 && idx + 1 < ch.Count) currentTrack = ch[idx+1];
                    else throw new IOException("Chain error.");
                }
            }
        }
        
        var entry = new XDosDirectoryEntry(
            RawFileType: rawType, Attribute: attributes.RawAttributes,
            FileName: fileName, RawFileName: rawName,
            LoadAddress: loadAddress ?? 0,
            ByteSize: (ushort)data.Length,      // 修正: 実バイト数をそのまま格納
            ExecutionAddress: executionAddress ?? 0,
            DatePacked: 0,                       // 書き込み時はタイムスタンプ未実装 → 0
            TimePacked: 0,
            Flags: 0x80,
            FirstCluster: (byte)startTrack, FirstSectorR: (byte)startSectorR, AlwaysOne: 0x01);
        _dirWriter.WriteEntry(entry);
        _cachedDirectory = null;
    }

    public void DeleteFile(string fileName) => throw new NotSupportedException();
    public void RenameFile(string old, string @new) => throw new NotSupportedException();
    public void CopyFile(string src, string dst) => throw new NotSupportedException();
    public void UpdateAttributes(string fn, ExtendedFileAttributes attr) => throw new NotSupportedException();
    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii) => new ExtendedFileAttributes(FileAttributes.None, 0x00, isAscii, "X-DOS");

    public void Format()
    {
        if (_container.IsReadOnly) throw new InvalidOperationException();
        var now = DateTime.Now;
        var vol = new byte[256]; vol[0] = 0x01;
        Array.Copy(Encoding.ASCII.GetBytes("X-DOS        SYS"), 0, vol, 1, 16);
        vol[24] = 0x88; vol[25] = ToBcd(now.Year % 100); vol[26] = ToBcd(now.Month); vol[27] = ToBcd(now.Day);
        _container.WriteSector(0, 0, 1, vol);
        _fatWriter.ClearAll(); _fatWriter.Commit(); Array.Copy(_fatWriter.Fat, _fat.Fat, _fat.Fat.Length);
        _famWriter.ClearAll(); _famWriter.Commit(); Array.Copy(_famWriter.Fam, _fam.Fam, _fam.Fam.Length);
        _dirWriter.ClearAll(); _cachedDirectory = null;
    }
    private static byte ToBcd(int v) => (byte)(((v / 10) << 4) | (v % 10));
    public byte[] ReadBootArea() { var res = new List<byte>(); for (int r = 1; r <= 16; r++) res.AddRange(_container.ReadSector(0, 0, r)); return res.ToArray(); }
    public void WriteBootArea(byte[] data) { int off = 0; for (int r = 1; r <= 16 && off < data.Length; r++) { byte[] b = new byte[256]; int t = Math.Min(256, data.Length - off); Array.Copy(data, off, b, 0, t); _container.WriteSector(0, 0, r, b); off += 256; } }
    public void Dispose() { }
    private IReadOnlyList<XDosDirectoryEntry> GetDirectory() => _cachedDirectory ??= _dirParser.Parse(_container);
    private static XDosVolumeRecord ReadVolumeRecord(IDiskContainer c) { var s = c.ReadSector(0, 0, 1); return new XDosVolumeRecord(Encoding.ASCII.GetString(s, 1, 16).TrimEnd(), s[24], s[25], s[26], s[27]); }
    private static FileEntry ToFileEntry(XDosDirectoryEntry e) => new FileEntry(
        FileName: e.FileName,
        Extension: string.Empty,
        Size: e.FileSize,
        CreatedAt: null,
        LastModifiedAt: null,
        Attributes: new ExtendedFileAttributes(FileAttributes.None, e.Attribute, false, "X-DOS"),
        StartCluster: e.FirstCluster,
        LoadAddress: e.LoadAddress,
        EndAddress: (ushort)(e.LoadAddress + e.ByteSize),  // 修正: ByteSize から再計算
        ExecutionAddress: e.ExecutionAddress,
        RawFileName: e.RawFileName);
    private const int SectorsPerTrack = 10;

    public static (int sectors, ushort size, byte density)? XDosTrackGeometry(int c, int h)
        => (c == 0 && h == 0)
            ? (16, (ushort)256, (byte)0x00)
            : (10, (ushort)512, (byte)0x00);
}
