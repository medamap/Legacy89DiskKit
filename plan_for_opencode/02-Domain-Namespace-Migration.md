# Task 02: Domain Namespace Migration

## Purpose

Migrate active code from legacy layer-first `Legacy89DiskKit.Domain.*` namespaces to responsibility-first `Legacy89DiskKit.<Responsibility>.Domain.*` namespaces.

## Scope

- Active code only
- Excludes `obsolete/`
- Excludes generated files

## Required Work

1. Update domain source files so that their declared namespaces become responsibility-first.
2. Update all active `using` statements and fully-qualified references to match.
3. Keep project references minimal and correct.
4. Apply the migration responsibility by responsibility in safe slices.
5. Run build/tests at reasonable milestones, not necessarily after every tiny slice.

## Suggested Slice Order

1. CharacterEncoding.Domain
2. DiskImage.Domain
3. Drive.Domain
4. Fdc.Domain
5. Timing.Domain
6. FileSystem.Domain
7. Native.Domain

## Completion Criteria

- No active-source `namespace Legacy89DiskKit.Domain.*` declarations remain.
- No active-source `using Legacy89DiskKit.Domain.*` remains unless intentionally preserved in the final bootstrap boundary and explicitly justified.
- The repository still builds after the task is complete.
