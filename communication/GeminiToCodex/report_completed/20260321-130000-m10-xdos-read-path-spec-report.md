# Gemini Task Report

## Task ID
20260321-130000-m10-xdos-read-path-spec

## Instruction Filename
20260321-130000-m10-xdos-read-path-spec.md

## Branch Name
codex/m10-xdos-read-path-spec

## Summary
Successfully consolidated X-DOS read-path evidence into a conservative pseudo-spec within `analysis/xdos-kernel/boot_and_io_notes.md`. The spec covers `sys_file`, `sys_ropen`, `sys_rdd`, and the primary read-engine delegate `helper_d6af`. Analysis confirmed the "inline parameter skipping" pattern for filename setting and identified a structure-offsetting pattern (`add hl, 0x1D`) that likely points to FCB or Directory entry processing.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added "Read Path Spec (Conservative Reconstruction)" section.

## Commands
```bash
# Verify the spec addition
cat analysis/xdos-kernel/boot_and_io_notes.md
# Check branch status
git status
# Verify diff against develop
git diff develop...HEAD -- analysis/xdos-kernel/boot_and_io_notes.md
```

## Evidence
The produced spec is based on the following directly observed byte sequences:
- `sys_file` (`0xC898`): `E3 C9` (`ex (sp), hl` / `ret`) - Confirms inline data usage.
- `sys_ropen` (`0xC914`): `38 07 FE 11 D8 D6 07 FE 10 3F C9` - Confirms register-A validation and Carry-flag based status return.
- `sys_rdd` (`0xC86C`): `FD B7 C0 C3 AF D6` - Confirms immediate delegation to `0xD6AF`.
- `helper_d6af` (`0xD6AF`): `... 11 1D 00 19 56 23 5E ...` - Confirms the use of `0x1D` (29) as a structure offset.

## Risks
- **Hypothesis Overlap**: While the byte-level behavior is confirmed, the exact mapping of `0x1D` to a specific field in a Directory entry (which is 32 bytes in X-DOS) is an inference.
- **Helper Depth**: The read path extends to at least `0xD753`, which is currently the limit of direct observation for this task.

## Requested Review
Review the "Read Path Spec" section in `boot_and_io_notes.md` to ensure the distinction between direct observation, inference, and hypothesis is sufficiently clear for the next implementation phase.

## Contradictions
None observed; the instruction sequences at the confirmed implementation entrypoints are consistent with the hypothesized filesystem roles.

## Provisional Conclusions
X-DOS uses a CP/M-like calling convention for filename setting (inline strings) but employs a deeply layered implementation for actual sector/cluster reading, delegating core logic to routines in the `0xDxxx` memory range.

## Unknown
- The specific field meaning at offset `29` within the `0x1D` structure.
- The functional roles of sub-calls `0xD155`, `0xE00E`, and `0xDEE8`.

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **Direct Observation vs Inference**: 
  - Direct Observation: Instruction bytes and disassembly for all listed entrypoints.
  - Inference: Functional interpretation of `ex (sp), hl` as inline parameter skipping and `add hl, 0x1D` as structure field access.
  - Hypothesis: Mapping these patterns to high-level OS concepts like "Set active filename" or "Read engine delegate".
