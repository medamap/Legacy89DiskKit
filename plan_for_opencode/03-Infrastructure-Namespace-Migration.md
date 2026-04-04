# Task 03: Infrastructure Namespace Migration

## Purpose

Migrate active code from legacy layer-first `Legacy89DiskKit.Infrastructure.*` namespaces to responsibility-first `Legacy89DiskKit.<Responsibility>.Infrastructure.*` namespaces.

## Scope

- Active code only
- Excludes `obsolete/`
- Excludes generated files

## Required Work

1. Update infrastructure source files so that their declared namespaces become responsibility-first.
2. Update all active `using` statements and fully-qualified references to match.
3. Ensure cross-responsibility references remain explicit and minimal.
4. Keep filesystem-specific parsing inside infrastructure.
5. Verify buildability at milestone boundaries.

## Suggested Slice Order

1. CharacterEncoding.Infrastructure
2. DiskImage.Infrastructure
3. Drive.Infrastructure
4. Fdc.Infrastructure
5. FileSystem.Infrastructure

## Completion Criteria

- No active-source `namespace Legacy89DiskKit.Infrastructure.*` declarations remain.
- No active-source `using Legacy89DiskKit.Infrastructure.*` remains unless intentionally preserved in a final compatibility boundary and explicitly justified.
- The repository still builds after the task is complete.
