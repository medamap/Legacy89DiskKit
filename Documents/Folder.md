# Document Index

This folder is organized by both document purpose and technical scope.

Top-level categories:

- `governance/`: current source-of-truth planning and project policy
- `plans/`: active or still-useful execution plans and checklists
- `platform/`: machine-specific specifications and analysis
- `systems/`: cross-platform operating systems, shared formats, and ecosystem material
- `guides/`: integration and usage guides
- `history/`: historical notes, older implementation guidance, and session summaries
- `obsolete/`: retired documents that should not guide current work

## Current Source of Truth

When documents disagree, use this order:

1. current code and CLI help
2. `governance/Agent_Handoff_Roadmap_V2.md`
3. `governance/Roadmap_V2.md`
4. `governance/ROADMAP.md`
5. the relevant platform or systems specification

## Canonical Documents

Use these first.

### Governance

- `governance/Agent_Handoff_Roadmap_V2.md`
- `governance/Roadmap_V2.md`
- `governance/Cpp_Ddd_Folder_Migration_Rulebook.md`
- `governance/ROADMAP.md`
- `governance/Release_Process.md`
- `governance/Technical_Vision.md`

### X1 / X-DOS

- `platform/x1/X-DOS_Filesystem_Analysis.md`: canonical X-DOS on-disk format reconstruction
- `platform/x1/X-DOS_CSharp_Implementation_Spec.md`: implementation-facing X-DOS C# plan
- `platform/x1/X-DOS_License_And_Sources.md`: historical source and license notes
- `platform/x1/HuBasic_Format_Specification.md`: canonical Hu-BASIC format specification

### Shared / Cross-Platform

- `systems/common/D88_Format.md`
- `systems/common/FAT12_Format.md`
- `systems/common/2D_Format_Specification.md`

## Secondary Documents

These are still useful, but they are not the primary source of truth when a canonical document exists.

- `platform/x1/Hu-BASIC_Format.md`: earlier Hu-BASIC analysis note kept for comparison
- `platform/pc8801/N88Basic_Implementation_Handoff.md`
- `systems/common/N88Basic_vs_HuBasic_Analysis.md`
- `history/Implement.md`
- `history/Chat_Summary.md`
- `history/Implementation_History.md`

## Placement Rules

Use these rules for future additions.

- Put machine-specific material under `platform/<machine>/`
  - examples: `platform/x1`, `platform/pc8801`, `platform/msx`, `platform/cpm`
- Put cross-platform OS or ecosystem material under `systems/<name>/`
  - examples: `systems/lsx-dodgers`, `systems/s-os`
- Put shared disk/container/reference material under `systems/common/`
- Put current project policy and authoritative planning under `governance/`
- Put active checklists, phase notes, and execution plans under `plans/`
- Put integration or developer-facing usage guides under `guides/`
- Put historical context or older guidance under `history/`

## Obsolete Documents

Retired documents are moved under:

- `obsolete/2026-03-doc-audit/`

Do not use obsolete documents as the source of truth.

## Practical Notes

- `systems/lsx-dodgers/` is reserved for future LSX-Dodgers material because that ecosystem spans multiple machines.
- `systems/s-os/` is reserved for future S-OS material if it needs to be separated from current Hu-BASIC-focused notes.
- Empty reserved folders may exist only as placeholders until real content is added.
