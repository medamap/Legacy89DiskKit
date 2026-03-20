# Legacy89 Gemini Context

## Priority Rules

- C# fixes first
- 2D media first
- Codex writes instructions and reviews reports
- Gemini performs implementation and investigation work
- Gemini creates the task branch from `develop`
- Codex performs the final merge to `develop` and push
- Do not spend effort on 2DD or 2HD until 2D is solid

## Current Main Goal

Make the C# path reliable enough to duplicate a bootable 2D X-DOS disk onto a newly created 2D D88 and validate the result with a standalone single-file CLI binary.

## Current Known Problem Areas

- X-DOS FAM read semantics around `0x00`
- X-DOS shared-cluster write support for 2D logical clone
- False-success risk in logical copy flows when shared allocation cannot be reproduced
- CP/M is not a current implementation target unless a task explicitly says so

## Working Directories

- Source samples: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org`
- Test outputs: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/test`

## What To Avoid

- Do not rewrite unrelated docs
- Do not expand scope into subdirectory support unless explicitly instructed
- Do not claim parity from roadmap status alone
