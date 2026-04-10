# Gemini Task Report

## Task ID
20260323-045800-m19a3-xdos-write-window-table-schema-fix

## Instruction Filename
20260323-045800-m19a3-xdos-write-window-table-schema-fix.md

## Branch Name
codex/m19a3-xdos-write-window-table-schema-fix

## Summary
Fixed the write-path window table schema in `analysis/xdos-kernel/boot_and_io_notes.md` by replacing the previous "Write Path Spec" section with a strict 4-column table. The table includes "label or address", "observed bytes", "evidence class", and "neutral note" for the 5 required windows. All prose and disassembly were removed to adhere to the analysis-only constraint.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`

## Commands
```bash
git checkout develop
git checkout -b codex/m19a3-xdos-write-window-table-schema-fix
# [Edit analysis/xdos-kernel/boot_and_io_notes.md]
git status
git diff HEAD
git add analysis/xdos-kernel/boot_and_io_notes.md
git commit -m "docs: replace write-path spec with 4-column window table"
```

## Evidence
The `## Write Path Entry Windows (Analysis-Only)` section now contains exactly one 4-column table:

| label or address | observed bytes | evidence class | neutral note |
| :--- | :--- | :--- | :--- |
| sys_wopen_impl | 17 CD 34 C9 FE 13 20 17 CD 34 C9 B7 20 FA CD 7E C9 | confirmed | documented entry window |
| sys_wrd_impl | CD 34 C9 B7 CA 38 C9 C9 | confirmed | documented entry window |
| helper_c934 | 02 38 0D 0F 0F 0F 0F 4F 1A 13 CD EA C9 38 01 B1 C1 C9 | confirmed | documented helper window |
| helper_c938 | 0F 0F 0F 4F 1A 13 CD EA C9 38 01 B1 C1 C9 | confirmed | documented helper window |
| helper_c97e | 78 C1 B7 E1 C9 | confirmed | documented helper window |

Verified that no prose exists before or after the table within the section.

## Risks
None. This is a documentation-only update for analysis notes.

## Requested Review
Verify that the table schema and content exactly match the 4-column requirement and that no prose was inadvertently preserved.

## Contradictions
None observed.

## Provisional Conclusions
The write-path analysis is now represented in the strictly required format, focusing exclusively on confirmed byte windows and their classifications.

## Unknown
The internal bit-level semantics of the data handled within these windows remain unknown.
