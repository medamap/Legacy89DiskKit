# Legacy Character Encoding & Filename Normalization

I have successfully implemented the infrastructure and CLI tools to handle legacy character encodings and filename constraints, ensuring that files can be transferred between modern systems and retro platforms (X1, PC-88, MSX) without character corruption or naming issues.

## Key Accomplishments

### 1. Robust Character Encoding Support
- **Dynamic Decoding**: The `list` command now supports a global `--encoding` (or `-e`) option. This allows users to override the default filesystem encoding (e.g., forcing Shift-JIS on a Hu-BASIC disk).
- **Raw Byte Preservation**: `FileEntry` now stores original filename bytes (`RawFileName`, `RawExtension`), allowing for accurate re-decoding without losing data to intermediary UTF-8 conversions.
- **Encoder Registry**: Centralized registry for retro-platform encoders (X1, PC88/Shift-JIS).

### 2. Intelligent Filename Normalization
- **Platform-Specific Constraints**: Implemented `FileNameNormalizationService` that understands:
    - **Hu-BASIC**: 13-character unified names.
    - **PC-88 (N88-BASIC)**: 6.3 naming format.
    - **MSX-DOS**: 8.3 naming format.
- **Safe Sanitization**: Automatically replaces illegal characters (spaces, special symbols) with underscores.
- **Collision Resolution**: Uses the classic `~1`, `~2` numbering strategy when multiple files map to the same shortened name.

### 3. File Injection CLI (`inject`)
- **Host-to-Disk Copy**: A new `inject` command allows copying individual files from the host to a disk image.
- **Auto-Adaptation**: The tool automatically detects the target platform, normalizes the filename, and chooses the correct character encoding and file attributes (ASCII/Binary heuristic).

## Verification Results

### Filename Shortening & Injection
Successfully injected a file with a long name into a Hu-BASIC disk image:
- **Source**: `test_file_with_long_name_and_japanese_文字.txt`
- **Result**: `test_file_wit` (Shortened to 13 bytes, sanitized).
- **Status**: Verified by listing the disk contents after injection.

### Encoding Override
Verified that the `--encoding` option changes how filenames are rendered in the `list` command, providing a way to recover filenames that would otherwise be corrupted by incorrect character set assumptions.

```bash
# Example list command with encoding override
dotnet run -- list images/test_inject.2D --encoding SJIS
```

## Next Steps & Future Plans

### Phase 17: Advanced File Listing
- **Symbolic Attribute Representation**: Move from raw hex to a character-based display (e.g., `PBAGSIH` for Password, Basic, Asc, Guard, System, IPL, Hidden), making it easier to read at a glance.
- **Platform-Specific Metadata**: 
    - **Hu-BASIC**: Display 16-bit Load, End, and Execution addresses in hexadecimal.
    - **General**: Ensure metadata relevancy changes dynamically based on the detected filesystem.
- **Filesystem-Aware Attribute Operations**: Refactor the attribute management to handle flags that vary significantly between retro platforms.

### Refined Batch Testing
- Implement broad batch testing for all supported file systems.
- Add support for deeper subdirectories (where applicable in MSX-DOS).
