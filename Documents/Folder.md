# Document Index

This folder contains the project documents that are still useful for current development, reference work, or release handling.

## Active Planning and Status

Use these files first when you need the current project direction.

- `Agent_Handoff_Roadmap_V2.md`: single-entry handoff note for resuming the active Roadmap V2 migration work
- `ROADMAP.md`: broader long-term direction
- `Roadmap_V2.md`: DDD-oriented C# to C++ migration roadmap with per-layer phase status
- `Cpp_Ddd_Folder_Migration_Rulebook.md`: staged relocation policy and mapping ledger for moving C++ work into a DDD-oriented folder layout
- `Technical_Vision.md`: high-level technical intent
- `plans/RealImageTestPlan.md`: planned real-image verification work
- `Release_Process.md`: release procedure
- `Phase20_Fdc_Raw_Notes.md`: decision notes for controller-facing and raw-direction discussions
- `Phase21_Emulator_Host_Integration_Plan.md`: emulator-first host adapter planning for the next phase
- `Phase24_Real_Emulator_Integration_Plan.md`: real-host bridge planning for the next emulator-integration phase
- `Phase24_First_EventDriven_Host_Checklist.md`: minimum proof checklist for the first real event-driven host bridge
- `Phase24_First_EventDriven_Host_Bridge_Tasks.md`: bridge-side task list for the first real event-driven host proof
- `Phase24_First_Host_Proof_Report_Template.md`: report template for returning real-host proof results back to this repository
- `Glossary.md`: project terminology used across roadmap and migration work

## Reference Specifications

These files remain valuable as implementation references, even if they were written at different times and in different styles.

- `D88_Format.md`
- `2D_Format_Specification.md`
- `Hu-BASIC_Format.md`
- `HuBasic_Format_Specification.md`
- `N88Basic_Format.md`
- `MSX_DOS_Format.md`
- `FAT12_Format.md`
- `L89_Format_Specification.md`
- `CPM_Character_Encoding.md`
- `CPM_Implementation_Design.md`
- `N88Basic_vs_HuBasic_Analysis.md`
- `MFM_FM_Recording_Tutorial.md`

## Architecture and Future Work

- `TODO_BootInfo_Refactoring.md`

## Obsolete Documents

Documents that were no longer part of the active documentation set were moved to:

- `obsolete/2026-03-doc-audit/`

These files are kept temporarily for review and possible later deletion. They should not be treated as the source of truth for current behavior.

## Current Source of Truth

If a document disagrees with the current implementation, use these in this order:

1. current code and CLI help
2. `Agent_Handoff_Roadmap_V2.md`
3. `Roadmap_V2.md`
4. the relevant format specification
