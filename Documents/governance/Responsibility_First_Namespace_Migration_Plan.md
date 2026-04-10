# Responsibility-First Namespace Migration Plan

## Goal

Move from the current layer-first namespace convention:

- `Legacy89DiskKit.Application.FileSystem`
- `Legacy89DiskKit.Infrastructure.FileSystem.XDos`
- `Legacy89DiskKit.Domain.DiskImage.Model`

to a responsibility-first convention:

- `Legacy89DiskKit.FileSystem.Application`
- `Legacy89DiskKit.FileSystem.Infrastructure.XDos`
- `Legacy89DiskKit.DiskImage.Domain.Model`

without breaking the current `develop` branch.

## Constraints

- Do not attempt a repo-wide rename in one slice.
- Keep `develop` releasable after every slice.
- Start with the narrowest responsibility boundary that is already active in recent work.
- Run build and relevant tests after each slice.
- Do not mix new naming rules into only one new feature while leaving its immediate neighbors inconsistent.

## Current Boundary Inventory

The current layer-first top-level areas are:

- `CharacterEncoding`
- `DiskImage`
- `Drive`
- `Fdc`
- `FileSystem`
- `Native`
- `Services`
- `Timing`

Because these are widely referenced, the migration must be staged.

## Phase 0: Freeze and Naming Rules

Objective:

- Stop adding any new layer-first namespaces for new work.
- Treat responsibility-first as the target architecture rule from this point forward.

Exit criteria:

- This plan is accepted as the migration baseline.
- New work does not introduce additional layer-first namespace drift.

## Phase 1: FileSystem Read Models and Inspector Slice

Scope:

- File inspection read models
- Disk inspection read models
- CSV-facing file/disk inspection outputs
- Formatter-facing inspection DTOs only

Candidate files:

- `InspectionModels.cs`
- `DiskInspectionService.cs`
- `FileInspectionService.cs`
- related CLI formatter adapters only when required

Target convention:

- `Legacy89DiskKit.FileSystem.Application`
- `Legacy89DiskKit.FileSystem.Presentation`

Why this slice is safe:

- It is recent work.
- It has limited downstream usage compared to boot/clone/transfer services.
- It is mostly additive and read-oriented.

Exit criteria:

- Build succeeds.
- Inspection and CLI presentation tests succeed.
- No functional change beyond namespace/folder movement.

## Phase 2: FileSystem Operational Services

Scope:

- file transfer
- layout
- boot export/import
- clone orchestration
- filesystem registry/resolver

Candidate files:

- `FileTransferService.cs`
- `DirectoryLayoutService.cs`
- `BootEntryExportService.cs`
- `BootEntryImportService.cs`
- `DiskCloneService.cs`
- `FileSystemRegistry.cs`
- `ExplicitFileSystemResolver.cs`
- related DTOs/interfaces

Why this slice is separate:

- These services are used by CLI write flows and tests extensively.
- This is the first slice where write-path regressions become likely.

Exit criteria:

- Build succeeds.
- File transfer, boot, layout, clone, and CLI tests succeed.
- No behavior regression in existing images/test workflows.

## Phase 3: DiskImage Boundary

Scope:

- D88/raw container abstractions
- geometry models
- disk container factories
- raw/d88 infrastructure readers and writers

Target convention:

- `Legacy89DiskKit.DiskImage.Domain`
- `Legacy89DiskKit.DiskImage.Infrastructure`

Why later:

- This boundary is broad and heavily referenced by FileSystem and Drive.

Exit criteria:

- Build succeeds.
- Disk image and container tests succeed.

## Phase 4: Drive and Fdc Boundaries

Scope:

- mounted medium
- drive state
- FDC controller abstractions
- controller-facing media

Why later:

- This is a lower-level subsystem and mistakes here can ripple broadly.

Exit criteria:

- Build succeeds.
- Drive/FDC tests succeed.

## Phase 5: CharacterEncoding and Timing

Scope:

- encoders
- machine/encoding profiles
- timing abstractions

Why later:

- These are cross-cutting dependencies and should be moved after the major filesystem/disk boundaries settle.

Exit criteria:

- Build succeeds.
- Encoding-sensitive tests succeed.

## Phase 6: Native and Cross-Boundary Cleanup

Scope:

- native bridge naming alignment
- remaining `Services` bucket cleanup
- remove obsolete namespace aliases/usings

Why last:

- Native integration and the generic `Services` bucket are both cleanup-heavy and depend on earlier slices stabilizing first.

Exit criteria:

- Build succeeds.
- Managed tests pass.
- Native validation is either passing or explicitly documented as temporarily out of sync.

## Recommended Execution Order

1. Phase 0
2. Phase 1
3. Phase 2
4. Phase 3
5. Phase 4
6. Phase 5
7. Phase 6

## Stop Conditions

Stop the migration immediately if:

- a slice requires cross-boundary repo-wide rename beyond its declared scope
- public CLI behavior changes unintentionally
- the test blast radius becomes larger than the current slice boundary
- generated/release-facing artifacts would need simultaneous changes across unrelated boundaries

When a stop condition occurs:

- commit or stash the slice-local work
- document the blocker
- split the slice further before continuing
