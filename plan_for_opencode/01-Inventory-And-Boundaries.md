# Task 01: Inventory And Boundaries

## Purpose

Create a precise, machine-checked inventory of what still uses legacy layer-first namespaces and define the allowed migration boundaries before changing code.

## Steps

1. Enumerate all remaining active-source occurrences of:
   - `namespace Legacy89DiskKit.Domain.*`
   - `namespace Legacy89DiskKit.Infrastructure.*`
   - `using Legacy89DiskKit.Domain.*`
   - `using Legacy89DiskKit.Infrastructure.*`
2. Exclude:
   - `obsolete/`
   - `bin/`
   - `obj/`
   - generated files
3. Group the results by responsibility:
   - CharacterEncoding
   - DiskImage
   - Drive
   - Fdc
   - FileSystem
   - Native
   - Timing
4. Identify any bootstrap/public-surface references that currently depend on the old names.
5. Produce a short implementation note in the agent report, not a new repository markdown document.

## Completion Criteria

- A complete active-source inventory exists in the working notes/report.
- The next tasks can be executed responsibility by responsibility without ambiguity.
- No production code is changed in this task unless needed for safe instrumentation.
