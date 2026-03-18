# X-DOS Filesystem C# Implementation Spec

## Scope

This document specifies the concrete C# implementation for X-DOS filesystem support
(Roadmap phases XD-01 through XD-05). It is the primary reference for implementation.

The technical filesystem analysis is in `X-DOS_Filesystem_Analysis.md`.
The roadmap phases are in `Roadmap_V2.md` under "X-DOS Filesystem Support".

Implementation order: **read-only first** (XD-01 through XD-03 + XD-05 read path),
then write support (XD-04) after the cluster addressing formula is confirmed.

---

## File Layout to Create

### Domain layer

```
csharp/Legacy89DiskKit.Domain/FileSystem/Model/XDos/
  XDosFileType.cs
  XDosVolumeRecord.cs
  XDosDirectoryEntry.cs
```

Namespace: `Legacy89DiskKit.Domain.FileSystem.Model.XDos`

### Infrastructure layer

```
csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/
  Reader/
    XDosFatReader.cs
    XDosFamReader.cs
    XDosDirParser.cs
    XDosClusterReader.cs
  XDosFileSystem.cs
  Provider/
    XDosFileSystemProvider.cs
```

Namespace: `Legacy89DiskKit.Infrastructure.FileSystem.XDos`
Namespace (provider): `Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider`

### Existing files to modify

```
csharp/Legacy89DiskKit.Application/Legacy89DiskKitApplication.cs
csharp/Legacy89DiskKit.Application/FileSystem/ExplicitFileSystemResolver.cs
csharp/Legacy89DiskKit.Cli/Program.cs
```

### Test

```
csharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs
```

---

## Domain Layer Detail

### XDosFileType.cs

```csharp
public enum XDosFileType : byte
{
    SubProgram  = 0x01,
    BasicText   = 0x02,
    Binary      = 0x03,
    Data        = 0x04,
    Overlay     = 0x05,
    Script      = 0x06,
    System      = 0x07,
}
```

### XDosVolumeRecord.cs

Parsed from Track 0, R=1 (256 bytes). Sector is read with N=1 (256 bytes).

```csharp
public record XDosVolumeRecord(
    string DiskLabel,       // bytes [1..16], ASCII, space-trimmed (16 chars)
    byte FormatType,        // byte [24]; 0x88 = Sharp X1 2D
    byte YearBcd,           // byte [25]
    byte MonthBcd,          // byte [26]
    byte DayBcd             // byte [27]
);
```

Parse rule: `DiskLabel = Encoding.ASCII.GetString(sector[1..17]).TrimEnd(' ')`.

### XDosDirectoryEntry.cs

Parsed from 32-byte slices of Track 1, R=2–10.

```csharp
public record XDosDirectoryEntry(
    byte        RawFileType,        // [0]
    byte        Attribute,          // [1]
    string      FileName,           // [2..17], 16 bytes, ASCII, TrimEnd(' ')
    byte[]      RawFileName,        // [2..17], raw 16 bytes (for Shift-JIS filenames)
    ushort      LoadAddress,        // [20..21] LE
    ushort      EndAddress,         // [22..23] LE
    ushort      ExecutionAddress,   // [24..25] LE
    byte        Flags,              // [28]
    byte        FirstCluster,       // [29] FAM chain head (provisional)
    byte        FirstSectorR,       // [30] starting R in first cluster (provisional)
    byte        AlwaysOne           // [31]
)
{
    public bool IsEmpty   => RawFileType == 0x00 || RawFileType == 0xFF || RawFileType == 0xD5;
    public XDosFileType FileType => (XDosFileType)(RawFileType & 0x7F);
    public bool IsKnownType => RawFileType >= 0x01 && RawFileType <= 0x07;
    public int FileSize => EndAddress > LoadAddress ? EndAddress - LoadAddress : 0;
}
```

---

## Infrastructure Layer Detail

### XDosFatReader.cs

Reads the flat allocation bitmap from C=0, H=1, R=1 (512 bytes).
`FAT[N] == 0x4A` means cluster N is occupied; `FAT[N] == 0x00` means free.
`FAT[0]` = 0x00 (IPL track reserved), `FAT[1]` = 0x01 (system marker).

```csharp
public class XDosFatReader
{
    private readonly byte[] _fat;

    public XDosFatReader(IDiskContainer container)
    {
        _fat = container.ReadSector(0, 1, 1);   // C=0, H=1, R=1  ← FAT track
    }

    public bool IsClusterFree(int clusterIndex) => _fat[clusterIndex] == 0x00;
    public int CountFreeClusters() => _fat.Count(b => b == 0x00);
    public int CountUsedClusters() => _fat.Count(b => b == 0x4A);
}
```

### XDosFamReader.cs

Reads and traverses the cluster chain table from C=1, H=0, R=1 (512 bytes).
`FAM[N]` = next cluster in the chain for files whose chain passes through cluster N.
`FAM[N] == 0x00` means end of chain.

```csharp
public class XDosFamReader
{
    private readonly byte[] _fam;
    private const int MaxChainLength = 256;

    public XDosFamReader(IDiskContainer container)
    {
        _fam = container.ReadSector(1, 0, 1);   // C=1, H=0, R=1  ← FAM track
    }

    public IReadOnlyList<byte> GetChain(byte firstCluster)
    {
        var chain = new List<byte>();
        byte current = firstCluster;
        int guard = 0;
        while (current != 0x00 && guard++ < MaxChainLength)
        {
            chain.Add(current);
            current = _fam[current];
        }
        return chain;
    }
}
```

### XDosDirParser.cs

Parses 9 directory sectors from C=0, H=1, R=2–10 (2D format).
The directory shares the same physical track (C=0,H=1) as the FAT: FAT=R=1, Dir=R=2–10.

```csharp
public class XDosDirParser
{
    private const int EntrySize = 32;
    private const int FirstDirR = 2;
    private const int LastDirR  = 10;   // 2D: R=2–10; 2DD/2HD: R=2–16

    public IReadOnlyList<XDosDirectoryEntry> Parse(IDiskContainer container)
    {
        var entries = new List<XDosDirectoryEntry>();
        for (int r = FirstDirR; r <= LastDirR; r++)
        {
            var sector = container.ReadSector(0, 1, r);   // C=0, H=1, R=r  ← FAT/Dir track
            for (int offset = 0; offset + EntrySize <= sector.Length; offset += EntrySize)
            {
                var entry = ParseEntry(sector, offset);
                if (!entry.IsEmpty)
                    entries.Add(entry);
            }
        }
        return entries;
    }

    private static XDosDirectoryEntry ParseEntry(byte[] sector, int offset)
    {
        byte rawType = sector[offset + 0];
        byte attr    = sector[offset + 1];
        var rawName  = sector[offset + 2 .. offset + 18];
        string name  = Encoding.ASCII.GetString(rawName).TrimEnd(' ');
        ushort load  = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 20));
        ushort end   = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 22));
        ushort exec  = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 24));
        byte flags   = sector[offset + 28];
        byte cluster = sector[offset + 29];
        byte startR  = sector[offset + 30];
        byte always1 = sector[offset + 31];

        return new XDosDirectoryEntry(rawType, attr, name, rawName,
            load, end, exec, flags, cluster, startR, always1);
    }
}
```

### XDosClusterReader.cs

Reads raw sector bytes for a cluster chain and assembles file payload.

**Physical mapping formula (CONFIRMED by binary analysis of Z80 kernel code)**:
```
cylinder = clusterIndex / 2
head     = clusterIndex % 2
```
This interleaved dual-sided layout is confirmed by:
1. Z80 kernel code at C=1,H=1,R=8 containing `EE 10` (XOR 0x10 = toggle head bit)
2. Directory entries verified against actual D88 sector positions:
   - cluster=2, sector=1 → ReadSector(1, 0, 1) = C=1,H=0,R=1 ✓ (FAM sector = part of kernel binary)
   - cluster=16, sector=1 → ReadSector(8, 0, 1) = C=8,H=0,R=1 ✓
   - cluster=6, sector=8 → ReadSector(3, 0, 8) = C=3,H=0,R=8 ✓

```csharp
public class XDosClusterReader
{
    private const int SectorsPerTrack = 10;
    private const int SectorSize      = 512;

    private readonly IDiskContainer _container;
    private readonly XDosFamReader  _fam;

    public XDosClusterReader(IDiskContainer container, XDosFamReader fam)
    {
        _container = container;
        _fam       = fam;
    }

    public byte[] ReadFile(XDosDirectoryEntry entry)
    {
        var chain = _fam.GetChain(entry.FirstCluster);
        int targetSize = entry.FileSize > 0 ? entry.FileSize : int.MaxValue;
        var result = new List<byte>();

        for (int i = 0; i < chain.Count && result.Count < targetSize; i++)
        {
            byte cluster  = chain[i];
            int startR    = (i == 0) ? entry.FirstSectorR : 1;
            int cylinder  = cluster / 2;   // interleaved dual-sided mapping
            int head      = cluster % 2;

            for (int r = startR; r <= SectorsPerTrack && result.Count < targetSize; r++)
            {
                var sector = _container.ReadSector(cylinder, head, r);
                int take = Math.Min(sector.Length, targetSize - result.Count);
                result.AddRange(sector[..take]);
            }
        }

        return result.ToArray();
    }
}
```

### XDosFileSystem.cs

Implements `IFileSystem`. Write operations throw `NotSupportedException` in the
initial read-only implementation.

```csharp
public class XDosFileSystem : IFileSystem
{
    private readonly IDiskContainer    _container;
    private readonly XDosVolumeRecord  _volumeRecord;
    private readonly XDosFatReader     _fat;
    private readonly XDosFamReader     _fam;
    private readonly XDosDirParser     _dirParser;
    private readonly XDosClusterReader _clusterReader;
    private IReadOnlyList<XDosDirectoryEntry>? _cachedDirectory;

    public XDosFileSystem(IDiskContainer container)
    {
        _container     = container;
        _volumeRecord  = ReadVolumeRecord(container);
        _fat           = new XDosFatReader(container);
        _fam           = new XDosFamReader(container);
        _dirParser     = new XDosDirParser();
        _clusterReader = new XDosClusterReader(container, _fam);
    }

    public FileSystemCapabilities Capabilities =>
        FileSystemCapabilities.Read;   // write added in XD-04

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        int freeCount = _fat.CountFreeClusters();
        int usedCount = _fat.CountUsedClusters();
        const int ClusterSize = SectorsPerTrack * SectorSize;   // provisional
        return new DiskFileSystemInfo(
            _volumeRecord.DiskLabel,
            totalCapacity: (freeCount + usedCount) * ClusterSize,
            freeCapacity:  freeCount * ClusterSize
        );
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        var dir = GetDirectory();
        return dir.Select(ToFileEntry);
    }

    public bool FileExists(string fileName)
        => GetDirectory().Any(e => string.Equals(e.FileName, fileName, StringComparison.Ordinal));

    public byte[] ReadFile(string fileName)
    {
        var entry = GetDirectory().FirstOrDefault(
            e => string.Equals(e.FileName, fileName, StringComparison.Ordinal))
            ?? throw new FileNotFoundException($"File not found: {fileName}");
        return _clusterReader.ReadFile(entry);
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes,
                          ushort? loadAddress = null, ushort? executionAddress = null)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void DeleteFile(string fileName)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void RenameFile(string oldName, string newName)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void CopyFile(string sourceName, string targetName)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii)
        => new ExtendedFileAttributes(FileAttributes.None, 0x00, isAscii, "X-DOS");

    public void Format()
        => throw new NotSupportedException("X-DOS format support is not yet implemented.");

    public byte[] ReadBootArea()
    {
        var result = new List<byte>();
        for (int r = 1; r <= 16; r++)
            result.AddRange(_container.ReadSector(0, 0, r));
        return result.ToArray();
    }

    public void WriteBootArea(byte[] data)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void Dispose() { }

    private IReadOnlyList<XDosDirectoryEntry> GetDirectory()
        => _cachedDirectory ??= _dirParser.Parse(_container);

    private static XDosVolumeRecord ReadVolumeRecord(IDiskContainer container)
    {
        var sector = container.ReadSector(0, 0, 1);
        string label = Encoding.ASCII.GetString(sector[1..17]).TrimEnd(' ');
        return new XDosVolumeRecord(label, sector[24], sector[25], sector[26], sector[27]);
    }

    private static FileEntry ToFileEntry(XDosDirectoryEntry e) =>
        new FileEntry(
            FileName:           e.FileName,
            Extension:          string.Empty,
            Size:               e.FileSize,
            CreatedAt:          null,
            LastModifiedAt:     null,
            Attributes:         new ExtendedFileAttributes(FileAttributes.None, e.Attribute, false, "X-DOS"),
            LoadAddress:        e.LoadAddress,
            EndAddress:         e.EndAddress,
            ExecutionAddress:   e.ExecutionAddress,
            RawFileName:        e.RawFileName
        );

    private const int SectorsPerTrack = 10;
    private const int SectorSize      = 512;
}
```

### XDosFileSystemProvider.cs

```csharp
public class XDosFileSystemProvider : IFileSystemProvider
{
    public string FileSystemName => "X-DOS";

    public bool CanHandle(IDiskContainer container)
    {
        try
        {
            var sector = container.ReadSector(0, 0, 1);
            return sector.Length >= 25 && sector[0] == 0x01 && sector[24] == 0x88;
        }
        catch
        {
            return false;
        }
    }

    public IFileSystem Create(IDiskContainer container)
        => new XDosFileSystem(container);
}
```

---

## Application Layer Changes

### Legacy89DiskKitApplication.cs

Add the provider registration:

```csharp
public static IFileSystemRegistry CreateFileSystemRegistry()
{
    var registry = new FileSystem.FileSystemRegistry();
    registry.Register(new HuBasicFileSystemProvider());
    registry.Register(new N88BasicFileSystemProvider());
    registry.Register(new MsxDosFileSystemProvider());
    registry.Register(new XDosFileSystemProvider());   // ADD THIS LINE
    return registry;
}
```

Add the using:
```csharp
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;
```

### ExplicitFileSystemResolver.cs

Add the `"xdos"` case to the switch expression:

```csharp
return Normalize(fileSystemName) switch
{
    "hubasic" => new HuBasicFileSystem(container),
    "n88basic" => CreateN88Basic(container),
    "msxdos"  => CreateMsxDos(container),
    "xdos"    => new XDosFileSystem(container),   // ADD THIS CASE
    _ => throw new InvalidOperationException($"Unsupported file system: {fileSystemName}")
};
```

Add the using:
```csharp
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
```

---

## CLI Changes

### Program.cs

1. Add `"xdos"` to any `--source-fs` / `--dest-fs` / `--fs` option description strings that
   enumerate supported filesystem types.

2. In the `list` command output, when the detected/selected filesystem is X-DOS,
   display extended columns: `TYPE`, `LOAD`, `END`, `EXEC` alongside the filename.
   Reuse the `LoadAddress`, `EndAddress`, `ExecutionAddress` fields already present on
   `FileEntry`. A minimal implementation may skip the extra columns and just display
   the filename and size as with other filesystems.

3. Add the `using`:
   ```csharp
   using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Provider;
   ```

---

## IDiskContainer ReadSector call convention for X-DOS

X-DOS Track 0 (C=0,H=0) has 256-byte sectors (N=1, 16 sectors).
All other tracks have 512-byte sectors (N=2, 10 sectors each).
The D88 container reads each sector at its stored data size, so:

```
container.ReadSector(cylinder: 0, head: 0, sector: 1)   // returns 256 bytes (Track 0 IPL)
container.ReadSector(cylinder: 0, head: 1, sector: 1)   // returns 512 bytes (FAT)
container.ReadSector(cylinder: 1, head: 0, sector: 1)   // returns 512 bytes (FAM)
```

No special flag is needed — `D88DiskContainer` returns the sector data at the stored
length for that sector record. Confirm this behaviour during implementation.
All XDos reader classes assume the returned byte[] is the correct length.

**Important**: `ReadSector` uses **(cylinder, head, sectorR)** addressing.
X-DOS uses **both sides of the disk** (H=0 and H=1) with an interleaved layout:
```
logical cluster N → cylinder = N/2,  head = N%2
```
This is confirmed by Z80 binary analysis of the X-DOS kernel (EE 10 = XOR 0x10 in
FDC access routine, toggling bit4 of the MB8877A MSDR register = side select).

---

## Test Specification

File: `csharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs`

Use the real disk images at:
- `images/disk_org/x1/XDOS_SYS.D88`
- `images/disk_org/x1/XDOSUTIL.D88`

These paths should be resolved relative to the repository root.

### Required test cases

1. **Provider detection (XDOS_SYS.D88)**
   - `XDosFileSystemProvider.CanHandle(container)` returns `true`

2. **Provider detection (non-X-DOS disk)**
   - Use a Hu-BASIC D88 image; `XDosFileSystemProvider.CanHandle(container)` returns `false`

3. **GetFileSystemInfo (XDOS_SYS.D88)**
   - Returns non-null `DiskFileSystemInfo`
   - `DiskLabel` contains `"X-DOS"` (from the stored disk label)

4. **GetFiles (XDOS_SYS.D88)** — tests correct directory parsing at C=0,H=1,R=2–10
   - Returns at least 30 entries (the XDOS_SYS.D88 disk has ~50+ files)
   - All returned entries have non-empty `FileName`
   - At least one entry has `FileName` containing "X-DOS"
   - At least one entry has `LoadAddress == 0xC800` (the kernel load address)
   - All `FirstCluster` values are >= 2 (clusters 0,1 are reserved system tracks)
   - All `FirstSectorR` values are in range 1–10

5. **GetFiles (XDOSUTIL.D88)**
   - Returns at least 15 entries

6. **FAT data sanity (XDOS_SYS.D88)** — tests correct FAT reading at C=0,H=1,R=1
   - `XDosFatReader` constructed directly from container
   - `_fat[0] == 0x00` (IPL track reserved)
   - `_fat[1] == 0x01` (system marker)
   - `_fat[2] == 0x4A` (first data cluster used)
   - `CountUsedClusters()` returns at least 70 (disk is mostly full)

7. **FAM data sanity (XDOS_SYS.D88)** — tests correct FAM reading at C=1,H=0,R=1
   - `XDosFamReader` constructed directly from container
   - FAM chain starting from cluster 2 is non-empty (`GetChain(2).Count >= 1`)
   - FAM[2] == 9 (confirmed from binary analysis: "X-DOS System" chain 2→9→end)

8. **FileExists (XDOS_SYS.D88)**
   - At least one of these returns `true`: `FileExists("X-DOS System")`,
     `FileExists("XEDIT")`, `FileExists("MKPCM")`
   - `FileExists("DOESNOTEXIST")` returns `false`

9. **FileSystemRegistry auto-detection (XDOS_SYS.D88)**
   - Registry with all providers registered detects X-DOS correctly

10. **ExplicitFileSystemResolver ("xdos")**
    - Creates an `XDosFileSystem` without throwing

### Optional / deferred test cases

11. **ReadFile binary (XDOS_SYS.D88)**
    - Read a known binary file (e.g. "XEDIT"); verify returned byte count > 0
    - First bytes should not be all 0x00 (confirm actual kernel code is read)

12. **ReadFile data/help (XDOSUTIL.D88)**
    - Read a known type-0x04 file; verify non-empty byte array is returned

---

## Known Constraints and TODOs

| # | Location | Issue |
| --- | --- | --- |
| 1 | `XDosClusterReader` | Physical mapping `cylinder=cluster/2, head=cluster%2` is **CONFIRMED** by Z80 binary analysis. Do NOT revert to `head=0` or flat track numbering. |
| 2 | `XDosFamReader.GetChain` | FAM chain is forward-linked: FAM[N] = next cluster. FAM[N]==0x00 means end of chain. Confirmed by: FAM[2]=9, FAM[9]=0 → chain for "X-DOS System" = [2, 9]. |
| 3 | `XDosDirectoryEntry.FileSize` | For type 0x02 (BASIC text), `EndAddress` is a record descriptor, not a byte count. Size calculation will be inaccurate. Return raw cluster data for non-binary types until confirmed. |
| 4 | `XDosFileSystem.ReadBootArea` | Reads 16 sectors at N=1 (256B each); total = 4096 bytes. This is consistent with the D88 stored sector count for Track 0. |
| 5 | CLI | X-DOS has no file extension concept. Callers that strip `string.Empty` extension should not produce a trailing `.` separator. |
| 6 | Filename encoding | Shift-JIS filenames in the 16-byte field will be garbled if decoded as ASCII. Future work should detect non-ASCII bytes and pass `RawFileName` through the `CharacterEncoding` infrastructure. |
