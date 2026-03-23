# Gemini Task Report

## Task ID
20260321-122730-m8-xdos-impl-entry-windows

## Instruction Filename
20260321-122730-m8-xdos-impl-entry-windows.md

## Branch Name
codex/m8-xdos-impl-entry-windows

## Summary
Successfully extracted and documented direct byte windows for the primary X-DOS filesystem syscall implementation entrypoints from `XDOS_SYS.D88`. Using the confirmed syscall jump table at `0xED78` (File offset `0x7c13`) as an anchor, physical file offsets were calculated and verified for `0xC8xx` and `0xC9xx` memory regions. The analysis confirmed a standard "returns via HL" pattern for `sys_file`, which is consistent with X-DOS's use of inline data for filenames.

## Changed Files
- `analysis/xdos-kernel/read_path.asm`: Added extracted byte windows for `sys_wrd_impl`, `sys_rdd_impl`, `sys_wopen_impl`, `sys_file_impl`, `sys_devi_impl`, and `sys_ropen_impl`.
- `analysis/xdos-kernel/labels.tsv`: Refined the note for `sys_file_impl` to record the HL-based return pattern.
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added detailed physical offset and entrypoint signature notes for the syscall implementation region.

## Commands
```bash
# Verify syscall table anchor
hexdump -C -s 0x7c13 -n 48 images/disk_org/x1/XDOS_SYS.D88
# Inspect implementation region around calculated offset 0x5711 (0xC876)
hexdump -C -s 0x5700 -n 256 images/disk_org/x1/XDOS_SYS.D88
# Exact byte extraction
od -t x1 -j 22272 -N 256 images/disk_org/x1/XDOS_SYS.D88
```

## Evidence
The following implementation entrypoints were directly observed and documented:
- `sys_wopen_impl` (`0xC876`): Offset `0x5711`, bytes `17 CD 34 C9 FE 13 20 17 ...`
- `sys_rdd_impl` (`0xC86C`): Offset `0x5707`, bytes `FD B7 C0 C3 AF D6 ...` (Jumps to `0xD6AF`)
- `sys_file_impl` (`0xC898`): Offset `0x5733`, bytes `E3 C9 CD F4 FD ...` (Returns via HL)
- `sys_devi_impl` (`0xC8C4`): Offset `0x575F`, bytes `CD BC C9 F6 30 1B ...`
- `sys_ropen_impl` (`0xC914`): Offset `0x57AF`, bytes `38 07 FE 11 D8 D6 ...`

## Risks
- **Kernel Fragmentation**: `sys_rdd_impl` jumps to `0xD6AF`, which is significantly far from the main `0xC800` kernel implementation region. This suggests the kernel may be fragmented or parts are loaded into non-contiguous memory blocks.
- **Side-Effect Syscalls**: The `ex (sp), hl / ret` pattern in `sys_file_impl` confirms that some syscalls modify the return address to skip parameters. This must be handled carefully in any future instruction-level modeling.

## Requested Review
Verify that the `0xD6AF` jump target for `sys_rdd` is consistent with the hypothesized memory map, and check if other syscalls also delegate to far regions.

## Contradictions
None observed; the physical offsets derived from the syscall table mapping consistently produced plausible Z80 instruction sequences at the expected targets.

## Provisional Conclusions
The X-DOS kernel implementation for core filesystem operations is mostly clustered around `0xC800`-`0xC900` in `XDOS_SYS.D88`, with some delegation to higher addresses. The syscall implementation entrypoints are now physically anchored for further reverse engineering.

## Unknown
- The exact purpose of the `0xD6AF` region reached by `sys_rdd`.
- The significance of the `17` (`rla`) instruction at the start of `sys_wopen_impl`.

## Notes
- Unrelated local changes (including `communication/` and `csharp/TestCylinders/`) were not reset or cleaned.
- Entrypoint windows for `sys_wopen_impl`, `sys_rdd_impl`, `sys_file_impl`, `sys_devi_impl`, and `sys_ropen_impl` were directly observed and documented.
