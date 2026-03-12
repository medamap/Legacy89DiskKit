# Codex Consultation: Phase 8 - Application Layer Optimization

## Objective
Implement a Registry system in the Application layer to allow dynamic discovery and management of File Systems and Character Encoders. This refactors the current hardcoded logic in `DiskService` and improves extensibility.

## Context
- **User Requirement**: Support various vintage systems (Hu-BASIC, PC-88, MSX, CP/M, etc.) and their specific file system formats and character encodings.
- **Current State**: `DiskService` has some hardcoded logic for detecting file systems. Multiple placeholder file systems and encoders are being added.
- **Goal**: Provide a central way to register and lookup `IFileSystem` implementations and `ICharacterEncoder` implementations.

## Proposed Components

### 1. `FileSystemRegistry`
- **Responsibility**: Map file system signatures (from boot areas) or IDs to `IFileSystem` factory methods.
- **Interface**:
  ```csharp
  public interface IFileSystemRegistry {
      void Register(string signature, Func<IDiskContainer, IFileSystem> factory);
      IFileSystem Resolve(IDiskContainer container);
  }
  ```

### 2. `EncoderRegistry`
- **Responsibility**: Map platform IDs (e.g., "X1", "PC88", "MSX") to `ICharacterEncoder` instances.
- **Interface**:
  ```csharp
  public interface IEncoderRegistry {
      void Register(string platformId, ICharacterEncoder encoder);
      ICharacterEncoder GetEncoder(string platformId);
  }
  ```

### 3. Refactoring `DiskService`
- Use `IFileSystemRegistry` to detect and instantiate file systems instead of switch statements or if/else checks.
- Potentially use `IEncoderRegistry` to provide the default encoder for a detected file system.

## Specific Questions for Codex
1. **Signature Collision**: In vintage systems, boot area signatures might be minimal or proprietary. How should the registry handle overlapping signatures or systems with NO signature?
2. **Resource Management**: The `IFileSystem` instances are `IDisposable`. Should the Registry or the Service track their lifetime?
3. **WASM/Native Considerations**: As this will be exported via Native AOT to C/WASM, are there any patterns in the Registry implementation (e.g., reflection vs. manual registration) that I should avoid?
4. **Platform vs. FileSystem**: One platform (e.g., MSX) might have multiple file systems (MSX-DOS, MSX-BASIC). How should the registry structure reflect this hierarchy?

## User's Core Intent
"Hu-BASIC is the priority for now, but the system must be DDD-based and extensible to any file system. Distinguish between abstraction and implementation."
