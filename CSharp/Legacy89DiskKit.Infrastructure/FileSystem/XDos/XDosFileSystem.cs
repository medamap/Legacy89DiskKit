using System.Text;
using System.Buffers.Binary;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;
using FileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos;

public class XDosFileSystem : IFileSystem
{
    private readonly IDiskContainer    _container;
    private XDosFatReader              _fat = null!;
    private XDosFamReader              _fam = null!;
    private readonly XDosDirParser     _dirParser;
    private XDosClusterReader          _recordReader = null!;
    private XDosFatWriter              _fatWriter = null!;
    private XDosFamWriter              _famWriter = null!;
    private XDosDirWriter              _dirWriter = null!;
    private readonly XDosMediaGeometry _geometry;
    private IReadOnlyList<XDosDirectoryEntry>? _cachedDirectory;

    public XDosFileSystem(IDiskContainer container)
    {
        _container    = container;
        _geometry     = XDosMediaGeometry.FromDiskType(container.DiskType);
        _dirParser    = new XDosDirParser();
        InitializeHelpers();
    }

    private void InitializeHelpers()
    {
        _fat          = new XDosFatReader(_container, _geometry);
        _fam          = new XDosFamReader(_container);
        _recordReader = new XDosClusterReader(_container, _fam);
        _fatWriter    = new XDosFatWriter(_container, _geometry);
        _famWriter    = new XDosFamWriter(_container);
        _dirWriter    = new XDosDirWriter(_container);
    }

    public FileSystemCapabilities Capabilities =>
        FileSystemCapabilities.SupportsBootArea |
        FileSystemCapabilities.SupportsAttributes |
        FileSystemCapabilities.FixedFileNameLength;

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        int free    = _fat.CountFreeRecords();
        int used    = _fat.CountUsedRecords();
        int recSize = _geometry.DataSectorSize;
        return new DiskFileSystemInfo(
            "X-DOS",
            (long)(free + used) * recSize,
            (long)free * recSize,
            recSize, 0, "X1", "X1", 16, 0);
    }

    public IEnumerable<FileEntry> GetFiles() => GetDirectory().Select(ToFileEntry);
    public IReadOnlyList<XDosDirectoryEntry> GetFilesWithMetadata() => GetDirectory();

    public (int Sector, int Offset)? FindDirectorySlot(byte[] rawName, ushort rawType)
    {
        int maxSector = _container.DiskType == DiskType.TwoHD ? 16 : 10;
        var normalized = NormalizeRawName(rawName);

        for (int sectorNumber = 2; sectorNumber <= maxSector; sectorNumber++)
        {
            if (!_container.SectorExists(0, 1, sectorNumber))
                continue;

            var sector = _container.ReadSector(0, 1, sectorNumber);
            for (int offset = 0; offset + 32 <= sector.Length; offset += 32)
            {
                ushort candidateType = BinaryPrimitives.ReadUInt16BigEndian(sector.AsSpan(offset));
                if (candidateType != rawType)
                    continue;

                if (sector.AsSpan(offset + 2, 16).SequenceEqual(normalized))
                    return (sectorNumber, offset);
            }
        }

        return null;
    }

    public bool FileExists(string fileName)   => FileExistsRaw(NormalizeRawName(Encoding.Latin1.GetBytes(fileName)));
    public bool FileExistsRaw(byte[] rawName) => GetDirectory().Any(e => e.RawFileName.SequenceEqual(NormalizeRawName(rawName)));
    public bool FileExistsExact(byte[] rawName, ushort rawType) =>
        GetDirectory().Any(e => e.RawFileName.SequenceEqual(NormalizeRawName(rawName)) && e.RawFileType == rawType);

    public byte[] ReadFile(string fileName) => ReadFileRaw(Encoding.Latin1.GetBytes(fileName));
    public byte[] ReadFileRaw(byte[] rawName)
    {
        var normalized = NormalizeRawName(rawName);
        var entry = GetDirectory().FirstOrDefault(e => e.RawFileName.SequenceEqual(normalized))
            ?? throw new FileNotFoundException();
        return _recordReader.ReadFile(entry);
    }

    private static byte[] NormalizeRawName(byte[] rawName)
    {
        if (rawName.Length == 16) return rawName;
        var res = new byte[16];
        Array.Fill(res, (byte)0x20);
        Array.Copy(rawName, 0, res, 0, Math.Min(rawName.Length, 16));
        return res;
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes,
        ushort? loadAddress = null, ushort? executionAddress = null)
        => WriteFileInternal(fileName, data, attributes, loadAddress, executionAddress);

    public void WriteFileInternal(
        string fileName, byte[] data, ExtendedFileAttributes attributes,
        ushort? loadAddress = null, ushort? executionAddress = null,
        int? forcedFamTrack = null, byte[]? forcedRawName = null, ushort? forcedRawType = null,
        int? forcedFamSector = null, IReadOnlyList<(int Track, int Sector)>? forcedRecords = null,
        int? forcedDirSector = null, int? forcedDirOffset = null)
    {
        if (_container.IsReadOnly) throw new InvalidOperationException("Read-only.");
        _cachedDirectory = null;

        byte[] rawName = NormalizeRawName(forcedRawName ?? Encoding.Latin1.GetBytes(fileName));
        ushort rawType = forcedRawType ?? (attributes.IsAscii
            ? (ushort)XDosFileType.Asc
            : (ushort)XDosFileType.Bin);

        if (!forcedFamTrack.HasValue && FileExistsExact(rawName, rawType))
            throw new IOException("Exists.");

        List<(int Track, int Sector)> allocated;
        if (forcedRecords != null)
        {
            allocated = forcedRecords.ToList();
            foreach (var (t, s) in allocated) _fatWriter.MarkUsed(t, s);
        }
        else if (forcedFamTrack.HasValue)
        {
            allocated = BuildForcedRecordList(forcedFamTrack.Value, forcedFamSector ?? 1, data.Length);
            foreach (var (t, s) in allocated) _fatWriter.MarkUsed(t, s);
        }
        else
        {
            int dataRec    = Math.Max(1, (int)Math.Ceiling((double)data.Length / _geometry.DataSectorSize));
            int totalNeeded = Math.Max(2, dataRec + 1);
            allocated = _fatWriter.AllocateRecords(totalNeeded);
        }

        var (famTrack, famSector) = allocated[0];
        var dataRecords = allocated.Skip(1).ToList();

        WriteDataRecords(data, dataRecords);

        var famEntries = BuildFamEntries(dataRecords);
        _famWriter.WriteFam(famTrack, famSector, famEntries);

        _fatWriter.Commit();
        Array.Copy(_fatWriter.FatSector, _fat.FatSector, _fat.FatSector.Length);

        var entry = new XDosDirectoryEntry(
            RawFileType:           rawType,
            FileName:              fileName,
            RawFileName:           rawName,
            StartAddress:          loadAddress ?? 0,
            SizeLow:               (ushort)(data.Length & 0xFFFF),
            ExecAddressOrSizeHigh: executionAddress ?? (ushort)((data.Length >> 16) & 0xFFFF),
            TimestampRaw:          0,
            Attribute:             attributes.RawAttributes,
            FamPointer:            new XDosFamPointer((byte)famTrack, (byte)famSector, 0x01));

        if (forcedDirSector.HasValue || forcedDirOffset.HasValue)
        {
            if (!forcedDirSector.HasValue || !forcedDirOffset.HasValue)
                throw new IOException("Directory slot specification is incomplete.");
            _dirWriter.WriteEntry(entry, forcedDirSector.Value, forcedDirOffset.Value);
        }
        else
        {
            _dirWriter.WriteEntry(entry);
        }
        _cachedDirectory = null;
    }

    private List<(int Track, int Sector)> BuildForcedRecordList(int famTrack, int famSector, int dataLength)
    {
        var result = new List<(int, int)> { (famTrack, famSector) };
        int dataRec = Math.Max(1, (int)Math.Ceiling((double)dataLength / _geometry.DataSectorSize));
        int t = famTrack;
        int s = famSector + 1;
        for (int i = 0; i < dataRec; i++)
        {
            if (s > _geometry.DataSectorsPerTrack) { t++; s = 1; }
            result.Add((t, s));
            s++;
        }
        return result;
    }

    private void WriteDataRecords(byte[] data, IReadOnlyList<(int Track, int Sector)> records)
    {
        int written = 0;
        foreach (var (t, s) in records)
        {
            if (written >= data.Length) break;
            int c    = t / 2;
            int h    = t % 2;
            var buf  = new byte[_geometry.DataSectorSize];
            int take = Math.Min(buf.Length, data.Length - written);
            Array.Copy(data, written, buf, 0, take);
            _container.WriteSector(c, h, s, buf);
            written += take;
        }
    }

    private static List<XDosFamEntry> BuildFamEntries(IReadOnlyList<(int Track, int Sector)> records)
    {
        var entries = new List<XDosFamEntry>();
        if (records.Count == 0) return entries;

        int runTrack  = records[0].Track;
        int runSector = records[0].Sector;
        int runCount  = 1;

        for (int i = 1; i < records.Count; i++)
        {
            var (t, s) = records[i];
            if (t == runTrack && s == runSector + runCount)
                runCount++;
            else
            {
                entries.Add(new XDosFamEntry((byte)runTrack, (byte)runSector, (byte)runCount));
                runTrack = t; runSector = s; runCount = 1;
            }
        }
        entries.Add(new XDosFamEntry((byte)runTrack, (byte)runSector, (byte)runCount));
        return entries;
    }

    public void DeleteFile(string fileName)                              => throw new NotSupportedException();
    public void RenameFile(string old, string @new)                      => throw new NotSupportedException();
    public void CopyFile(string src, string dst)                         => throw new NotSupportedException();
    public void UpdateAttributes(string fn, ExtendedFileAttributes attr) => throw new NotSupportedException();

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii)
        => new ExtendedFileAttributes(FileAttributes.None, 0x00, isAscii, "X-DOS");

    public void Format()
    {
        if (_container.IsReadOnly) throw new InvalidOperationException();
        if (_container is IGeometryRebuildableDiskContainer rebuildable)
        {
            rebuildable.RebuildGeometry((c, h) => _geometry.GetTrackGeometry(c, h));
            InitializeHelpers();
        }
        var now = DateTime.Now;
        var vol = new byte[256];
        vol[0] = 0x01;
        Array.Copy(Encoding.ASCII.GetBytes("X-DOS        SYS"), 0, vol, 1, 16);
        vol[24] = 0x88;
        vol[25] = ToBcd(now.Year % 100);
        vol[26] = ToBcd(now.Month);
        vol[27] = ToBcd(now.Day);
        _container.WriteSector(0, 0, 1, vol);
        _fatWriter.ClearAll();
        _fatWriter.Commit();
        _fat = new XDosFatReader(_container, _geometry);
        _famWriter.ClearAll();
        _dirWriter.ClearAll();
        _cachedDirectory = null;
    }

    private static byte ToBcd(int v) => (byte)(((v / 10) << 4) | (v % 10));

    public byte[] ReadBootArea()
    {
        var res = new List<byte>();
        for (int r = 1; r <= 16; r++) res.AddRange(_container.ReadSector(0, 0, r));
        return res.ToArray();
    }

    public void WriteBootArea(byte[] data)
    {
        int off = 0;
        for (int r = 1; r <= 16 && off < data.Length; r++)
        {
            var b = new byte[256];
            int t = Math.Min(256, data.Length - off);
            Array.Copy(data, off, b, 0, t);
            _container.WriteSector(0, 0, r, b);
            off += 256;
        }
    }

    public void Dispose() { }

    private IReadOnlyList<XDosDirectoryEntry> GetDirectory()
        => _cachedDirectory ??= _dirParser.Parse(_container);

    private static FileEntry ToFileEntry(XDosDirectoryEntry e) => new FileEntry(
        FileName:         e.FileName,
        Extension:        string.Empty,
        Size:             e.FileSize,
        CreatedAt:        null,
        LastModifiedAt:   null,
        Attributes:       new ExtendedFileAttributes(FileAttributes.None, e.Attribute, false, "X-DOS"),
        StartCluster:     e.FamPointer.Track,
        LoadAddress:      e.StartAddress,
        EndAddress:       (ushort)(e.StartAddress + e.SizeLow),
        ExecutionAddress: e.ExecAddressOrSizeHigh,
        RawFileName:      e.RawFileName);

    public static (int sectors, ushort size, byte density)? XDosTrackGeometry(int c, int h)
        => XDosMediaGeometry.FromDiskType(DiskType.TwoD).GetTrackGeometry(c, h);
}
