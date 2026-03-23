# Gemini Task Report

## Task ID
20260321-131911-m12-xdos-boot-clone-conditions

## Instruction Filename
20260321-132106-m12-xdos-boot-clone-conditions-retry.md

## Branch Name
codex/m12-xdos-boot-clone-conditions-retry

## Summary
Successfully refined the X-DOS boot and clone conditions specification in `analysis/xdos-kernel/boot_and_io_notes.md` with a focus on evidence grading. The revised spec explicitly separates kernel-proven facts (physical layout, logical mapping) from image-level observations (shared clusters in `XDOS_SYS.D88`) and behavioral hypotheses (FirstSectorR encoding). Any premature "decision-complete" claims were removed from `analysis/xdos-kernel/README.md`, replaced with a "Research-Active" status that highlights the critical unknowns blocking a reliable 2D clone implementation.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Rewrote the final section into an evidence-graded "2D X-DOS Boot and Clone Conditions" spec.
- `analysis/xdos-kernel/README.md`: Updated Status to "Research-Active" and listed critical unknowns.

## Commands
```bash
git add analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md
git commit -m "docs(xdos): refine boot/clone conditions with evidence grading"
git status --short
```

## Evidence
- **Proven Physical Layout**: Confirmed Track 0 (IPL), Track 1 (Metadata), and Track 2 (System Metadata) based on kernel logical record constants.
- **Proven Logical Mapping**: Verified the `10-sector-per-track` formula for 2D media, which defines the physical cluster boundaries.
- **Evidence Grading**: The spec now clearly labels "Shared-Cluster Writer is required" as a strong hypothesis rather than a proven decision, and marks the exact encoding of shared clusters as "Unknown".
- **Documentation Correction**: `README.md` now correctly states that the phase is still refining requirements rather than being implementation-ready.

## Risks
- **Implementation Blocking**: The lack of bit-level FAM/Directory encoding for cluster sharing remains the primary blocker for a bit-accurate 2D system clone.

## Requested Review
Review the grading in the "2D X-DOS Boot and Clone Conditions" section of `boot_and_io_notes.md` to ensure it meets the required level of skepticism regarding shared-cluster implementation.

## Contradictions
None; the revised spec aligns with the previous M3 failure evidence while maintaining strict separation between observation and inference.

## Provisional Conclusions
A bootable 2D clone of a dense system disk like `XDOS_SYS.D88` cannot be guaranteed with the current "one cluster per file" writer. Shared-cluster support is a strong candidate for a required feature, but its bit-level mechanics are still unproven.

## Unknown
- The exact bit-level encoding of "starting sector within a cluster" (hypothesized as `FirstSectorR`).
- The specific mapping of the Directory byte at offset 29 (`0x1D`).
- The traversal engine logic at `0xD6AF` that resolves shared clusters.

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **Clone/Boot Conditions Evidence Grading**:
    - **Proven**: Physical layout, logical record mapping (10 sectors/track), volume record location.
    - **Observed (Image)**: Shared-cluster occupancy in `XDOS_SYS.D88`, non-zero `0x1D` bytes.
    - **Observed (Tool)**: 80-cluster limit enforcement, naive clone capacity failure.
    - **Hypothesis**: Shared-cluster writer requirement, `0x1D` as sector offset, 12-bit/4-bit FAM packing.
    - **Unknown**: Bit-level traversal engine logic.
