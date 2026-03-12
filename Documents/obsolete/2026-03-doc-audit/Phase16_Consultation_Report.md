# Phase 16 Consultation Report: Legacy Character Encoding & Filename Normalization

## 1. Current Status
The `l89disk` CLI is functional, but listing files in non-X1 images (like PC-88 or MSX) results in character corruption because it currently defaults to the X1 encoding (or fallback ASCII). Legacy 2HD/2D disks use Shift-JIS or platform-specific variant encodings.

## 2. Problem Statement
1. **Auto-Detection**: The CLI should ideally use the encoding associated with the detected file system (e.g., Hu-BASIC for X1 -> X1 encoding, N88-BASIC -> PC-88 encoding).
2. **User Override**: Users need an option to specify encoding (e.g., `--encoding shift-jis` or `--encoding pc88`).
3. **Filename Normalization (Inject)**: When copying files from modern OS (UTF-8, long filenames) to legacy disks (8.3, Shift-JIS, limited length), we need consistent rules for shortening and conversion.
4. **Name Conversion (Extract)**: When extracting to modern OS, illegal characters must be handled.

## 3. Proposed Technical Approach
- **Encoding Interface**: Enhance `EncoderRegistry` to be accessible from the `Application` layer.
- **FS-Aware Encoding**: Update `DiskFileSystemInfo` to include a `SuggestedEncodingId`.
- **Normalization Service**: Create a `FileNameNormalizationService` that:
  - Truncates lengths based on FS limits.
  - Generates unique names for collisions (e.g., `LONGFILE.TXT` -> `LONGFI~1.TXT`).
  - Converts characters to the target encoding, replacing unsupported ones with underscores.

## 4. Consultation Request
Please provide advice on:
1. **Shortening Rules**: Best practices for 8.3 or platform-specific (e.g., X1's 16-byte) filename shortening while maintaining uniquely identifiable names.
2. **Ambiguous Encodings**: How to handle platforms where multiple encodings might exist (e.g., standard Shift-JIS vs vendor-specific extensions).
3. **CLI UX**: How to present "raw bytes" vs "decoded names" when the decoding is potentially lossy or incorrect.
4. **Collision Handling**: Suggestions for handling name collisions during batch injection (e.g., `Document_A.txt` and `Document_B.txt` both becoming `DOCUME~1.TXT`).

**User Requirements**:
- Automatic encoding detection.
- Manual encoding override.
- Smart filename shortening for host -> disk injection.
