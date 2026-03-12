# Professional CLI and Library Refactoring Plan

The goal is to transition the current diagnostic tools into a professional, distributed CLI tool (`l89disk`) and a reusable library (`Legacy89DiskKit.Core`).

## User Review Required

> [!IMPORTANT]
> **Native AOT Publishing**: We will use .NET Native AOT to produce single, standalone executable files for Windows (.exe), Linux, and macOS. This eliminates the need for the user to have the .NET SDK installed to run the tool.

## Proposed Architecture (Layered approach)

To ensure portability and Native AOT compatibility, we will reorganize into three distinct layers:

### 1. [Legacy89DiskKit.Core] (Domain + Logic)
- **High Integrity**: Pure logic for disk containers (D88/Raw), file systems (Hu-BASIC, N88, MSX), and character encoding.
- **Abstraction**: Define `IDiskContainer`, `IFileSystem`, and `IBootStrategy` interfaces.
- **No Side Effects**: Avoid `Console`, `OS Paths`, or `Reflection`. Use `Stream` and `ReadOnlyMemory<byte>` for data handling.
- **Native AOT Ready**: Avoid dynamic reflection and un-trimmable dependencies.

### 2. [Legacy89DiskKit.Application] (Service Layer)
- Provides service-oriented APIs for CLI and future GUIs.
- `DiskService`: Atomic operations (Write to `.tmp`, then rename).
- `ArchiveService`: Extract/Inject logic with manifest generation.

### 3. [Legacy89DiskKit.Cli] (Presentation)
- Modern CLI using `System.CommandLine`.
- **Global Config**: `--json`, `--verbose`, `--no-color`, `--force`, `--dry-run`.
- **Subcommands**:
  - `list`: Show file list. Use `--name-mode` (decoded/hex/raw).
  - `extract <image> <dest>`: Export files with manifest JSON.
  - `inject <image> <files...>`: Add files to image.
  - `boot <src> <dest>`: Create bootable clone (with auto-patching).
  - `format <type> <output>`: Create blank formatted image.

## Critical Considerations from Codex

- **Native AOT Early Validation**: Enable `PublishAot` and trimming in project files immediately to catch non-compatible code early.
- **Character Encoding**: 
  - Register `CodePagesEncodingProvider` for Shift-JIS support.
  - Handle original raw byte names separate from display names to prevent irreversible filename clobbering.
- **Atomic Operations**: Implement "write-then-rename" pattern for all image modifications to prevent corruption on crash.
- **Cross-Platform Distribution**:
  - Use GitHub Actions (CI) to build specific binaries for `osx-arm64`, `win-x64`, and `linux-x64`.
  - Account for dynamic linking issues on Linux (glibc versions).

## Phase 16: Character Encoding & Filename Normalization
Implement robust handling of legacy encodings and filename translation between host and disk.

### [Core] [MODIFY] [IFileSystem.cs](file:///Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Domain/FileSystem/Interface/FileSystem/IFileSystem.cs)
- Update `DiskFileSystemInfo` to include `DefaultEncodingId`.
- Ensure `FileEntry` provides both raw and decoded names if applicable.

### [Application] [NEW] [FileNameNormalizationService.cs](file:///Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Application/Services/FileNameNormalizationService.cs)
- Implement `IShortenName(string longName, int maxLength, bool allowExtension)` logic.
- Implement collision handling for batch operations (e.g., generating `FILE~1.TXT`).
- Integrate with `IEncoderRegistry` to validate byte lengths after encoding.

### [Cli] [MODIFY] [Program.cs](file:///Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Cli/Program.cs)
- Add global `--encoding` option to override FS-suggested encoding.
- Update `list` command to respect the chosen encoding.
- Ensure `inject` (copy-to-disk) uses the normalization service.

### Automated Tests
- Build native binaries using `dotnet publish -r osx-arm64 -c Release /p:PublishAot=true`.
- Verify the generated single-file executable works independently.

### Manual Verification
- Verify `l89disk boot <src> <dest>` produces the same high-quality 2HD boot image we just perfected.
