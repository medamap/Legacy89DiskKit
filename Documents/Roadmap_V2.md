# Roadmap V2

## Purpose

Roadmap V2 is the DDD-oriented migration roadmap for moving the current C# reference implementation toward a future C++-centered implementation line.

It exists to answer these questions clearly:

- which C# responsibility is being migrated
- which architectural layer it belongs to
- which C++ replacement surface is expected
- whether that migration phase is complete or incomplete

This roadmap intentionally prefers many small, explicit migration phases over a smaller number of broad product phases.

When a phase touches C++ structure, use [Cpp_Ddd_Folder_Migration_Rulebook.md](Cpp_Ddd_Folder_Migration_Rulebook.md) to decide whether the touched files should also move into a DDD-oriented folder layout.

## Layer Model

This roadmap uses the following layers:

- Presentation
- Application
- Infrastructure
- Domain

Each phase is assigned to one primary layer, even when it depends on work in other layers.

## Migration Status Model

- `[ ]` not yet complete
- `[x]` complete

A phase is marked complete only when the current repository actually satisfies the intended replacement responsibility for that phase.

## Planning Rule

This roadmap is based on the current C# implementation structure.

The migration is therefore tracked as:

1. identify a C# responsibility
2. decide which C++ replacement surface should exist
3. implement and validate that replacement
4. mark the phase complete only when the replacement responsibility is genuinely present

This means phases may complete out of order.

That is expected.

The same rule applies to structural relocation work. File moves may happen gradually, as long as they remain tied to an active migration phase and follow the rulebook above.

## High-Level Direction

The intended direction is:

1. keep C# as the reference implementation
2. migrate Domain responsibilities into C++
3. migrate Infrastructure responsibilities into C++
4. add bridge or wrapper paths so C# or test-side code can validate C++ behavior early
5. migrate Application responsibilities into C++
6. replace or supplement Presentation and host-facing entrypoints later
7. expand toward WASM and bare-metal only after the required C++ layers are mature

## Completion Rule By Layer

### Domain

A Domain phase is complete when the required rules, models, parsing logic, and transaction-planning logic exist in C++ with meaningful parity to the C# reference behavior.

### Infrastructure

An Infrastructure phase is complete when C++ has concrete adapters or runtime construction paths, not just pure rules.

### Application

An Application phase is complete when C++ has use-case orchestration equivalents, not just helper classes or rule bundles.

### Presentation

A Presentation phase is complete when there is a real executable, bridge, or frontend path that exercises the migrated lower layers for that responsibility.

## Phase List

### Domain Phases

- [x] Phase V2-01: Disk image domain status and result contracts
  - Layer: Domain
  - C# source area: disk-image result and status contracts
  - Expected C++ target: portable status, result, and disk-image metadata types

- [x] Phase V2-02: Disk geometry and sector-address domain rules
  - Layer: Domain
  - C# source area: raw disk geometry and sector offset logic
  - Expected C++ target: raw geometry rules and record/sector address helpers

- [x] Phase V2-03: D88 metadata and parser domain rules
  - Layer: Domain
  - C# source area: D88 header and sector parsing logic
  - Expected C++ target: D88 parse result, metadata, and sector-oriented parser rules

- [x] Phase V2-04: Character encoding domain identifiers and profile rules
  - Layer: Domain
  - C# source area: encoding profile resolution and logical encoding identifiers
  - Expected C++ target: encoding profile types and lookup rules

- [x] Phase V2-05: Hu-BASIC filesystem domain core
  - Layer: Domain
  - C# source area: Hu-BASIC FAT, directory, payload, name, attribute, and write rules
  - Expected C++ target: Hu-BASIC types, rules, transactions, and shell helpers

- [x] Phase V2-06: N88-BASIC filesystem domain core
  - Layer: Domain
  - C# source area: N88-BASIC FAT, directory, payload, name, attribute, and write rules
  - Expected C++ target: N88-BASIC types, rules, transactions, and shell helpers

- [x] Phase V2-07: MSX-DOS filesystem domain core
  - Layer: Domain
  - C# source area: MSX-DOS FAT12, boot, directory, payload, name, attribute, and write rules
  - Expected C++ target: MSX-DOS types, rules, transactions, and shell helpers

- [x] Phase V2-08: Cross-filesystem domain surface catalog
  - Layer: Domain
  - C# source area: implicit supported-surface knowledge spread across providers and bootstrap
  - Expected C++ target: family and capability catalog for migrated filesystem surfaces

- [x] Phase V2-09: Controller-facing domain contracts
  - Layer: Domain
  - C# source area: FDC, drive, timing, controller-facing medium contracts
  - Expected C++ target: host-independent controller and medium contracts
  - Structural relocation: newly introduced controller-facing headers should move under `domain/` when the move is low-risk

- [x] Phase V2-10: Raw preservation domain model
  - Layer: Domain
  - C# source area: raw-direction notes and future preservation model
  - Expected C++ target: encoded-track, metadata, integrity, and conversion domain contracts
  - Structural relocation: newly introduced raw preservation headers should be created directly under `domain/raw/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

### Infrastructure Phases

- [x] Phase V2-11: Disk image buffer-loading infrastructure
  - Layer: Infrastructure
  - C# source area: path or buffer image loading shells
  - Expected C++ target: buffer-oriented container construction entrypoints
  - Structural relocation: newly introduced buffer-loading infrastructure should be created directly under `infrastructure/disk_image/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-12: Raw disk container infrastructure
  - Layer: Infrastructure
  - C# source area: raw container assembly and sector access shells
  - Expected C++ target: concrete raw container adapters over migrated domain rules
  - Structural relocation: newly introduced raw container infrastructure should be created directly under `infrastructure/disk_image/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-13: D88 container infrastructure
  - Layer: Infrastructure
  - C# source area: D88 container shell and concrete sector access
  - Expected C++ target: concrete D88 container adapters over migrated parser and domain rules
  - Structural relocation: newly introduced D88 container infrastructure should be created directly under `infrastructure/disk_image/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-14: Character encoding table infrastructure
  - Layer: Infrastructure
  - C# source area: concrete encoding table and conversion data
  - Expected C++ target: reusable encoding table data and lookup infrastructure
  - Structural relocation: newly introduced character-encoding infrastructure should be created directly under `infrastructure/character_encoding/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-15: Hu-BASIC filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: Hu-BASIC filesystem implementation shell
  - Expected C++ target: concrete Hu-BASIC container-backed infrastructure over domain rules
  - Structural relocation: newly introduced Hu-BASIC filesystem infrastructure should be created directly under `infrastructure/filesystem/hu_basic/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-16: N88-BASIC filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: N88-BASIC filesystem implementation shell
  - Expected C++ target: concrete N88-BASIC container-backed infrastructure over domain rules
  - Structural relocation: newly introduced N88-BASIC filesystem infrastructure should be created directly under `infrastructure/filesystem/n88_basic/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-17: MSX-DOS filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: MSX-DOS filesystem implementation shell
  - Expected C++ target: concrete MSX-DOS container-backed infrastructure over domain rules
  - Structural relocation: newly introduced MSX-DOS filesystem infrastructure should be created directly under `infrastructure/filesystem/msx_dos/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-18: Explicit filesystem selection infrastructure
  - Layer: Infrastructure
  - C# source area: explicit resolver and provider wiring
  - Expected C++ target: provider-independent selection and resolver infrastructure
  - Structural relocation: newly introduced explicit selection infrastructure should be created directly under `infrastructure/filesystem/` and filesystem-specific explicit helpers should remain under their family subfolders per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-19: Filesystem detection infrastructure
  - Layer: Infrastructure
  - C# source area: filesystem detection and candidate evaluation
  - Expected C++ target: detection infrastructure over concrete container and filesystem adapters
  - Structural relocation: newly introduced filesystem detection infrastructure should be created directly under `infrastructure/filesystem/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-20: Mounted-medium and controller-medium infrastructure
  - Layer: Infrastructure
  - C# source area: mounted medium binding and controller-facing medium adapters
  - Expected C++ target: concrete mounted-medium and controller-medium adapters
  - Structural relocation: newly introduced mounted-medium infrastructure should be created directly under `infrastructure/drive/`, and newly introduced controller-facing medium adapters should be created directly under `infrastructure/fdc/medium/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-21: Native bridge infrastructure over C++ implementations
  - Layer: Infrastructure
  - C# source area: current native bridge backed by managed implementation
  - Expected C++ target: ABI-compatible native bridge backed by C++ implementations
  - Structural relocation: newly introduced native bridge infrastructure should be created directly under `infrastructure/native/` per the [DDD folder migration rulebook](Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-22: WASM path-independent infrastructure
  - Layer: Infrastructure
  - C# source area: browser-first and buffer-first WASM shape definition
  - Expected C++ target: buffer-oriented infrastructure suitable for WASM-facing runtime binding

### Application Phases

- [x] Phase V2-23: Disk service application layer in C++
  - Layer: Application
  - C# source area: disk open, create, format, and metadata orchestration
  - Expected C++ target: application service for disk-level use cases

- [x] Phase V2-24: File transfer application layer in C++
  - Layer: Application
  - C# source area: import, export, overwrite, and payload transfer orchestration
  - Expected C++ target: application service for file-level use cases
  - Completion Note: Implemented `FileTransferService` and `CharacterEncodingService`. Note: Current implementation uses a simplified ASCII check (bit 0) and maps PC88/MSX encoding to X1 table as a temporary measure. Full parity verification belongs to V2-30.

- [x] Phase V2-25: Directory layout application layer in C++
  - Layer: Application
  - C# source area: layout-oriented listing, ordering, and plan handling
  - Expected C++ target: application service for directory layout operations
  - Completion Note: Implemented `DirectoryLayoutService` with plan-based orchestration. Supported Hu-BASIC directory layout reconstruction and label insertion. Fixed a session bug where `Format()` misidentified Hu-BASIC as N88-BASIC.

- [x] Phase V2-26: Boot and clone application layer in C++
  - Layer: Application
  - C# source area: boot area and disk clone orchestration
  - Expected C++ target: application service for boot and clone workflows
  - Completion Note: Implemented `BootAndCloneService` for transferring boot areas and files between disks. Standardized `ReadBootArea` / `WriteBootArea` across all C++ file systems.

- [x] Phase V2-27: Explicit resolver and bootstrap application layer in C++
  - Layer: Application
  - C# source area: managed bootstrap and supported default wiring
  - Expected C++ target: native or C++ bootstrap for supported application services
  - Completion Note: Implemented `legacy89diskkit::cpp::application` namespace with factory functions for all services. Added `ExplicitFileSystemResolver` with `InitializeForDetection` to enable scoring-based Hu-BASIC detection.

- [x] Phase V2-28: Controller-facing application layer in C++
  - Layer: Application
  - C# source area: mounted-medium binding, FDC access, and host adapter orchestration
  - Expected C++ target: application services for controller-shaped and host-facing runtime flows
  - Completion Note: Implemented `DriveMountService`, `MountedMediumBindingService`, `FdcAccessService`, and `EventDrivenEmulatorFdcHostAdapter`. Introduced `FdcMediumController` in Infrastructure to bridge mediums to FDC interface.

- [x] Phase V2-29: Managed-to-native validation bridge
  - Layer: Application
  - C# source area: managed application calling into native or C++-backed implementations
  - Expected C++ target: validation path that allows C# side workflows to exercise C++ slices early
  - Completion Note: Implemented `ValidationNativeBridgeBackend` which routes calls to both C# reference and C++ implementation. Created a C++ SHARED library and C# P/Invoke backend to enable this interop.

- [x] Phase V2-30: Representative workflow parity verification
  - Layer: Application
  - C# source area: list, extract, inject, rename, delete, format, info, and clone flows
  - Expected C++ target: workflow-level comparison between the C# reference and the C++-backed path
  - Completion Note: Verified all major workflows using `ValidationNativeBridgeBackend`. Fixed attribute mapping disparities between C# and C++.

### Presentation Phases

- [x] Phase V2-31: C++ diagnostic and smoke executables
  - Layer: Presentation
  - C# source area: current test and verification harnesses
  - Expected C++ target: minimal executables that prove migrated slices directly

- [x] Phase V2-32: Native test app and host-side verification presentation
  - Layer: Presentation
  - C# source area: native test app and diagnostic host usage
  - Expected C++ target: equivalent verification-facing entrypoints over C++-backed native infrastructure
  - Completion Note: Implemented `ldk-verify` C++ tool for comprehensive disk verification and bootable disk creation. Extended `NativeBridge` and `NativeFileSystemSession` with raw sector access (`ldk_read_sector`/`ldk_write_sector`).

- [x] Phase V2-33: CLI backend substitution bridge
  - Layer: Presentation
  - C# source area: managed CLI command handlers
  - Expected C++ target: C# CLI paths able to call C++-backed services through a stable bridge
  - Completion Note: Refactored `DiskService` to support `INativeBridgeBackend` substitution. Added `--native` option to C# CLI to switch between managed and C++ backends. Updated `ValidationNativeBridgeBackend` to support full `IDiskContainer` parity.

- [x] Phase V2-34: CLI command migration to C++-backed workflows
  - Layer: Presentation
  - C# source area: `list`, `info`, `file`, `disk`, `boot`, and host-facing commands
  - Expected C++ target: selected commands routed through validated C++-backed application paths
  - Completion Note: Validated all major CLI commands (`list`, `file`, `disk`, `boot`, `layout`) through the native bridge (`--native`). Resolved encoding differences by moving `NativeFileEntry` string fields to byte arrays and using the file system's target encoder in the `NativeInterop` facade. Implemented C API bindings for directory layout manipulation (`ldk_read_directory_layout` / `ldk_apply_directory_layout`).

- [x] Phase V2-35: Full C++ CLI
  - Layer: Presentation
  - C# source area: current `Legacy89DiskKit.Cli`
  - Expected C++ target: standalone CLI over C++ application and infrastructure layers
  - Completion Note: Implemented standalone `ldk` C++ CLI mirroring the command surface of the C# CLI (`list`, `file`, `disk`, `boot`, `layout`). Integrated `CliLocalizer` for multi-language support (ja/en) and enabled basic encoding resolution via `CharacterEncodingTableCatalog`.

- [x] Phase V2-36: WASM presentation runtime
  - Layer: Presentation
  - C# source area: current documented-only WASM direction
  - Expected C++ target: browser-facing or WASI-facing runtime entrypoint over path-independent core and infrastructure
  - Completion Note: Implemented `ldk_wasm.cpp` entrypoint exposing path-independent Core and Infrastructure services. Validated WASM configuration via CMake `ldk-wasm` target, utilizing existing C API `native_bridge_exports` and memory helpers for JS/browser contexts.

## Current Repository Reading

The current repository state supports the following interpretation.

- Domain migration is the most advanced area
- filesystem-domain work for Hu-BASIC, N88-BASIC, and MSX-DOS has meaningful C++ coverage
- cross-family Domain visibility exists through the filesystem surface catalog
- Presentation has minimal direct smoke and diagnostic executables
- Infrastructure and Application migration remain largely ahead of the project
- native and CLI replacement work have not yet crossed into C++-backed runtime substitution

In DDD terms, the current state is not "full C++ parity."

It is better described as:

- substantial Domain migration
- minimal Presentation-level smoke support
- limited or not-yet-started Infrastructure migration
- not-yet-started Application migration

## Current Bottlenecks

The current bottlenecks are:

1. C++ disk-image and filesystem Infrastructure
2. C++ Application services
3. bridge validation from C# Application into C++-backed paths
4. Presentation substitution at the CLI boundary

## Recommended Immediate Order

The recommended next execution order is:

1. Phase V2-11 through Phase V2-19
2. Phase V2-23 through Phase V2-30
3. Phase V2-21 and Phase V2-33 through Phase V2-35
4. Phase V2-22 and Phase V2-36 when the lower layers are mature enough

This order keeps the migration understandable:

- Domain first
- Infrastructure second
- Application third
- Presentation replacement after validation

## Interpretation Rule

Completion of a Domain phase does not imply completion of Infrastructure, Application, or Presentation for the same subsystem.

For example:

- a filesystem Domain phase may be complete
- while the matching Infrastructure phase remains incomplete
- which means the end-user CLI still cannot rely on that C++ implementation path directly

This distinction is essential to keeping the migration roadmap honest.

## Future Refinements

- **Multi-byte Filename Encoding**: Current C++ parsers (`DecodeTrimmed`) perform simple `char` casting, which causes ShiftJIS kanji filenames to appear corrupted in UTF-8 terminals. A proper encoding-aware decoder or raw byte preservation in the Domain layer is required.
- **Stable ID Parity**: `DirectoryLayoutService` uses `std::hash`, which lacks binary parity with C# SHA256 IDs.
- **D88 Template Maturity**: `NativeFileSystemSession::Create` uses fixed geometry templates which may conflict with source disks during cloning (V2-32 finding).

## Post-V2 Considerations

The following items are out of scope for the V2 roadmap and should be considered only after V2-36 is complete.

### X-DOS Filesystem Support (First Post-V2 Priority)

X-DOS is a Sharp X1-exclusive OS distributed by C&S Soft. Its filesystem is structurally unlike every other filesystem currently supported by Legacy89DiskKit: it uses a directly-addressed track/sector layout, a flat allocation bitmap, and a mixed-geometry disk structure that cannot be handled by reusing the existing FAT12, Hu-BASIC, or N88-BASIC infrastructure. This work should begin before any other Post-V2 item.

#### Disk Physical Layout

The following layout is confirmed by binary analysis of `XDOS_SYS.D88`:

X-DOS uses **both sides** of the disk with an interleaved (zigzag) track layout:
logical cluster N → `cylinder = N/2, head = N%2`

| Logical Track | Physical Address | Sector geometry | Content |
| :--- | :--- | :--- | :--- |
| Track 0 (cluster 0) | C=0, H=0 | R=1–16, N=1 (256 bytes each) | IPL boot code; R=1 is the X-DOS Volume Record |
| Track 1 (cluster 1) | C=0, H=1 | R=1–10, N=2 (512 bytes each) | R=1 = FAT bitmap; R=2–10 = directory entries |
| Track 2 (cluster 2) | C=1, H=0 | R=1–10, N=2 (512 bytes each) | R=1 = FAM cluster chain table; R=2 = bdir (Z80 system code) |
| Track 3+ (cluster 3+) | C=1,H=1 / C=2,H=0 / C=2,H=1 / … | R=1–10, N=2 (512 bytes each) | File content; 10 × 512B = 5120 bytes per logical track |

This mixed-geometry layout (Track 0 uses 256B sectors; Track 1+ uses 512B sectors) means the disk cannot be read using a single uniform sector-size assumption. Container construction must handle both sector sizes.

The interleaved dual-sided layout is **confirmed by Z80 binary analysis** of the X-DOS kernel:
the FDC access routine at C=1,H=1,R=8 contains `EE 10` (XOR 0x10 = toggle MB8877A MSDR bit4 = head select) and `14` (INC D = advance cylinder only when returning to H=0). This produces the zigzag pattern across both disk sides.

#### Volume Record Format (Track 0, R=1, 256 bytes)

| Offset | Size | Content |
| :--- | :--- | :--- |
| [0] | 1 | Record type identifier: 0x01 |
| [1:17] | 16 | Disk label, ASCII, space-padded |
| [24] | 1 | Format type byte (0x88 = Sharp X1 2D) |
| [25:28] | 3 | BCD date: year, month, day (e.g. 0x24, 0x04, 0x17 = April 17, 1984) |

Bytes outside the above ranges are present in the sector but their semantics are not yet fully confirmed by analysis and must be treated as reserved during initial implementation.

#### FAT Format (Track 1 = C=0,H=1, R=1, 512 bytes)

The FAT is a flat allocation bitmap. Each byte represents one cluster. Confirmed byte values:

| Value | Meaning |
| :--- | :--- |
| 0x00 | Free cluster |
| 0x01 | Reserved (appears only at index 1) |
| 0x4A | Allocated/used |
| 0x3F | Observed in the FAT beyond the last used entry; exact meaning TBD |
| 0xC0, 0xFF | Observed beyond used range; exact meaning TBD |

Index 0 and index 1 are reserved (0x00 and 0x01 respectively). Index 2 onwards addresses data clusters. Cluster N → `cylinder = N/2, head = N%2` (interleaved mapping confirmed by directory entry cross-reference). Unlike FAT12/FAT16, the FAT is purely an occupancy bitmap — it contains no cluster chain links. Cluster chain navigation for multi-cluster files is handled by the **FAM** (File Allocation Map) at C=1,H=0,R=1; see the FAM section in `X-DOS_Filesystem_Analysis.md` for details.

#### Directory Entry Format (Track 1 = C=0,H=1, R=2–10, 32 bytes each)

| Offset | Size | Content |
| :--- | :--- | :--- |
| [0] | 1 | File type (see table below) |
| [1] | 1 | Attribute byte |
| [2:18] | 16 | Filename, ASCII, space-padded (no extension separator) |
| [20:22] | 2 | Load address, little-endian |
| [22:24] | 2 | End address (binary files) or record descriptor (text files), little-endian |
| [24:26] | 2 | Execution address, little-endian |
| [26:28] | 2 | Unknown (observed as checksum or timestamp fragment; do not rely on) |
| [28] | 1 | Flags byte (0x80 is common; exact bit semantics TBD) |
| [29] | 1 | First FAM cluster index (chain head; provisional — see Known Pitfalls) |
| [30] | 1 | Starting sector R within the first cluster (R=1–10; confirmed range from directory analysis) |
| [31] | 1 | Always 0x01 in observed data (purpose unknown) |

A directory entry with [0] = 0x00 or [0] = 0xFF is treated as an empty/deleted slot.

#### File Type Codes

| Value | Type | Notes |
| :--- | :--- | :--- |
| 0x02 | BASIC text program | `end` field stores record descriptor, not end address |
| 0x03 | Binary (machine code) | `load`/`end`/`exec` all meaningful |
| 0x04 | Help / auxiliary data | `end` stores a page count |
| 0x05 | Overlay / system module | Loaded by the OS on demand |
| 0x06 | Script / batch | Similar to `.BAT` |
| 0x07 | Core system file | Used by X-DOS kernel and BIOS |

Values with bit 7 set (e.g. 0x85, 0xAE, 0xB4, 0xC0) have been observed in directory entries. These may represent application-defined extensions or corrupted entries and should be treated as unknown during initial implementation.

#### Implementation Phases

The following phases are required to support X-DOS in both the C# reference and C++ implementations.

**Phase XD-01: X-DOS filesystem domain core**
- Layer: Domain
- New domain types: `XDosFileType`, `XDosVolumeRecord`, `XDosDirectoryEntry`, `XDosAllocationBitmap`
- New domain rules: directory parsing rules, allocation bitmap rules, file type classification rules, load/end/exec address triple rules, track/sector address rules
- Note: the mixed-geometry track 0 structure requires an explicit Track-0 domain model separate from the data-track model

**Phase XD-02: X-DOS filesystem infrastructure**
- Layer: Infrastructure
- New infrastructure: `XDosFileSystem` backed by D88 container
- Key requirement: container must be able to read Track 0 sectors (N=1, 256B) and Track 1+ sectors (N=2, 512B) within the same session
- File read path: start from `entry[29]` (first FAM cluster), follow FAM chain until 0x00, read each cluster's sectors to reconstruct payload
- Structural relocation: files should be created under `infrastructure/filesystem/x_dos/`

**Phase XD-03: X-DOS detection and registration**
- Layer: Infrastructure
- Detection heuristic: Track 0, R=1, byte[0] = 0x01 (Volume Record identifier), format type byte[24] = 0x88
- Registration: add X-DOS to `FilesystemSurfaceCatalog` and `FilesystemDetection` infrastructure
- Explicit selector: add `XDosExplicitFileSystem` alongside existing explicit selectors

**Phase XD-04: X-DOS write and format support**
- Layer: Domain + Infrastructure
- Write path: allocation bitmap update, directory entry creation, contiguous-track write
- Format path: write Volume Record to C=0,H=0,R=1; write empty FAT bitmap to C=0,H=1,R=1; zero directory area C=0,H=1,R=2–10
- Note: the end address / allocation boundary encoding in entry[29] must be resolved before write support is correct

**Phase XD-05: X-DOS application and CLI integration**
- Layer: Application + Presentation
- C# application: integrate X-DOS into `FileTransferService`, `DiskService`, and `DirectoryLayoutService`
- C++ application: same integration into C++ application services
- C++ CLI: add X-DOS as a recognized `--fs` value in `ldk`

#### Known Implementation Pitfalls

**Mixed-geometry sector size**

Track 0 uses 256-byte sectors (N=1). Tracks 1 and above use 512-byte sectors (N=2). Any code that assumes a uniform sector size across the entire disk will fail on Track 0 reads. The container must report the correct sector size per track and the filesystem layer must account for this when reading the Volume Record.

**FAT and FAM roles are distinct**

The FAT (C=0,H=1,R=1) is a flat occupancy bitmap where 0x4A means allocated and 0x00 means free. It does not encode cluster chains.
Cluster chain navigation is handled by the **FAM** (File Allocation Map, C=1,H=0,R=1), where FAM[cluster] = next cluster in chain, 0x00 = end of chain.
Files may therefore be allocated in **non-contiguous clusters**. The FAT indicates whether a cluster is in use; the FAM traces the actual chain for each file.
Confirmed: FAM[2]=9, FAM[9]=0 → "X-DOS System" file spans clusters 2 and 9 (= C=1,H=0 and C=4,H=1).

**Directory entry[29] and entry[30] encoding — CONFIRMED**

- `entry[29]` = first cluster index (FAM chain head for the file)
- `entry[30]` = starting sector R within that first cluster (1–10)

Multiple files share the same `entry[29]` value with different `entry[30]` values, consistent with multiple small files packed into a single cluster (one logical track). The physical location is: `cylinder = entry[29]/2, head = entry[29]%2, sectorR = entry[30]`.

**Cluster-to-physical-track mapping — CONFIRMED**

Cluster N → `cylinder = N/2, head = N%2` (interleaved dual-sided layout).
1 cluster = 1 logical track = 10 sectors × 512 bytes = 5120 bytes.
Verified by cross-referencing all directory entries in XDOS_SYS.D88 against the D88 sector map.
Note: **H=0 only is WRONG**. X-DOS uses both sides. This was confirmed by Z80 disassembly of the FDC access routine.

**16-character filename with no extension**

X-DOS filenames are 16 bytes, ASCII, space-padded, with no `.extension` concept. Filename matching must be case-sensitive and space-exact. Tools that assume 8.3 naming or that strip trailing spaces before comparison will not behave correctly.

**File type byte values above 0x07**

Values such as 0x85, 0xAE, 0xB4, 0xC0 appear in the analyzed directory. These are likely application-defined extensions. The implementation should not reject entries with unknown type bytes — it should expose the raw byte and leave interpretation to the caller.

#### Reference Resources

| Topic | Source | Notes |
| :--- | :--- | :--- |
| X-DOS disk structure (primary) | Binary analysis of `XDOS_SYS.D88` | Direct sector-level inspection; see above |
| X-DOS user documentation | Sharp X1 technical archives | Physical or scanned documentation |
| X-DOS sector R numbering | Cross-reference with D88 sector headers | All R values in analyzed image are 1-indexed |

- **WASM Linker Dead Code Elimination**: Existing C APIs in `native_bridge_exports.cpp` do not have `EMSCRIPTEN_KEEPALIVE`. When performing a production WASM build, ensure that `-s EXPORTED_FUNCTIONS` is used in CMake or `EMSCRIPTEN_KEEPALIVE` is added to all required entrypoints to prevent the linker from stripping them.

### PC-9801 Disk Image Format Support (FDI / HDI)

References:
- FDI/HDI specification: https://www.pc98.org/project/doc/hdi.html
- Anex86 PC-98 floppy image: http://justsolve.archiveteam.org/wiki/Anex86_PC98_floppy_image
- Reference tools: https://github.com/tsdko/98imgtools

#### Format Overview

- **FDI**: PC-9801 floppy disk image. Header is nominally 4096 bytes, followed by raw sector data. The `HeaderSize` field at offset 0x08 must be read to determine the actual data start offset rather than assuming a fixed value.
- **HDI**: PC-9801 hard disk image. Contains a fixed-size header with geometry fields (Cylinders, Surfaces, Sectors, SectorSize). Partition table analysis is required for filesystem access. Image size and header geometry should be cross-validated because malformed images exist.
- Both formats store filenames in Shift-JIS. FAT12/FAT16 filesystem naming must go through the same encoding layer used by MSX-DOS.

#### Implementation Constraints

The following constraints must be enforced at implementation time. These are not optional.

1. **No implicit struct padding**: All header structures (FDI, HDI) must use `#pragma pack(push, 1)` or `[[gnu::packed]]`. Each structure must include a `static_assert` verifying the byte size matches the specification (FDI header: 4096 bytes, etc.).

2. **Explicit little-endian conversion**: Multi-byte fields must not be read by casting a byte pointer directly to `uint32_t*` or by `fread` into a struct. Each field must be extracted via an explicit conversion function (e.g., `read_u32_le(buffer, offset)`) that is safe on both little-endian and big-endian hosts.

3. **`HeaderSize` field must be respected**: The data start offset must be derived from the `HeaderSize` field in the header, not from a hardcoded constant. This applies to both FDI and HDI.

4. **`FDDType` magic number validation**: The `FDDType` field at offset 0x04 must be validated against known magic values (e.g., `PDA0`) before any further parsing proceeds.

5. **Buffer-safe serialization**: Raw pointer arithmetic over heap buffers is not acceptable. All buffer access must go through `std::vector<uint8_t>` or an equivalent bounds-checked span type to prevent buffer overrun.

#### Design Recommendations

- **Linear storage abstraction**: FDI and HDI are essentially raw images with a header prefix. They should be abstracted as a `LinearDiskContainer` (or equivalent) rather than reusing the D88 sector-pointer model. This abstraction would also accommodate future NHD (T98-Next format) support without structural changes.

- **Value-object byte extraction**: Header fields should be read through a value object that wraps a `std::vector<uint8_t>` and exposes named accessors (e.g., `header.header_size()`, `header.fdd_type()`). Direct struct casting from a raw buffer is prohibited per constraint 2 above.

- **Encoding layer reuse**: PC-9801 Shift-JIS filenames must reuse the existing `CharacterEncoding` infrastructure. No new hardcoded encoding paths should be introduced.

- **Dependency injection for allocation**: If bare-metal or OS-less targets are in scope at that time, memory allocation should be injectable from the outside rather than relying on `new` or `malloc` directly inside the container implementation.

#### Scope at Implementation Time

- New Domain phases will be required for FDI/HDI geometry rules and HDI partition table rules.
- New Infrastructure phases will be required for FDI and HDI container adapters.
- The existing FAT12/MSX-DOS filesystem infrastructure should be reusable for the FAT layer on top of an HDI container, provided the container abstraction is compatible.

#### Reference Resources

| Topic | Source | URL |
| :--- | :--- | :--- |
| HDI partition structure | えぬまのページ | http://www.n-uma.jp |
| FAT12/FAT16 BPB and cluster calculation | OSDev Wiki | https://wiki.osdev.org |
| FD geometry (2HD/2DD sector counts and track layout) | ジャンク・ハーツ | http://www.junk-hearts.com |
| BIOS-level disk I/O conventions | PC-9801 Technical Data Book | physical book / archive |

#### Known Implementation Pitfalls

The following pitfalls are documented based on known PC-98 format behavior. Each one has caused silent data corruption or incorrect parsing in existing tools and must be explicitly addressed at implementation time.

**HDI: Partition table physical layout**

- The first sector of the HDI data area (immediately after the header) contains the IPL and partition table.
- Each partition entry is 16 bytes. Up to 16 partition entries exist.
- Each entry includes an OS type code in addition to an active flag (0x80). MS-DOS partitions use type 0x01. Treating all partitions as FAT without checking the type code risks treating FreeBSD or other OS partitions as FAT and corrupting them.

**FAT: Sector size is not always 512 bytes**

- PC-98 2HD (1.2 MB) disks use 1024 bytes per sector as the standard, not 512.
- `BytesPerSector` must always be read from the image header or BPB at runtime. Hardcoding 512 is incorrect.
- The cluster offset formula `(ClusterIndex - 2) * SectorsPerCluster * BytesPerSector` must use the dynamically obtained `BytesPerSector` value.

**FAT: Filename encoding and the 0xE5 deletion flag**

- FAT directory entries are 32 bytes: 8-byte name, 3-byte extension. Long File Names (LFN) do not exist in PC-98 MS-DOS environments and must not be implemented.
- The 0xE5 byte marks a deleted entry, but it also collides with the first byte of some Shift-JIS characters (e.g., filenames starting with certain kana). The historical workaround substitutes 0x05 for a leading 0xE5 in live entries. This must be handled correctly in both read and write paths.

**Geometry consistency validation**

- FDI and HDI headers both contain CHS geometry fields (Cylinders, Heads, Sectors per track, Bytes per sector).
- The invariant `ImageDataSize == Cylinders * Heads * SectorsPerTrack * BytesPerSector` must be verified during container construction. Images that violate this invariant exist in the wild and must be rejected or flagged rather than silently accepted.
