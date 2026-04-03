# Platform Support Status

## Purpose

This document summarizes the current public support status by platform family, filesystem family, and workflow type.

It is intentionally release-facing and should stay conservative.

## Sharp X1

### Hu-BASIC

- listing: supported
- file read: supported
- file write: supported
- format: supported
- boot metadata inspection: supported
- boot export/import surface: supported
- same-filesystem logical copy: supported
- emulator-confirmed bootable copies:
  - `2D`
  - `2DD`
  - `2HD`

### X-DOS

- listing: supported
- file read: supported
- file write: supported
- format: supported
- boot metadata inspection: supported
- bootable clone workflow: supported
- emulator-confirmed bootable copies:
  - `2D`
  - `2DD`
  - `2HD`
- post-boot utility workflow parity:
  - still under manual verification

### CP/M

- listing: supported
- file read: supported
- write-side behavior:
  - limited
  - not yet positioned as a general public write workflow
- emulator-confirmed bootable copy:
  - `2D`
- `2DD` and `2HD`:
  - not yet confirmed
  - currently treated as tentative

## PC-8801

### N88-BASIC

- listing: supported
- file read: supported
- file write: supported
- format: supported
- boot metadata summary: supported

## MSX

### MSX-DOS

- listing: supported
- file read: supported
- file write: supported
- format: supported
- boot metadata summary: supported

## Native Bridge

- documented C ABI: supported
- host-platform verification: supported
- multi-platform release parity:
  - not yet the release gate

## Notes

- "supported" means the current repository contains intended behavior with tests or direct validation.
- "tentative" means behavior exists but is not yet confirmed as a stable public workflow.
- bootability results reflect the current direct emulator validation track and may still be refined before release.
