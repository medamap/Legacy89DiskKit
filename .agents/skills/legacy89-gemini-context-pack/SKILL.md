---
name: legacy89-gemini-context-pack
description: Load the shared Legacy89DiskKit repository context for Gemini handoff work, including repo rules, sample disk images, document index, and verification expectations. Use when writing instructions, implementing tasks, or reviewing reports for the Gemini queue in this repository.
---

# Legacy89 Gemini Context Pack

Load the shared context files for any Gemini queue task in this repository.

## Load Order

1. [communication/legacy89_context.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md)
2. [communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
3. [communication/verification_baseline.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md)
4. [communication/sample_images.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/sample_images.md)
5. [communication/document_index.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/document_index.md)

## Use This Context To

- keep Gemini aligned with AGENTS.md
- keep work C# first
- keep work 2D first
- avoid accidental Codex-side implementation
- use the correct sample disks and output locations
- use the correct verification commands

## References

- Use [references/context-summary.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/legacy89-gemini-context-pack/references/context-summary.md) for the short checklist
