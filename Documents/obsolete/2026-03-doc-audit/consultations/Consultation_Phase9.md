# Phase 9: Additional File System Support Consultation

## Goals
Implement support for N88-BASIC (PC-8801) and MSX-DOS (MSX) file systems. This includes detection, directory listing, file reading/writing, and boot area management.

## Technical Approach

### 1. N88-BASIC (PC-8801)
- **Format**: D88 (same as Hu-BASIC).
- **Core Strategy**: 
    - Implement `N88BasicFileSystemProvider` and `N88BasicFileSystem`.
    - Handle 16-byte directory entries (differs from Hu-BASIC's 32-byte).
    - Map FAT (1-2 bytes per entry) and Cluster to Track/Sector.
    - Support 2D (T18/H1 system area) and 2DD/2HD (T40/H0 system area).
    - Handle tokenized BASIC programs (attributes bit 7).

### 2. MSX-DOS (MSX)
- **Format**: Raw DSK (512 bytes per sector).
- **Core Strategy**:
    - Implement `MsxDosFileSystemProvider` and `MsxDosFileSystem`.
    - Implement FAT12 (12-bit entry) parsing with bit-packing logic.
    - Support BPB (BIOS Parameter Block) but fallback to Media Descriptor in FAT for MSX-DOS 1.0 compatibility.
    - Handle standard 32-byte FAT directory entries.

### 3. Native Interop Integration
- Register new providers in `FileSystemRegistry` within `DiskExports`.
- No changes to the `ldk_*` API surface; the existing functions should work transparently once providers are registered.

## Specific Questions for Codex
1. **N88-BASIC FAT copies**: The spec mentions 3 copies of FAT. Should we implement a voting system or just use the first valid one?
2. **MSX-DOS 1.0 vs 2.0**: How critical is the Media Descriptor fallback in the FAT for common MSX disk images? Many emulators seems to use BPB if available.
3. **Encodings**: N88-BASIC uses JIS X 0201 (Half-width Katakana). MSX-DOS uses standard ASCII/Shift-JIS. What are the common pitfalls for character mapping in these legacy file systems when exporting to UTF-8?
4. **Boot Area**: For MSX, we need to handle the 256-byte load limit for the primary boot sector. Any specific advice on `ldk_write_boot_area` for MSX?

## User Requirements
- Generic DDD architecture (already established).
- Implementation must be "generic" but focus on specific file systems like Hu-BASIC (done) and now N88-BASIC/MSX-DOS.
- Native AOT/WASM compatibility (no JIT-sensitive code).
