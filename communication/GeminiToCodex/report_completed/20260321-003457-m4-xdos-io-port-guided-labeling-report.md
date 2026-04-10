# Gemini Implementation Report

## Task ID
20260321-003457-m4-xdos-io-port-guided-labeling

## Instruction Filename
20260321-003457-m4-xdos-io-port-guided-labeling.md

## Branch Name
codex/m4-xdos-io-port-guided-labeling

## Summary
Updated the X-DOS kernel analysis workspace with hardware-side context from the Sharp X1 I/O port reference. This improves the interpretability of assembly reconstruction and provides grounded targets for future reverse engineering of disk I/O and boot sequences.

- **FDC Ports**: Added probable labels for 5-inch FDC registers (0xFF8H-0FFFH).
- **Boot/Memory**: Added placeholder labels for IPL ROM mapping (1DxxH/1ExxH) and DMA control (1F80H).
- **Assembly Equates**: Integrated port equates into `read_path.asm` and clarified interleaved side-selection logic (bit 4 toggle).
- **Notes & Conventions**: Documented the "hardware-known but kernel-unconfirmed" classification in `README.md` and `boot_and_io_notes.md`.

## Changed Files
- `analysis/xdos-kernel/labels.tsv`
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`
- `analysis/xdos-kernel/README.md`

## Commands
- `git checkout -b codex/m4-xdos-io-port-guided-labeling`
- `git add analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/read_path.asm`
- `git commit -m "docs: improve xdos kernel analysis labels and notes using x1 hardware reference"`

## Evidence

### labels.tsv additions
```tsv
0x0FF8	fdc_status_cmd	probable	x1_io_ports_reference.md	FDC Status (R) / Command (W)
0x0FF9	fdc_track	probable	x1_io_ports_reference.md	FDC Track Register
0x0FFA	fdc_sector	probable	x1_io_ports_reference.md	FDC Sector Register
0x0FFB	fdc_data	probable	x1_io_ports_reference.md	FDC Data Register
0x0FFC	fdc_control	probable	x1_io_ports_reference.md	Drive/Side/Motor control latch
0x1D00	ipl_rom_on	placeholder	x1_io_ports_reference.md	Map IPL ROM to 0000-7FFF (Any offset in 1DxxH)
0x1E00	ipl_rom_off	placeholder	x1_io_ports_reference.md	Disable IPL ROM (Any offset in 1ExxH)
0x1F80	dma_ctrl	placeholder	x1_io_ports_reference.md	Z80 DMA control/status
```

### read_path.asm clarification
```asm
interleaved_side_select:
    db 0xEE, 0x10   ; xor 0x10 (toggle head bit 4 for side selection)
    ; Note: Observed bytes EE 10 at this logic point.
    ; This likely precedes an 'out (fdc_control), a' to switch physical side.
```

## Risks
- **Low**: Analysis-only changes.
- Port usage at 0xFF8-0FFF is considered "probable" rather than "confirmed" because although they match the hardware spec and the side-select pattern, the kernel entry address for the full FDC routine is not yet fully mapped.

## Requested Review
- Confirm that the side-select bit (bit 4) mapping for 0xFFC is correct for X1 2D systems.
- Verify that using 0x1D00 and 0x1E00 as representative addresses for the range-based ports is acceptable for current labeling.
