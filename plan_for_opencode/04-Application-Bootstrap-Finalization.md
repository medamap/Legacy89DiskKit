# Task 04: Application Bootstrap Finalization

## Purpose

Decide and implement the final handling of the remaining `Legacy89DiskKit.Application` bootstrap/public surface.

## Required Work

1. Identify what `Legacy89DiskKit.Application` still provides.
2. Separate:
   - compatibility/bootstrap behavior worth keeping
   - accidental leftovers that should be migrated away
3. Choose one of these outcomes:
   - keep a thin, intentional bootstrap surface
   - migrate all remaining usages away and retire it
4. Update `Cli`, `Tests`, `NativeInterop`, and helper tools accordingly.
5. Remove any accidental dependency on legacy layer-first namespaces left in application-facing entrypoints.

## Important Constraint

This task must not leave the repository with an unclear hybrid state. Either:

- `Legacy89DiskKit.Application` remains intentionally as the public bootstrap facade, or
- it is fully retired from active use.

## Completion Criteria

- The status of `Legacy89DiskKit.Application` is intentional and defensible.
- Active code no longer depends on stray layer-first names by accident.
- Build and tests pass after finalization.
