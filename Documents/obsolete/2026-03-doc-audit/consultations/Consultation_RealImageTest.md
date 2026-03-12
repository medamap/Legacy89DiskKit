# Consultation Report: Real Image Verification Plan (Phase 10)

## Objective
Verify the current implementation of Hu-BASIC (X1), N88-BASIC (PC-8801), and MSX-DOS (MSX) file systems using real disk images available in the repository.

## Available Real Images
### X1 (Hu-BASIC / S-OS)
- `CZ8FB01.d88` / `cz8fb01.2d` (Hu-BASIC 1.0)
- `CZ8FB02.2d` (Hu-BASIC 2.0)
- `X1turboIIIDemo.d88` (2HD Turbo image)
- `ldsys.d88`
- Various Game and Utility disks (`.2D`, `.D88`)

### PC-8801 (N88-BASIC)
- `PC-88SR.D88` (2D)
- `[OS] PC-8801MA system disk (N88 BASIC).d88` (2DD/2HD)
- `PC-8801mkIISR store demo.d88`

### MSX (MSX-DOS)
- `ldsys.dsk` (720KB raw)
- `ROM.DSK` (720KB raw)

## Current Implementation State
- **Application Layer**: `DiskService`, `FileTransferService` (generalized for all FS).
- **Native Interop**: C-style API for Open, Read, Write, List, Rename, Delete, Attributes, BootArea.
- **Infrastructure Layer**:
    - **Hu-BASIC**: Boot sector detection, FAT16-like management, complex directory parsing.
    - **N88-BASIC**: 16-byte directory entries, 1-byte FAT entries, 2D/2DD geometry support.
    - **MSX-DOS**: FAT12 Support, BPB parsing, 32-byte directory entries.
- **Domain Layer**: `IFileSystem` interface, `EncoderRegistry` for platform-specific character handling.

## Request for Codex
Please provide a systematic **Test Plan** for verifying these implementations against the real images. Specifically:

1.  **Detection Test**: How to verify `CanHandle` logic for different densities (2D/2DD/2HD) and container formats (D88 vs RAW).
2.  **Read Integrity**: Steps to verify that filenames, attributes, and file contents match the expected data on these vintage systems.
3.  **Cross-Platform Charsets**: How to test character encoding (Katakana, symbols, vertical bars) using these images.
4.  **Write Safety**: How to safely test writing/deleting without corrupting the original structure, and what invariants to check post-write.
5.  **Geometry Edge Cases**: Potential pitfalls with sector sizes (256 vs 1024), track counts, and media descriptors that might differ in real-world samples.
6.  **Boot Area**: Verifying the IPL (Initial Program Loader) can be read and written correctly for bootable images.

User requirements:
- Ensure the test plan is systematic.
- Store the resulting plan in `Documents/plans/RealImageTestPlan.md`.
