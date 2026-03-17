# Agent Guide

## Project Summary

Legacy89DiskKit is a C#-based retro disk toolkit for Japanese computer disk images and filesystems from the 1980s and 1990s.

The current product direction is:

- `Legacy89DiskKit.Cli`: primary shipped end-user artifact
- `Legacy89DiskKit.CSharp`: supported managed integration surface
- `Legacy89DiskKit.Native`: planned native library line
- `Legacy89DiskKit.Wasm`: planned browser/runtime line

## Source of Truth

When documents disagree, use this order:

1. current code and CLI help
2. `Documents/Agent_Handoff_Roadmap_V2.md` for the active C# to C++ migration track
3. `Documents/Roadmap_V2.md`
4. `Documents/ROADMAP.md`
5. the relevant format specification under `Documents/`

Useful documents:

- Release process: `Documents/Release_Process.md`
- Roadmap V2 handoff: `Documents/Agent_Handoff_Roadmap_V2.md`
- Roadmap V2: `Documents/Roadmap_V2.md`
- C++ DDD folder rulebook: `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`
- Roadmap: `Documents/ROADMAP.md`
- Document index: `Documents/Folder.md`

## Current Build and Test Commands

```bash
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false
cmake -S Cpp -B /tmp/legacy89-cpp-build
cmake --build /tmp/legacy89-cpp-build
ctest --test-dir /tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp --output-on-failure
```

Standalone CLI release automation:

```bash
./scripts/release-cli.sh 2.0.0
```

## Architecture

- DDD-oriented structure
- `Domain / Application / Infrastructure` layering
- CLI presentation logic should stay under the CLI project
- Filesystem-specific raw parsing belongs in infrastructure
- UI formatting must not be mixed into filesystem parsing code
- The active C# to C++ migration track is managed through `Documents/Roadmap_V2.md`
- When adding new C++ files, prefer the DDD-oriented folder layout defined in `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`

## Coding Rules

- Do not add code comments unless explicitly requested.
- Write exception messages in English.
- Use PascalCase for file names and public type names.
- Keep namespaces consistent with the current project structure.
- Prefer explicit filesystem selection for write and format flows.
- Keep encoding behavior platform-specific and explicit.

## Git Rules

- Commit messages should be written in Japanese.
- Use `--no-ff` merges when performing branch merges.
- Use `Co-Authored-By: Codex <Codex-ai@anthropic.invalid>` when requested.
- Do not commit unrelated local files.
- For Roadmap V2 work, branch from `develop` and use the `codex/` prefix.

## Testing Rules

- Run relevant tests before and after changes.
- Add tests for new behavior.
- Keep test file names in the `{FeatureName}Test.cs` style.

## Documentation Rules

- Do not create or rewrite markdown documents unless the user asks for it.
- When documentation changes are requested, keep document bodies in English unless the user says otherwise.
- Keep `README.md` compact and release-facing.
- Keep the current C# to C++ migration backlog in `Documents/Roadmap_V2.md`.

## Release Notes

- Use semantic versioning.
- Treat `v2.0.0` as the packaging and product-boundary milestone.
- The current CLI release baseline is self-contained single-file publishing.
- GitHub Actions is deferred for now; local release automation is the current source of truth.

## Practical Debug Tips

- Inspect disk images with `hexdump -C`.
- Check sector-level behavior through `IDiskContainer.ReadSector()`.
- Check detection behavior through the filesystem factory and CLI help/output.

## Project-Local Review Skill

- `legacy89-v2-review`: `.agents/skills/legacy89-v2-review/SKILL.md`
  - Use for Roadmap V2 phase reviews, especially native bridge, infrastructure, and C# to C++ parity changes.
